using HarmonyLib;
using Needleforge.Data;
using Needleforge.Makers;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.SetupNewPlayerData))]
    internal class NewProfileSetup
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            foreach(CrestData data in NeedleforgePlugin.newCrestData)
            {
                if (data.ToolCrest != null && data.UnlockedAtStart)
                {
                    data.ToolCrest.SaveData = CrestMaker.CreateDefaultSaveData();
                }
            }
        }
    }
}
