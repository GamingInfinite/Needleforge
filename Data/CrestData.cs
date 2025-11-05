using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamCherry.Localization;
using UnityEngine;

namespace Needleforge.Data
{

    public class CrestData
    {
        public Sprite? RealSprite;
        public Sprite? Silhouette;
        public Sprite? CrestGlow;
        public HeroControllerConfig? AttackConfig;
        public List<ToolCrest.SlotInfo> slots = [];
        public int bindCost = 9;
        public string name = "";
        public bool UnlockedAtStart = true;

        public LocalisedString displayName;
        public LocalisedString description;

        /// <summary>
        /// <para>
        /// Can be used to customize the look of the HUD. The default is the
        /// Hunter crest level 1 HUD.
        /// </para><para>
        /// Customization can be as simple as picking a different HUD from a vanilla
        /// crest with <see cref="HudFrameData.Preset"/>, or further customized with a
        /// custom coroutine, custom animations, and/or adding additional
        /// GameObjects/MonoBehaviours to the HUD.
        /// </para>
        /// </summary>
        public HudFrameData HudFrame { get; }

        public Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM> BindEvent
        {
            get
            {
                return NeedleforgePlugin.bindEvents[name];
            }
            set
            {
                NeedleforgePlugin.bindEvents[name] = value;
            }
        }
        public UniqueBindEvent uniqueBindEvent
        {
            get
            {
                return NeedleforgePlugin.uniqueBind[name];
            }
            set
            {
                NeedleforgePlugin.uniqueBind[name] = value;
            }
        }

        public ToolCrest? ToolCrest
        {
            get => NeedleforgePlugin.newCrests.FirstOrDefault(crest => crest.name == name);
        }

        public bool IsEquipped
        {
            get => ToolCrest != null && ToolCrest.IsEquipped;
        }

        public void AddToolSlot(ToolItemType color, AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            ToolCrest.SlotInfo newSlot = new()
            {
                AttackBinding = binding,
                Type = color,
                Position = position,
                IsLocked = isLocked,
                NavUpIndex = -1,
                NavUpFallbackIndex = -1,
                NavRightIndex = -1,
                NavRightFallbackIndex = -1,
                NavLeftIndex = -1,
                NavLeftFallbackIndex = -1,
                NavDownIndex = -1,
                NavDownFallbackIndex = -1,
            };

            slots.Add(newSlot);
        }

        public void AddSkillSlot(AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            AddToolSlot(ToolItemType.Skill, binding, position, isLocked);
        }

        public void AddRedSlot(AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            AddToolSlot(ToolItemType.Red, binding, position, isLocked);
        }

        public void AddYellowSlot(Vector2 position, bool isLocked)
        {
            AddToolSlot(ToolItemType.Yellow, AttackToolBinding.Neutral, position, isLocked);
        }

        public void AddBlueSlot(Vector2 position, bool isLocked)
        {
            AddToolSlot(ToolItemType.Blue, AttackToolBinding.Neutral, position, isLocked);
        }

        public CrestData(string name, LocalisedString displayName, LocalisedString description, Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
        {
            this.name = name;
            this.RealSprite = RealSprite;
            this.Silhouette = Silhouette;
            this.CrestGlow = CrestGlow;
            this.displayName = displayName;
            this.description = description;
            HudFrame = new HudFrameData(this);
        }
    }
}
