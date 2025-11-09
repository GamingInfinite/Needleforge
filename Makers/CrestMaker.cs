using System;
using System.Collections.Generic;
using System.Text;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Makers
{
    internal class CrestMaker
    {
        internal static ToolCrestsData.Data CreateDefaultSaveData() =>
            new()
            {
                IsUnlocked = true,
                Slots = [],
                DisplayNewIndicator = true,
            };

        /// <summary>
        /// NEVER USE THIS, USE <see cref="NeedleforgePlugin.AddCrest"/> INSTEAD
        /// </summary>
        /// <param name="RealSprite"></param>
        /// <param name="Silhouette"></param>
        /// <param name="attackConfig"></param>
        /// <param name="slots"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static ToolCrest CreateCrest(Sprite? RealSprite, Sprite? Silhouette, HeroControllerConfig? attackConfig, List<ToolCrest.SlotInfo> slots, string name, LocalisedString displayName, LocalisedString description)
        {
            List<ToolCrest> crests = ToolItemManager.GetAllCrests();
            ToolCrest hunter = crests[0];

            ToolCrest newCrest = ScriptableObject.CreateInstance<ToolCrest>();

            newCrest.name = name;
            newCrest.crestGlow = hunter.crestGlow;
            newCrest.crestSilhouette = Silhouette ?? hunter.crestSilhouette;
            newCrest.crestSprite = RealSprite ?? hunter.crestSprite;

            newCrest.displayName = displayName;
            newCrest.description = description;

            newCrest.slots = [.. slots];

            newCrest.heroConfig = attackConfig ?? hunter.heroConfig;
            newCrest.SaveData = CreateDefaultSaveData();

            ToolItemManager.Instance.crestList.Add(newCrest);

            NeedleforgePlugin.newCrests.Add(newCrest);

            return newCrest;
        }
    }
}
