using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker;
using Needleforge.Data;
using Needleforge.Makers;
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
        public static Dictionary<string, Action<FsmInt, FsmInt, FsmFloat>> bindEvents = new();

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
            harmony = new("com.example.patch");
            harmony.PatchAll();
        }

        public static int AddTool(Sprite? InventorySprite, ToolItemType type, string name)
        {
            ToolData data = new()
            {
                inventorySprite = InventorySprite,
                type = type,
                name = name
            };

            newToolData.Add(data);
            return newToolData.Count - 1;
        }

        public static int AddTool(string name)
        {
            return AddTool(null, ToolItemType.Yellow, name);
        }

        public static int AddCrest(Sprite? RealSprite, Sprite? Silhouette, string name)
        {
            CrestData crestData = new()
            {
                RealSprite = RealSprite,
                Silhouette = Silhouette,
                name = name,
            };

            newCrestData.Add(crestData);
            bindEvents[name] = (value, amount, time) =>
            {
                ModHelper.Log($"Running Bind for {name} Crest");
            };

            return newCrestData.Count - 1;
        }

        public static int AddCrest(string name)
        {
            return AddCrest(null, null, name);
        }
    }
}