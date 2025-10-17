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
        public static Dictionary<string, Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM>> bindEvents = new();

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
            harmony = new("com.example.patch");
            harmony.PatchAll();
        }

        public static ToolData AddTool(Sprite? InventorySprite, ToolItemType type, string name)
        {
            ToolData data = new()
            {
                inventorySprite = InventorySprite,
                type = type,
                name = name
            };

            newToolData.Add(data);
            return data;
        }

        public static ToolData AddTool(string name)
        {
            return AddTool(null, ToolItemType.Yellow, name);
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
        public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette)
        {
            CrestData crestData = new(name, RealSprite, Silhouette);

            newCrestData.Add(crestData);
            bindEvents[name] = (value, amount, time, fsm) =>
            {
                ModHelper.Log($"Running Bind for {name} Crest");
            };

            return crestData;
        }

        /// <summary>
        /// Adds a named Crest
        /// </summary>
        /// <param name="name">Name of the Crest</param>
        /// <returns><see cref="CrestData"/></returns>
        public static CrestData AddCrest(string name)
        {
            return AddCrest(name, null, null);
        }
    }
}