using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using Needleforge.Data;
using Silksong.FsmUtil;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.Start))]
internal class MovesetFSMEdits
{
    [HarmonyPostfix]
    private static void AddCustomDownslashes(HeroController __instance)
    {
        IEnumerable<MovesetData> movesets =
            NeedleforgePlugin.newCrestData.Select(cd => cd.Moveset)
            .Where(m => m.HeroConfig && m.HeroConfig.DownSlashType == DownSlashTypes.Custom);

        if (!movesets.Any())
            return;

        PlayMakerFSM? fsm = __instance.crestAttacksFSM;
        fsm.Preprocess();

        FsmState
            Idle = fsm.GetState("Idle")!,
            End = fsm.GetState("End")!;

        foreach(MovesetData m in movesets)
        {
            string name = m.Crest.name;
            var fsmEdit = m.HeroConfig!.DownSlashFsmSetup;

            if (fsmEdit == null)
            {
                ModHelper.LogWarning(
                    $"Crest {name} has a custom downslash type, but doesn't define " +
                    $"a {nameof(HeroConfigNeedleforge.DownSlashFsmSetup)} function."
                );
                continue;
            }
            if (string.IsNullOrWhiteSpace(m.HeroConfig!.downSlashEvent))
            {
                ModHelper.LogWarning(
                    $"Crest {name} has a custom downslash type, but doesn't have a " +
                    $"valid {nameof(HeroControllerConfig.downSlashEvent)}."
                );
                continue;
            }

            FsmState AtkAntic = fsm.AddState($"{name} Downslash Antic");

            fsmEdit.Invoke(fsm, AtkAntic, out FsmState AtkEnd, out FsmState AtkBounce);

            Idle.AddTransition(m.HeroConfig!.downSlashEvent, AtkAntic.Name);
            AtkEnd.AddTransition("FINISHED", End.Name);
            AtkBounce.AddTransition("FINISHED", End.Name);
        }
    }
}
