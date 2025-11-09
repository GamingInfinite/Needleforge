using HarmonyLib;
using Needleforge.Data;
using Needleforge.Makers;

namespace Needleforge.Patches
{
    [HarmonyPatch(typeof(ToolItemManager), nameof(ToolItemManager.Awake))]
    internal class AddToolsAndCrests
    {
        [HarmonyPostfix]
        public static void AddCrests()
        {
            ModHelper.Log("Adding Crests...");
            foreach (CrestData data in NeedleforgePlugin.newCrestData)
            {
                ModHelper.Log($"Adding {data.name}");
                CrestMaker.CreateCrest(data);
            }
        }

        [HarmonyPostfix]
        public static void AddTools()
        {
            ModHelper.Log("Adding Tools...");
            foreach (ToolData data in NeedleforgePlugin.newToolData)
            {
                ModHelper.Log($"Adding {data.name}");

                if (data is LiquidToolData liquidData)
                {
                    ToolMaker.CreateLiquidTool(liquidData.name, liquidData.storageAmount, liquidData.maxRefills, liquidData.color, liquidData.infiniteRefills, liquidData.resource, liquidData.replenishUsage, liquidData.replenishMult, liquidData.FullSprites, liquidData.EmptySprites, data.displayName, data.description);
                }
                else
                {
                    ToolMaker.CreateBasicTool(data.inventorySprite, data.type, data.name, data.displayName, data.description);
                }
            }
        }
    }
}
