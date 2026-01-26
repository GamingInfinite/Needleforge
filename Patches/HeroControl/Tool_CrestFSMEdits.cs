using System;
using System.Collections.Generic;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class Tool_CrestFSMEdits
{
    private static readonly Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM> defaultBind = (value, amount, time, fsm) =>
    {
        value.Value = 3;
        amount.Value = 1;
        time.Value = 1.2f;
    };
    private static readonly Dictionary<UniqueBindDirection, Func<bool>> directionGet = new()
    {
        { UniqueBindDirection.UP, () => HeroController.instance.inputHandler.inputActions.Up.IsPressed },
        { UniqueBindDirection.DOWN, () => HeroController.instance.inputHandler.inputActions.Down.IsPressed },
        { UniqueBindDirection.LEFT, () => HeroController.instance.inputHandler.inputActions.Left.IsPressed },
        { UniqueBindDirection.RIGHT, () => HeroController.instance.inputHandler.inputActions.Right.IsPressed }
    };

    [HarmonyPrefix]
    private static void AddCrests(HeroController __instance)
    {
        // Finding it this way until Needleforge bumps its FsmUtil version because a
        // bug was discovered in GetFsmPreprocessed that's present on our current
        // minimum version of it
        PlayMakerFSM bind = __instance.gameObject.LocateMyFSM("Bind");
        if (!bind.Fsm.preprocessed)
            bind.Preprocess();

        FsmState CanBind = bind.GetState("Can Bind?");

        FsmState BindType = bind.GetState("Bind Type");

        FsmState QuickBind = bind.GetState("Quick Bind?");

        FsmState BindBell = bind.GetState("Bind Bell?");

        FsmState EndBind = bind.GetState("End Bind");

        FsmState QuickCraft = bind.GetState("Quick Craft?");
        FsmState UseReserve = bind.GetState("Use Reserve Bind?");
        FsmState ReserveBurst = bind.GetState("Reserve Bind Burst");

        FsmInt healValue = bind.GetIntVariable("Heal Amount");
        FsmInt healAmount = bind.GetIntVariable("Bind Amount");
        FsmFloat healTime = bind.GetFloatVariable("Bind Time");

        FsmState whichCrest = bind.AddState("Which Crest?");
        whichCrest.AddTransition("Toolmaster", QuickCraft.name);
        whichCrest.AddLambdaMethod(finish =>
        {
            if (!NeedleforgePlugin.uniqueBind.ContainsKey(PlayerData.instance.CurrentCrestID))
            {
                bind.SendEvent("Toolmaster");
            }
            finish.Invoke();
        });
        UseReserve.ChangeTransition("FALSE", whichCrest.name);
        ReserveBurst.ChangeTransition("FINISHED", whichCrest.name);
        foreach (ToolCrest crest in NeedleforgePlugin.newCrests)
        {
            FsmBool equipped = bind.AddBoolVariable($"Is {crest.name} Equipped");
            CanBind.AddAction(new CheckIfCrestEquipped()
            {
                Crest = crest,
                storeValue = equipped
            });

            FsmState newBindState = bind.AddState($"{crest.name} Bind");
            FsmEvent newBindTransition = BindType.AddTransition($"{crest.name}", newBindState.name);

            BindType.AddAction(new BoolTest()
            {
                boolVariable = equipped,
                isTrue = newBindTransition,
                everyFrame = false
            });

            newBindState.AddTransition("FINISHED", QuickBind.name);

            newBindState.AddLambdaMethod((action) =>
            {
                defaultBind.Invoke(healValue, healAmount, healTime, bind);
                NeedleforgePlugin.bindEvents[crest.name].Invoke(healValue, healAmount, healTime, bind);
                bind.SendEvent("FINISHED");
            });
            
            if (NeedleforgePlugin.uniqueBind.ContainsKey(crest.name))
            {
                var bindData = NeedleforgePlugin.uniqueBind[crest.name];
                FsmState specialBindCheck = bind.AddState($"{crest.name} Special Bind?");
                FsmState specialBindTrigger = bind.AddState($"{crest.name} Special Bind Trigger");
                FsmEvent specialBindTransition = whichCrest.AddTransition($"{crest.name} Special", specialBindCheck.name);

                whichCrest.AddLambdaMethod(finish =>
                {
                    if (crest.IsEquipped)
                    {
                        bind.SendEvent($"{crest.name} Special");
                    }
                    finish.Invoke();
                });

                specialBindCheck.AddTransition("FALSE", BindBell.name);
                specialBindCheck.AddTransition("TRUE", specialBindTrigger.name);
                specialBindCheck.AddLambdaMethod(finish =>
                {
                    bind.SendEvent(directionGet[bindData.Direction]() ? "TRUE" : "FALSE");
                    finish.Invoke();
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
        CanBind.ReplaceAction(9, replaceSilkCost);
    }

    [HarmonyPrefix]
    private static void AddTools(HeroController __instance)
    {

        PlayMakerFSM toolEvents = __instance.toolEventTarget;
        FsmState toolChoiceState = toolEvents.GetState("Tool Choice");

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
