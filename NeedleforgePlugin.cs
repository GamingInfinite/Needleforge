using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker;
using Needleforge.Data;
using Needleforge.Makers;
using PrepatcherPlugin;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge
{
    // TODO - adjust the plugin guid as needed
    [BepInAutoPlugin(id: "io.github.needleforge")]
    public partial class NeedleforgePlugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public static Harmony harmony;

        public static List<ToolData> newToolData = new();
        public static List<CrestData> newCrestData = new();
        public static List<ToolCrest> newCrests = new();
        public static List<ToolItem> newTools = new();
        public static Dictionary<string, Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM>> bindEvents = new();
        public static Dictionary<string, UniqueBindEvent> uniqueBind = new();
        public static Dictionary<string, Action> toolEventHooks = new();

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
            harmony = new("com.example.patch");
            harmony.PatchAll();
        }

        public static ToolData GetToolDataByName(string name)
        {
            foreach (var t in newToolData)
            {
                if (t.name == name)
                {
                    return t;
                }
            }
            return null;
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, string InfiniteRefillsPD, Color liquidColor, 
            ToolItem.ReplenishResources resource, ToolItem.ReplenishUsages replenishUsage, float replenishMult, 
            StateSprites? fullSprites, StateSprites? emptySprites, 
            string clip = "Charge Up")
        {
            LiquidToolData data = new()
            {
                type = ToolItemType.Red,
                name = name,

                color = liquidColor,
                maxRefills = maxRefills,
                storageAmount = storageAmount,
                infiniteRefills = InfiniteRefillsPD,

                clip = clip,

                resource = resource,
                replenishUsage = replenishUsage,
                replenishMult = replenishMult,

                FullSprites = fullSprites,
                EmptySprites = emptySprites
            };

            newToolData.Add(data);
            toolEventHooks[$"{data.name} BEFORE ANIM"] = () =>
            {
                ModHelper.Log($"BEFORE ANIM for {data.name}");
            };
            toolEventHooks[$"{data.name} AFTER ANIM"] = () =>
            {
                ModHelper.Log($"AFTER ANIM for {data.name}");
            };

            return data;
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, string InfiniteRefillsPD, Color liquidColor, ToolItem.ReplenishResources resource, ToolItem.ReplenishUsages replenishUsage, float replenishMult)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor, resource, replenishUsage, replenishMult, null, null, "Charge Up");
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, string InfiniteRefillsPD, Color liquidColor)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor, ToolItem.ReplenishResources.Shard, ToolItem.ReplenishUsages.Percentage, 1f);
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, Color liquidColor)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, "", liquidColor);
        }

        public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName, LocalisedString description, Sprite? InventorySprite)
        {
            ToolData data = new()
            {
                inventorySprite = InventorySprite,
                type = type,
                name = name,
                displayName = displayName,
                description = description,
            };

            PlayerDataVariableEvents.OnGetBool += (pd, fieldname, current) =>
            {
                if (fieldname == data.unlockedPDString)
                {
                    return data.UnlockedAtStart;
                }
                return current;
            };

            newToolData.Add(data);
            return data;
        }

        public static ToolData AddTool(string name, ToolItemType type, Sprite? InventorySprite)
        {
            return AddTool(name, type, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" }, new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, InventorySprite);
        }

        public static ToolData AddTool(string name, ToolItemType type)
        {
            return AddTool(name, type, null);
        }

        public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName, LocalisedString description)
        {
            return AddTool(name, type, displayName, description, null);
        }

        public static ToolData AddTool(string name, LocalisedString displayName, LocalisedString description)
        {
            return AddTool(name, ToolItemType.Yellow, displayName, description, null);
        }

        public static ToolData AddTool(string name)
        {
            return AddTool(name, ToolItemType.Yellow, null);
        }

        /// <summary>
        /// Adds your class with the sprites already attached.
        /// <para/>
        /// IMPORTANT: <br/>
        /// for obvious reasons certain data will return either null or some default value (eg. false for bool) until a Save is loaded.
        /// </summary>
        /// <param name="name">Name of the Crest</param>
        /// <param name="RealSprite">Inventory Sprite</param>
        /// <param name="Silhouette">Crest List Sprite</param>
        /// <returns><see cref="CrestData"/></returns>
        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description, Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
        {
            CrestData crestData = new(name, displayName, description, RealSprite, Silhouette, CrestGlow);

            newCrestData.Add(crestData);
            bindEvents[name] = (value, amount, time, fsm) =>
            {
                ModHelper.Log($"Running Bind for {name} Crest");
            };

            return crestData;
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
        {
            return AddCrest(name, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" }, new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, RealSprite, Silhouette, CrestGlow);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description, Sprite? RealSprite, Sprite? Silhouette)
        {
            return AddCrest(name, displayName, description, RealSprite, Silhouette, null);
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette)
        {
            return AddCrest(name, RealSprite, Silhouette, null);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description, Sprite? RealSprite)
        {
            return AddCrest(name, displayName, description, RealSprite, null);
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite)
        {
            return AddCrest(name, RealSprite, null);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description)
        {
            return AddCrest(name, displayName, description, null);
        }

        /// <summary>
        /// Adds a named Crest
        /// </summary>
        /// <param name="name">Name of the Crest</param>
        /// <returns><see cref="CrestData"/></returns>
        public static CrestData AddCrest(string name)
        {
            return AddCrest(name, null);
        }
    }
}