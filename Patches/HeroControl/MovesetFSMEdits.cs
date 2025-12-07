using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge.Data;
using Silksong.FsmUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class DownSlashFSMEdits
{
    private static void Postfix(HeroController __instance)
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
}

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class DashSlashFSMEdits
{
    private static void Postfix(HeroController __instance)
    {
        PlayMakerFSM fsm = __instance.sprintFSM;
        fsm.Preprocess();

        FsmState
            StartAttack = fsm.GetState("Start Attack")!,
            RegainControlNormal = fsm.GetState("Regain Control Normal")!;

        int
            equipCheckIndex = 1 + Array.FindLastIndex(StartAttack.Actions, x => x is CheckIfCrestEquipped);

        #region Making default behaviour functional

        StartAttack.InsertLambdaMethod(
            1 + Array.FindLastIndex(StartAttack.Actions, x => x is SetIntValue),
            finished => RedirectToLoopingDefault(finished, fsm)
        );

        FsmState SetAttackMultiple = fsm.GetState("Set Attack Multiple")!;
        SetAttackMultiple.InsertLambdaMethod(
            Array.FindLastIndex(SetAttackMultiple.Actions, x => x is SetPolygonCollider),
            finished => DetectAttackStepName(finished, fsm)
        );

        FsmState AttackDashStart = fsm.GetState("Attack Dash Start")!;
        AttackDashStart.InsertLambdaMethod(
            0,
            finished => ClearAttackCallMethodCaches(finished, AttackDashStart)
            // Necessary to avoid breaking hunter and witch dash slashes if the custom
            // crest's dash attack is used before either of theirs after a save is loaded
        );
        AttackDashStart.InsertLambdaMethod(
            Array.FindIndex(AttackDashStart.Actions, x => x is PlayAudioEvent),
            finished => SetAttackAudioClip(finished, fsm)
        );

        #endregion

        IEnumerable<MovesetData>
            movesets = NeedleforgePlugin.newCrestData
                .Select(cd => cd.Moveset)
                .Where(m => m.HeroConfig && m.HeroConfig.DashSlashFsmEdit != null);

        if (!movesets.Any())
            return;

        foreach(MovesetData m in movesets) {
            string name = m.Crest.name;
            var fsmEdit = m.HeroConfig!.DashSlashFsmEdit!;

            FsmState Antic = fsm.AddState($"{name} Antic");
            fsmEdit.Invoke(fsm, Antic, out FsmState AtkEnd, out FsmState AtkHit);

            StartAttack.InsertAction(equipCheckIndex, CreateCrestEquipCheck(m.Crest));

            StartAttack.AddTransition(name, Antic.name);
            AtkEnd.AddTransition("FINISHED", RegainControlNormal.Name);
            AtkHit.AddTransition("FINISHED", RegainControlNormal.Name);
        }
    }

    private static void RedirectToLoopingDefault(Action finished, PlayMakerFSM fsm)
    {
        if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
        {
            var attack = crest.Moveset.ConfGroup!.DashStab.transform;
            fsm.GetIntVariable("Attack Steps").Value = attack.childCount;
            fsm.Fsm.Event(FsmEvent.GetFsmEvent("MULTIPLE"));
        }
        finished();
    }

    private static void DetectAttackStepName(Action finished, PlayMakerFSM fsm)
    {
		if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
        {
            int i = fsm.GetIntVariable("Attack Step").Value - 1;
            var attack = crest.Moveset.ConfGroup!.DashStab.transform;

            fsm.GetStringVariable("Attack Child Name")
                .Value = attack.GetChild(i).name;
        }
        finished();
    }

    private static void SetAttackAudioClip(Action finished, PlayMakerFSM fsm)
    {
        if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
        {
            int i = fsm.GetIntVariable("Attack Step").Value - 1;
            var attack = crest.Moveset.ConfGroup!.DashStab.transform;
            var audioSrc = attack.GetChild(i).GetComponent<AudioSource>();

            if (audioSrc)
                fsm.FsmVariables.FindFsmObject("Clip").Value = audioSrc.clip;
        }
        finished();
    }

    private static void ClearAttackCallMethodCaches(Action finished, FsmState state)
    {
        foreach(var callmethod in state.Actions.OfType<CallMethodProper>())
        {
            if (
                typeof(NailAttackBase).IsAssignableFrom(callmethod.cachedType)
                && callmethod.cachedType != typeof(DashStabNailAttack)
            ) {
                callmethod.cachedType = null;
                callmethod.cachedMethodInfo = null;
                callmethod.cachedParameterInfo = [];
            }
        }
        finished();
    }

    private static readonly FsmEvent noEvent = FsmEvent.GetFsmEvent("");

    private static CheckIfCrestEquipped CreateCrestEquipCheck(CrestData crest) =>
        new()
        {
            Crest = new FsmObject() { Value = crest.ToolCrest },
            trueEvent = FsmEvent.GetFsmEvent(crest.name),
            falseEvent = noEvent,
            storeValue = false,
        };

}
