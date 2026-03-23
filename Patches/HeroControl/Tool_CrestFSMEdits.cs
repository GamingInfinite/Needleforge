using System;
using System.Collections.Generic;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;
using BindEventHandler = Needleforge.Data.CrestData.BindEventHandler;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class Tool_CrestFSMEdits
{
    private static readonly BindEventHandler defaultBind = (value, amount, time, fsm) =>
    {
        value.Value = 3;
        amount.Value = 1;
        time.Value = 1.2f;
    };

    private static readonly Dictionary<UniqueBindDirection, Func<bool>> directionGet = new()
    {
        { UniqueBindDirection.UP, () => HeroInput.Up.IsPressed },
        { UniqueBindDirection.DOWN, () => HeroInput.Down.IsPressed },
        { UniqueBindDirection.LEFT, () => HeroInput.Left.IsPressed },
        { UniqueBindDirection.RIGHT, () => HeroInput.Right.IsPressed }
    };
    private static HeroActions HeroInput => HeroController.instance.inputHandler.inputActions;

	[HarmonyPrefix]
    private static void AddCrests(HeroController __instance)
    {
        PlayMakerFSM bind = __instance.gameObject.GetFsmPreprocessed("Bind")!;
        FsmState
            CanBind = bind.GetState("Can Bind?")!,
            BindType = bind.GetState("Bind Type")!,
            QuickBind = bind.GetState("Quick Bind?")!,
            BindBell = bind.GetState("Bind Bell?")!,
            EndBind = bind.GetState("End Bind")!,
            QuickCraft = bind.GetState("Quick Craft?")!,
            UseReserve = bind.GetState("Use Reserve Bind?")!,
            ReserveBurst = bind.GetState("Reserve Bind Burst")!;

        FsmInt healValue = bind.GetIntVariable("Heal Amount");
        FsmInt healAmount = bind.GetIntVariable("Bind Amount");
        FsmFloat healTime = bind.GetFloatVariable("Bind Time");

        FsmState whichCrest = bind.AddState("Which Crest?");
        whichCrest.AddTransition("Toolmaster", QuickCraft.name);
        whichCrest.AddMethod(() =>
        {
            if (!NeedleforgePlugin.uniqueBind.ContainsKey(PlayerData.instance.CurrentCrestID))
            {
                bind.SendEvent("Toolmaster");
            }
        });
        UseReserve.ChangeTransition("FALSE", whichCrest.name);
        ReserveBurst.ChangeTransition("FINISHED", whichCrest.name);
        foreach (ToolCrest crest in NeedleforgePlugin.newCrests)
        {
            FsmBool equipped = bind.AddBoolVariable($"Is {crest.name} Equipped");
            CanBind.AddAction(new CheckIfCrestEquipped
            {
                Crest = crest,
                storeValue = equipped
            });

            FsmState newBindState = bind.AddState($"{crest.name} Bind");
            FsmEvent newBindTransition = BindType.AddTransition($"{crest.name}", newBindState.name);

            BindType.AddAction(new BoolTest
            {
                boolVariable = equipped,
                isTrue = newBindTransition,
                everyFrame = false
            });

            newBindState.AddTransition("FINISHED", QuickBind.name);

            newBindState.AddMethod(() =>
            {
                defaultBind.Invoke(healValue, healAmount, healTime, bind);
                NeedleforgePlugin.bindEvents[crest.name].Invoke(healValue, healAmount, healTime, bind);
            });
            
            if (NeedleforgePlugin.uniqueBind.TryGetValue(crest.name, out UniqueBindEvent bindData))
            {
                FsmState specialBindCheck = bind.AddState($"{crest.name} Special Bind?");
                FsmState specialBindTrigger = bind.AddState($"{crest.name} Special Bind Trigger");
                FsmEvent specialBindTransition = whichCrest.AddTransition($"{crest.name} Special", specialBindCheck.name);

                whichCrest.AddMethod(() =>
                {
                    if (crest.IsEquipped)
                    {
                        bind.SendEvent($"{crest.name} Special");
                    }
                });

                specialBindCheck.AddTransition("FALSE", BindBell.name);
                specialBindCheck.AddTransition("TRUE", specialBindTrigger.name);
                specialBindCheck.AddMethod(() =>
                {
                    bind.SendEvent(directionGet[bindData.Direction]() ? "TRUE" : "FALSE");
                });

                specialBindTrigger.AddTransition("FINISHED", EndBind.name);
                specialBindTrigger.AddLambdaMethod(bindData.lambdaMethod);
            }
        }

        DelegateAction<Action> replaceSilkCost = new()
        {
            Method = (action) =>
            {
                FsmInt silkCost = bind.GetIntVariable("Current Silk Cost");
                FsmInt witchSilkCost = bind.GetIntVariable("Silk Cost Witch");
                FsmBool witchEquipped = bind.GetBoolVariable("Is Witch Equipped");
                bool unset = true;

                if (witchEquipped.Value == true)
                {
                    silkCost.Value = witchSilkCost.Value;
                }
                else
                {
                    foreach (var crest in NeedleforgePlugin.newCrestData)
                    {
                        if (crest.IsEquipped)
                        {
                            silkCost.Value = crest.bindCost;
                            unset = false;
                        }
                    }
                    if (unset)
                    {
                        silkCost.Value = 9;
                    }
                }

                action.Invoke();
            }
        };
        replaceSilkCost.Arg = replaceSilkCost.Finish;
        // bit better compatibility with other mods that edit this fsm
        int silkCostIdx = Array.FindIndex(
            CanBind.Actions,
            x => x is ConvertBoolToInt y && y.intVariable.Name == "Current Silk Cost"
        );
        CanBind.ReplaceAction(silkCostIdx, replaceSilkCost);
    }

    [HarmonyPrefix]
    private static void AddTools(HeroController __instance)
    {

        PlayMakerFSM toolEvents = __instance.toolEventTarget;
        FsmState toolChoiceState = toolEvents.GetState("Tool Choice")!;

        foreach (ToolData newTool in NeedleforgePlugin.newToolData)
        {
            if (newTool is LiquidToolData liquidTool)
            {
                FsmState newToolState = toolEvents.AddState($"{liquidTool.name} ANIM");

                var animator = __instance.GetComponent<tk2dSpriteAnimator>();
                string clipName = liquidTool.clip;

                newToolState.AddLambdaMethod((finish) =>
                {
                    animator.Play(clipName);

                    void FinishEventThenRemove(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip)
                    {
                        if (clip.name != clipName)
                        {
                            return;
                        }
                        NeedleforgePlugin.toolEventHooks[$"{newTool.name} AFTER ANIM"].Invoke();
                        animator.AnimationCompleted -= FinishEventThenRemove;
                        finish.Invoke();
                    }

                    animator.AnimationCompleted += FinishEventThenRemove;
                    NeedleforgePlugin.toolEventHooks[$"{newTool.name} BEFORE ANIM"].Invoke();
                });

                FsmEvent toolChoiceTrans = toolChoiceState.AddTransition($"{liquidTool.name}", newToolState.name);
                toolEvents.FsmTemplate.fsm.Events = [.. toolEvents.Fsm.Events, toolChoiceTrans];

                newToolState.AddTransition("FINISHED", "Return Control");
            }
        }
    }
}
