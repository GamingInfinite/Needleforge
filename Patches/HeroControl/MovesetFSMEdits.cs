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
internal static class MovesetFSMEdits
{
    private static void Prefix(HeroController __instance)
    {
        ModHelper.Log("Editing Moveset FSMs...");
        DownSlashFSMEdits(__instance);
        DashSlashFSMEdits(__instance);
        ChargedSlashFSMEdits(__instance);
    }
    
    private static void DownSlashFSMEdits(HeroController hc)
    {
        IEnumerable<MovesetData>
            movesets = NeedleforgePlugin.newCrestData
                .Select(cd => cd.Moveset)
                .Where(m => m.HeroConfig
                    && (m.HeroConfig.DownSlashType == DownSlashTypes.Custom
                        || m.HeroConfig.DownSlashFsmEdit != null)
                );

        if (!movesets.Any())
            return;

        PlayMakerFSM fsm = hc.crestAttacksFSM;
        if (!fsm.Fsm.preprocessed)
            fsm.Preprocess();

        FsmState
            Idle = fsm.GetState("Idle")!,
            End = fsm.GetState("End")!;

        foreach(MovesetData m in movesets)
        {
            string name = m.Crest.name;
            ModHelper.Log($"{name} Down Slash");

            var fsmEdit = m.HeroConfig!.DownSlashFsmEdit;

            if (fsmEdit == null)
            {
                ModHelper.LogError(
                    $"Crest {name} has a custom downslash type, but doesn't define " +
                    $"a {nameof(HeroConfigNeedleforge.DownSlashFsmEdit)} function."
                );
                continue;
            }
            if (m.HeroConfig.DownSlashType != DownSlashTypes.Custom)
            {
                ModHelper.LogError(
                    $"Crest {name} has a {nameof(HeroConfigNeedleforge.DownSlashFsmEdit)} " +
                    $"function, but its {nameof(HeroConfigNeedleforge.DownSlashType)} " +
                    $"is not {DownSlashTypes.Custom}."
                );
                continue;
            }
            if (string.IsNullOrWhiteSpace(m.HeroConfig!.downSlashEvent))
            {
                ModHelper.LogError(
                    $"Crest {name} has a custom downslash, but doesn't have a " +
                    $"valid {nameof(HeroControllerConfig.downSlashEvent)}."
                );
                continue;
            }

            FsmState AtkStart = fsm.AddState($"{name} Start");
            Idle.AddTransition(m.HeroConfig!.downSlashEvent, AtkStart.Name);

            fsmEdit.Invoke(fsm, AtkStart, out FsmState[] AtkEnds);

            foreach(var end in AtkEnds)
                end.AddTransition("FINISHED", End.Name);
        }
    }

    private static void DashSlashFSMEdits(HeroController hc)
    {
        PlayMakerFSM fsm = hc.sprintFSM;
        if (!fsm.Fsm.preprocessed)
            fsm.Preprocess();

        FsmState
            StartAttack = fsm.GetState("Start Attack")!,
            RegainControlNormal = fsm.GetState("Regain Control Normal")!;

        #region Default behaviour

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

        int equipCheckIndex = 1 + Array.FindLastIndex(StartAttack.Actions, x => x is CheckIfCrestEquipped);

        foreach(MovesetData m in movesets)
        {
            string name = m.Crest.name;
            ModHelper.Log($"{name} Dash Slash");

            FsmState AtkStart = fsm.AddState($"{name} Start");

            var equipCheckAction = CreateCrestEquipCheck(m.Crest);
            StartAttack.InsertAction(equipCheckIndex, equipCheckAction);
            StartAttack.AddTransition(equipCheckAction.trueEvent.Name, AtkStart.name);

            m.HeroConfig!.DashSlashFsmEdit!.Invoke(fsm, AtkStart, out FsmState[] AtkEnds);

            foreach(var end in AtkEnds)
                end.AddTransition("FINISHED", RegainControlNormal.Name);
        }

        #region FSM Action Delegates

        static void RedirectToLoopingDefault(Action finished, PlayMakerFSM fsm)
        {
            if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
            {
                var attack = crest.Moveset.ConfigGroup!.DashStab.transform;
                fsm.GetIntVariable("Attack Steps").Value = attack.childCount;
                fsm.Fsm.Event(FsmEvent.GetFsmEvent("MULTIPLE"));
            }
            finished();
        }

        static void DetectAttackStepName(Action finished, PlayMakerFSM fsm)
        {
            if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
            {
                int i = fsm.GetIntVariable("Attack Step").Value - 1;
                var attack = crest.Moveset.ConfigGroup!.DashStab.transform;

                fsm.GetStringVariable("Attack Child Name")
                    .Value = attack.GetChild(i).name;
            }
            finished();
        }

        static void SetAttackAudioClip(Action finished, PlayMakerFSM fsm)
        {
            if (NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped) is CrestData crest)
            {
                int i = fsm.GetIntVariable("Attack Step").Value - 1;
                var attack = crest.Moveset.ConfigGroup!.DashStab.transform;
                var audioSrc = attack.GetChild(i).GetComponent<AudioSource>();

                if (audioSrc)
                    fsm.FsmVariables.FindFsmObject("Clip").Value = audioSrc.clip;
            }
            finished();
        }

        static void ClearAttackCallMethodCaches(Action finished, FsmState state)
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

        #endregion
    }

    private static void ChargedSlashFSMEdits(HeroController hc)
    {
        // Finding it this way until Needleforge bumps its FsmUtil version because a
        // bug was discovered in GetFsmPreprocessed that's present on our current
        // minimum version of it
        PlayMakerFSM fsm = hc.gameObject.GetComponents<PlayMakerFSM>().First(x => x.FsmName == "Nail Arts")!;
        if (!fsm.Fsm.preprocessed)
            fsm.Preprocess();

        FsmState
            AnticType = fsm.GetState("Antic Type")!,
            SetFinished = fsm.GetState("Set Finished")!;

        #region Default behaviour for custom charged slashes w/o fsm edits

        FsmState Kickoff = fsm.AddState("Needleforge Kickoff");

        AnticType.AddLambdaMethod(finished => RedirectToNeedleforgeKickoff(finished, fsm));
        AnticType.AddTransition(needleforgeDefaultEvent.Name, Kickoff.Name);

        Kickoff.AddAction(new CheckIsCharacterGrounded()
        {
            Target = new() { OwnerOption = OwnerDefaultOption.UseOwner },
            RayCount = new() { Value = 3 },
            GroundDistance = new() { Value = 0.2f },
            SkinWidth = new() { Value = -0.05f },
            SkinHeight = new() { Value = 0.1f },
            StoreResult = new() { Value = false },
            NotGroundedEvent = FsmEvent.GetFsmEvent("FINISHED"),
            EveryFrame = false,
        });
        Kickoff.AddLambdaMethod(DoKickoffIfRequested);
        Kickoff.AddTransition("FINISHED", "Antic");

        #endregion

        IEnumerable<MovesetData>
            movesets = NeedleforgePlugin.newCrestData
                .Select(cd => cd.Moveset)
                .Where(m => m.HeroConfig && m.HeroConfig.ChargedSlashFsmEdit != null);

        if (!movesets.Any())
            return;

        int equipCheckIndex = 1 + Array.FindLastIndex(AnticType.Actions, x => x is CheckIfCrestEquipped);

        foreach(MovesetData m in movesets)
        {
            string name = m.Crest.name;
            ModHelper.Log($"{name} Charged Slash");

            FsmState AtkStart = fsm.AddState($"{name} Start");

            var equipCheckAction = CreateCrestEquipCheck(m.Crest);
            AnticType.InsertAction(equipCheckIndex, equipCheckAction);
            AnticType.AddTransition(equipCheckAction.trueEvent.Name, AtkStart.name);

            m.HeroConfig!.ChargedSlashFsmEdit!.Invoke(fsm, AtkStart, out FsmState[] AtkEnds);

            foreach(var end in AtkEnds)
                end.AddTransition("FINISHED", SetFinished.Name);
        }

        #region FSM Action Delegates

        static void RedirectToNeedleforgeKickoff(Action finished, PlayMakerFSM fsm)
        {
            var crest = NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped);
            if (
                crest != null
                && crest.Moveset.HeroConfig!.ChargedSlashFsmEdit == null
            ) {
                fsm.Fsm.Event(needleforgeDefaultEvent);
            }
            finished();
        }

        static void DoKickoffIfRequested(Action finished)
        {
            var hc = HeroController.instance;
            if (hc.Config is HeroConfigNeedleforge config && config.ChargedSlashDoesKickoff)
            {
                hc.rb2d.linearVelocityY = 10;
            }
            finished();
        }

        #endregion
    }

    #region Utils

    private static readonly FsmEvent
        needleforgeDefaultEvent = FsmEvent.GetFsmEvent("NEEDLEFORGE DEFAULT"),
        noEvent = FsmEvent.GetFsmEvent("");

    private static CheckIfCrestEquipped CreateCrestEquipCheck(CrestData crest) =>
        new()
        {
            Crest = new FsmObject() { Value = crest.ToolCrest },
            trueEvent = FsmEvent.GetFsmEvent(crest.name),
            falseEvent = noEvent,
            storeValue = false,
        };

    #endregion

}
