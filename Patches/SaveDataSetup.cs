using HarmonyLib;
using Needleforge.Data;
using Needleforge.Makers;
using System.Collections.Generic;

namespace Needleforge.Patches;

[HarmonyPatch]
internal class SaveDataSetup
{
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupNewPlayerData))]
    [HarmonyPostfix]
    private static void AddToNewSaveData(PlayerData __instance)
    {
        foreach(CrestData data in NeedleforgePlugin.newCrestData)
        {
            if (data.ToolCrest != null && data.UnlockedAtStart)
            {
                __instance.ToolEquips.SetData(data.name, CrestMaker.CreateDefaultSaveData());
            }
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupExistingPlayerData))]
    [HarmonyPostfix]
    private static void AddToExistingSaveData(PlayerData __instance)
    {
        List<string> validCrestNames = __instance.ToolEquips.GetValidNames();
        foreach (CrestData data in NeedleforgePlugin.newCrestData)
        {
            if (data.ToolCrest != null && data.UnlockedAtStart && !validCrestNames.Contains(data.name))
            {
                __instance.ToolEquips.SetData(data.name, CrestMaker.CreateDefaultSaveData());
            }
        }
    }
}
