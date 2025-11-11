using HarmonyLib;
using Needleforge.Data;

namespace Needleforge.Patches;

[HarmonyPatch(typeof(HeroController), nameof(HeroController.BindCompleted))]
public class BindComplete
{
    [HarmonyPostfix]
    public static void Postfix(HeroController __instance)
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