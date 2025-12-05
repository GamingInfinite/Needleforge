using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Silksong.FsmUtil;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;
using Debug = UnityEngine.Debug;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class MovesetFSMEdits
{
    [HarmonyPostfix]
    private static void AddCustomDownslashes(HeroController __instance)
    {
        IEnumerable<MovesetData>
            movesets = NeedleforgePlugin.newCrestData
                .Select(cd => cd.Moveset)
            .Where(m => m.HeroConfig && m.HeroConfig.DownSlashType == DownSlashTypes.Custom);

        if (!movesets.Any())
            return;

        PlayMakerFSM fsm = __instance.crestAttacksFSM;
        fsm.Preprocess();

        FsmState
            Idle = fsm.GetState("Idle")!,
            End = fsm.GetState("End")!;

        foreach(MovesetData m in movesets)
        {
            string name = m.Crest.name;
            var fsmEdit = m.HeroConfig!.DownSlashFsmEdit;

            if (fsmEdit == null)
            {
                ModHelper.LogError(
                    $"Crest {name} has a custom downslash type, but doesn't define " +
                    $"a {nameof(HeroConfigNeedleforge.DownSlashFsmEdit)} function."
                );
                continue;
            }
            if (string.IsNullOrWhiteSpace(m.HeroConfig!.downSlashEvent))
            {
                ModHelper.LogError(
                    $"Crest {name} has a custom downslash type, but doesn't have a " +
                    $"valid {nameof(HeroControllerConfig.downSlashEvent)}."
                );
                continue;
            }

            FsmState AtkAntic = fsm.AddState($"{name} Downslash Antic");

            fsmEdit.Invoke(fsm, AtkAntic, out FsmState AtkEnd, out FsmState AtkHit);

            Idle.AddTransition(m.HeroConfig!.downSlashEvent, AtkAntic.Name);
            AtkEnd.AddTransition("FINISHED", End.Name);
            AtkHit.AddTransition("FINISHED", End.Name);
        }
    }

    [HarmonyPostfix]
    private static void AddCustomDashSlashes(HeroController __instance) {
        IEnumerable<MovesetData>
            movesets = NeedleforgePlugin.newCrestData
                .Select(cd => cd.Moveset)
                .Where(m => m.HeroConfig && m.HeroConfig.DashSlashFsmEdit != null);

        if (!movesets.Any())
            return;

        PlayMakerFSM fsm = __instance.sprintFSM;
        fsm.Preprocess();

        FsmState
            StartAttack = fsm.GetState("Start Attack")!,
            RegainControlNormal = fsm.GetState("Regain Control Normal")!;

        int crestCheckIndex = Array.FindLastIndex(StartAttack.Actions, x => x is CheckIfCrestEquipped);


        foreach(MovesetData m in movesets) {
            string name = m.Crest.name;
            var fsmEdit = m.HeroConfig!.DashSlashFsmEdit!;

            FsmState Antic = fsm.AddState($"{name} Antic");
            fsmEdit.Invoke(fsm, Antic, out FsmState AtkEnd, out FsmState AtkHit);

            StartAttack.InsertAction(crestCheckIndex, GetCrestEquippedAction(m.Crest));

            StartAttack.AddTransition(name, Antic.name);
            AtkEnd.AddTransition("FINISHED", RegainControlNormal.Name);
            AtkHit.AddTransition("FINISHED", RegainControlNormal.Name);
        }
    }


    private static readonly FsmEvent noEvent = FsmEvent.GetFsmEvent("");

    private static CheckIfCrestEquipped GetCrestEquippedAction(CrestData crest) =>
        new()
        {
            Crest = new FsmObject() { Value = crest.ToolCrest },
            trueEvent = FsmEvent.GetFsmEvent(crest.name),
            falseEvent = noEvent,
            storeValue = false,
        };

}
