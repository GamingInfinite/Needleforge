using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Needleforge.Data;
using PrepatcherPlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TeamCherry.Localization;
using UnityEngine;
using BindEventHandler = Needleforge.Data.CrestData.BindEventHandler;

namespace Needleforge;

#pragma warning disable CS1591 // Missing XML comment
[BepInAutoPlugin(id: "io.github.needleforge")]
public partial class NeedleforgePlugin : BaseUnityPlugin
#pragma warning restore CS1591
{
    internal static ManualLogSource logger;
    internal static Harmony harmony;

    internal static List<ToolData> newToolData = [];
    internal static List<CrestData> newCrestData = [];
    internal static List<ToolCrest> newCrests = [];
    internal static List<ToolItem> newTools = [];
    internal static ObservableCollection<ColorData> newColors = [];

    internal static Dictionary<string, GameObject> hudRoots = [];
    internal static Dictionary<string, BindEventHandler> bindEvents = [];
    internal static Dictionary<string, Action> bindCompleteEvents = [];
    internal static Dictionary<string, UniqueBindEvent> uniqueBind = [];

    internal static Dictionary<string, Action> toolEventHooks = [];

    /// <summary>
    /// A custom tool color. Green slots can accept green, blue and yellow tools;
    /// and green tools can be equipped into green, blue, and yellow slots.
    /// </summary>
    public static readonly ColorData GreenTools = AddToolColor(
        "Green",
        new Color(0.57f, 0.86f, 0.59f, 1f)
    );

    /// <summary>
    /// A custom tool color. Pink slots can accept pink, white and red tools;
    /// and pink tools can be equipped into pink, white and red slots.
    /// </summary>
    public static readonly ColorData PinkTools = AddToolColor(
        "Pink",
        new Color(1f, 0.59f, 0.78f, 1f),
        true
    );

    /// <summary>
    /// A custom tool color. Black slots can accept tools of any color;
    /// and black tools can be equipped into slots of any color.
    /// </summary>
    public static readonly ColorData BlackTools = AddToolColor(
        "Black",
        new Color(0.40f, 0.40f, 0.40f, 1f),
        true
    );

    private void Awake()
    {
        logger = Logger;
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        harmony = new(Id);
        harmony.PatchAll();

        newColors.CollectionChanged += NewColors_CollectionChanged;

        GreenTools.AddValidTypes(ToolItemType.Yellow, ToolItemType.Blue);
        PinkTools.AddValidTypes(ToolItemType.Red, ToolItemType.Skill);
        BlackTools.allColorsValid = true;
    }

    private void NewColors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // Doing this so the static variable updates
        // whenever a user of needleforge adds a color, in theory
        InventoryToolCrest.TOOL_TYPES = (ToolItemType[])Enum.GetValues(typeof(ToolItemType));
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

    #region Tools

    public static ColorData AddToolColor(string name, Color color, bool isAttackType = false)
    {
        ColorData newColor = new()
        {
            name = name,
            color = color,
            isAttackType = isAttackType
        };
        newColors.Add(newColor);
        return newColor;
    }

    public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
        string InfiniteRefillsPD, Color liquidColor,
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
        toolEventHooks[$"{data.name} BEFORE ANIM"] = () => { ModHelper.Log($"BEFORE ANIM for {data.name}"); };
        toolEventHooks[$"{data.name} AFTER ANIM"] = () => { ModHelper.Log($"AFTER ANIM for {data.name}"); };

        return data;
    }

    public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
        string InfiniteRefillsPD, Color liquidColor, ToolItem.ReplenishResources resource,
        ToolItem.ReplenishUsages replenishUsage, float replenishMult)
    {
        return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor, resource,
            replenishUsage, replenishMult, null, null, "Charge Up");
    }

    public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
        string InfiniteRefillsPD, Color liquidColor)
    {
        return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor,
            ToolItem.ReplenishResources.Shard, ToolItem.ReplenishUsages.Percentage, 1f);
    }

    public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, Color liquidColor)
    {
        return AddLiquidTool(name, maxRefills, storageAmount, "", liquidColor);
    }

    public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName,
        LocalisedString description, Sprite? InventorySprite)
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
        return AddTool(name, type, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" },
            new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, InventorySprite);
    }

    public static ToolData AddTool(string name, ToolItemType type)
    {
        return AddTool(name, type, null);
    }

    public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName,
        LocalisedString description)
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

    #endregion

    #region Crests

    /// <summary>
    /// Creates a new custom crest and adds it to Needleforge.
    /// </summary>
    /// <remarks>
    /// <b>Important:</b> Certain data will return either null or some default value
    /// (eg. false for bool) until a Save is loaded.
    /// </remarks>
    /// <param name="name">Name of the crest.</param>
    /// <param name="displayName">In-game display name of the crest.</param>
    /// <param name="description">In-game description of the crest.</param>
    /// <param name="RealSprite">Main crest sprite in the inventory UI.</param>
    /// <param name="Silhouette">
    ///     Filled-in sprite used for unselected crests in the crest swapping menu.
    /// </param>
    /// <param name="CrestGlow">
    ///     Sprite which briefly flashes over top of the <paramref name="RealSprite"/>
    ///     when the crest is equipped.
    /// </param>
    /// <returns>The newly created <see cref="CrestData"/>.</returns>
    public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
        Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
    {
        CrestData crestData = new(name, displayName, description, RealSprite, Silhouette, CrestGlow);

        newCrestData.Add(crestData);
        bindEvents[name] = (value, amount, time, fsm) => { ModHelper.Log($"Running Bind for {name} Crest"); };
        bindCompleteEvents[name] = () => { ModHelper.Log($"Bind for {name} Crest Complete"); };

        return crestData;
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
    {
        return AddCrest(name, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" },
            new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, RealSprite, Silhouette, CrestGlow);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
        Sprite? RealSprite, Sprite? Silhouette)
    {
        return AddCrest(name, displayName, description, RealSprite, Silhouette, null);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette)
    {
        return AddCrest(name, RealSprite, Silhouette, null);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
        Sprite? RealSprite)
    {
        return AddCrest(name, displayName, description, RealSprite, null);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, Sprite? RealSprite)
    {
        return AddCrest(name, RealSprite, null);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description)
    {
        return AddCrest(name, displayName, description, null);
    }

    /// <inheritdoc cref="AddCrest(string, LocalisedString, LocalisedString, Sprite?, Sprite?, Sprite?)" />
    public static CrestData AddCrest(string name)
    {
        return AddCrest(name, null);
    }

    #endregion
}