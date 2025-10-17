using System;
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

            FsmInt healValue = bind.GetIntVariable("Heal Amount");
            FsmInt healAmount = bind.GetIntVariable("Bind Amount");
            FsmFloat healTime = bind.GetFloatVariable("Bind Time");

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
