using HarmonyLib;
using Needleforge.Data;
using System.Linq;

namespace Needleforge.Patches.HUD;

[HarmonyPatch(typeof(SilkSpool), nameof(SilkSpool.BindCost), MethodType.Getter)]
internal static class ShowBindCostInHud
{
    private static void Postfix(ref float __result)
    {
        CrestData? crest = NeedleforgePlugin.newCrestData.FirstOrDefault(x => x.IsEquipped);
        if (crest != null)
            __result = crest.bindCost;
    }
}
