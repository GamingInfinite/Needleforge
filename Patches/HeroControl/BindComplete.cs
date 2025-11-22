using HarmonyLib;
using Needleforge.Data;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.BindCompleted))]
internal class BindComplete
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        foreach (CrestData data in NeedleforgePlugin.newCrestData)
        {
            if (data.IsEquipped)
            {
                data.BindCompleteEvent();
            }
        }
    }
}