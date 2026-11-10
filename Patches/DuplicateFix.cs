using HarmonyLib;
using System.Linq;

namespace Needleforge.Patches.HeroControl;

[HarmonyPatch]
internal static class DuplicateFix
{
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.FinishedEnteringScene))]
    [HarmonyPostfix]
    private static void ReapplyCrest()
    {
        PlayerData? pd = PlayerData.instance;
        HeroController? hc = HeroController.SilentInstance;
        if (pd == null || !hc)
            return;

        ToolCrest? crest = NeedleforgePlugin.newCrests.FirstOrDefault(c => c && c.name == pd.CurrentCrestID);
        if (!crest || !crest!.HeroConfig || hc!.Config == crest.HeroConfig)
            return;

        ModHelper.Log($"{crest.name} is equipped but Hornet is using another config: re-applying it");
        ToolItemManager.SendEquippedChangedEvent(force: true);
    }
}