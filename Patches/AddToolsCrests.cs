using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Needleforge.Makers;
using Silksong.FsmUtil;
using Silksong.FsmUtil.Actions;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    internal class AddToolsCrests
    {
        public static Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM> defaultBind = (value, amount, time, fsm) =>
        {
            value.Value = 3;
            amount.Value = 1;
            time.Value = 1.2f;
        };
        public static Dictionary<string, Func<bool>> directionGet = new()
        {
            { "Up", () => HeroController.instance.inputHandler.inputActions.Up.IsPressed },
            { "Down", () => HeroController.instance.inputHandler.inputActions.Down.IsPressed },
            { "Left", () => HeroController.instance.inputHandler.inputActions.Left.IsPressed },
            { "Right", () => HeroController.instance.inputHandler.inputActions.Right.IsPressed }
        };

        [HarmonyPostfix]
        public static void AddCrests(HeroController __instance)
        {
            ModHelper.Log("Adding Crests...");
            foreach (CrestData data in NeedleforgePlugin.newCrestData)
            {
                CrestMaker.CreateCrest(data.RealSprite, data.Silhouette, data.AttackConfig, data.slots, data.name);
            }
            PlayMakerFSM bind = __instance.gameObject.GetFsmPreprocessed("Bind");
            FsmState CanBind = bind.GetState("Can Bind?");

            FsmState BindType = bind.GetState("Bind Type");

            FsmState QuickBind = bind.GetState("Quick Bind?");

            FsmState QuickCraft = bind.GetState("Quick Craft?");
            FsmState UseReserve = bind.GetState("Use Reserve Bind?");
            FsmState ReserveBurst = bind.GetState("Reserve Bind Burst");

            FsmInt healValue = bind.GetIntVariable("Heal Amount");
            FsmInt healAmount = bind.GetIntVariable("Bind Amount");
            FsmFloat healTime = bind.GetFloatVariable("Bind Time");

            FsmState whichCrest = bind.AddState("Which Crest?");
            whichCrest.AddTransition("Toolmaster", "Quick Craft?");
            whichCrest.AddLambdaMethod(finish =>
            {
                if (!NeedleforgePlugin.newCrests.Any(crest => crest.name == PlayerData.instance.CurrentCrestID))
                {
                    bind.SendEvent("Toolmaster");
                }
                finish.Invoke();
            });
            UseReserve.ChangeTransition("FALSE", "Which Crest?");
            ReserveBurst.ChangeTransition("FINISHED", "Which Crest?");
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
                    FsmEvent specialBindTransition = whichCrest.AddTransition($"{crest.name} Special", $"{crest.name} Special Bind?");

                    whichCrest.AddLambdaMethod(finish =>
                    {
                        if (crest.IsEquipped)
                        {
                            bind.SendEvent($"{crest.name} Special");
                            ModHelper.Log($"{crest.name} special trigger, moving to ${crest.name} special bind?");
                        }
                        ModHelper.Log($"{crest.name} called which crest");
                        finish.Invoke();
                    });


                    specialBindCheck.AddTransition("FALSE", "Bind Bell?");
                    specialBindCheck.AddTransition("TRUE", $"{crest.name} Special Bind Trigger");
                    specialBindCheck.AddLambdaMethod(finish =>
                    {
                        bind.SendEvent(directionGet[bindData.Direction]() ? "TRUE" : "FALSE");
                        finish.Invoke();
                    });

                    specialBindTrigger.AddTransition("FINISHED", "End Bind");
                    specialBindTrigger.AddLambdaMethod(bindData.lambdaMethod);
                }
            }

            DelegateAction<Action> replaceSilkCost = new()
            {
                Method = (action) =>
                {
                    FsmInt silkCost = bind.GetIntVariable("Current Silk Cost");
                    FsmBool witchEquipped = bind.GetBoolVariable("Is Witch Equipped");
                    bool unset = true;

                    if (witchEquipped.Value == true)
                    {
                        silkCost.Value = 0;
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

        [HarmonyPostfix]
        public static void AddTools(HeroController __instance)
        {
            ModHelper.Log("Adding Tools...");
            foreach (ToolData data in NeedleforgePlugin.newToolData)
            {
                ModHelper.Log($"Adding {data.name}");
                ToolMaker.CreateBasicTool(data.inventorySprite, data.type, data.name);
            }
        }
    }
}
