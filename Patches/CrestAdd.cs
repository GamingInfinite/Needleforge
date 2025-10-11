using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Needleforge.Makers;
using Silksong.FsmUtil;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
    internal class CrestAdd
    {
        public static Action<FsmInt, FsmInt, FsmFloat> defaultBind = (value, amount, time) =>
        {
            value.Value = 3;
            amount.Value = 1;
            time.Value = 1.2f;
        };

        [HarmonyPostfix]
        public static void Postfix(HeroController __instance)
        {
            foreach(CrestData data in NeedleforgePlugin.newCrestData)
            {
                CrestMaker.CreateCrest(data.RealSprite, data.Silhouette, data.name);
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
                    defaultBind.Invoke(healValue, healAmount, healTime);
                    NeedleforgePlugin.bindEvents[crest.name].Invoke(healValue, healAmount, healTime);
                    bind.SendEvent("FINISHED");
                });
            }
        }
    }
}
