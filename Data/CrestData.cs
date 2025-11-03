using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using TeamCherry.Localization;
using UnityEngine;
using CoroutineFunction = BindOrbHudFrame.CoroutineFunction;

namespace Needleforge.Data
{
    public enum UniqueBindDirection
    {
        UP,
        DOWN, 
        LEFT, 
        RIGHT
    }

    public enum BaseGameCrest
    {
        HUNTER,
        HUNTER_V2,
        HUNTER_V3,
        WARRIOR,
        REAPER,
        WANDERER,
        WITCH,
        ARCHITECT,
        SHAMAN,
        CURSED,
        CLOAKLESS,
    }

    public class UniqueBindEvent(UniqueBindDirection Direction, Action<Action> lambdaMethod)
    {
        public UniqueBindDirection Direction = Direction;
        public Action<Action> lambdaMethod = lambdaMethod;
    }

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
        /// Changes the look of this crest's HUD frame to match one of the base game
        /// crests. This doesn't include unique animations like Beast's rage mode HUD.
        /// </summary>
        public BaseGameCrest BaseGameHudFrame { get; set; } = BaseGameCrest.HUNTER;

        /// <summary>
        /// An optional coroutine which will run continuously when this crest is equipped.
        /// This should be used to control any extra animations or visual effects for the
        /// crest's HUD frame.
        /// For examples, see the source code of <see cref="BindOrbHudFrame"/>.
        /// </summary>
        public CoroutineFunction? HudFrameCoroutine { get; set; }

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
            get
            {
                if (HeroController.instance != null)
                {
                    foreach (var crest in NeedleforgePlugin.newCrests)
                    {
                        if (crest.name == name)
                        {
                            return crest;
                        }
                    }
                }
                return null;
            }
        }

        public bool IsEquipped
        {
            get
            {
                if (ToolCrest != null)
                {
                    return ToolCrest.IsEquipped;
                }
                return false;
            }
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
        }
    }
}
