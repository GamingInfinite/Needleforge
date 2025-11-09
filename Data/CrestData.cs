using HutongGames.PlayMaker;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TeamCherry.Localization;
using UnityEngine;
using SlotInfo = ToolCrest.SlotInfo;
using System.Reflection;

namespace Needleforge.Data
{

    public class CrestData
    {
        public Sprite? RealSprite;
        public Sprite? Silhouette;
        public Sprite? CrestGlow;
        public HeroControllerConfig? AttackConfig;
        public List<SlotInfo> slots = [];
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

        public SlotInfo AddToolSlot(ToolItemType color, AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            SlotInfo newSlot = new()
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
            return newSlot;
        }

        public SlotInfo AddSkillSlot(AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            return AddToolSlot(ToolItemType.Skill, binding, position, isLocked);
        }

        public SlotInfo AddRedSlot(AttackToolBinding binding, Vector2 position, bool isLocked)
        {
            return AddToolSlot(ToolItemType.Red, binding, position, isLocked);
        }

        public SlotInfo AddYellowSlot(Vector2 position, bool isLocked)
        {
            return AddToolSlot(ToolItemType.Yellow, AttackToolBinding.Neutral, position, isLocked);
        }

        public SlotInfo AddBlueSlot(Vector2 position, bool isLocked)
        {
            return AddToolSlot(ToolItemType.Blue, AttackToolBinding.Neutral, position, isLocked);
        }

        #region Auto Slot Navigation

        /// <summary>
        /// Sets the targets of navigation on <paramref name="source"/> for each
        /// specified direction. Leaving any of the directional parameters null or unset
        /// will leave <paramref name="source"/>'s navigation in that direction unchanged.
        /// </summary>
        /// <param name="source">The slot to set the navigation properties of.</param>
        /// <param name="up">
        ///     The slot to select when navigating up from <paramref name="source"/>.
        /// </param>
        /// <param name="right">
        ///     The slot to select when navigating right from <paramref name="source"/>.
        /// </param>
        /// <param name="left">
        ///     The slot to select when navigating left from <paramref name="source"/>.
        /// </param>
        /// <param name="down">
        ///     The slot to select when navigating down from <paramref name="source"/>.
        /// </param>
        public void SetSlotNavigation(
            SlotInfo source,
            SlotInfo? up = null, SlotInfo? right = null, SlotInfo? left = null, SlotInfo? down = null
        ) {
            int source_i = slots.FindIndex(s => SlotsEqual(source, s));
            if (source_i == -1)
            {
                ModHelper.LogError($"{SlotNotFoundMsg("Source")}. Stack Trace: {GetStackTrace()}");
                return;
            }

            SlotInfo slot = slots[source_i];
            bool printTrace = false;

            if (up != null)
            {
                int up_i = slots.FindIndex(s => SlotsEqual(up, s));
                if (up_i == -1)
                {
                    ModHelper.LogWarning(SlotNotFoundMsg("Up"));
                    printTrace = true;
                }
                else slot = slot with { NavUpIndex = up_i };
            }
            if (right != null)
            {
                int right_i = slots.FindIndex(s => SlotsEqual(right, s));
                if (right_i == -1)
                {
                    ModHelper.LogWarning(SlotNotFoundMsg("Right"));
                    printTrace = true;
                }
                else slot = slot with { NavRightIndex = right_i };
            }
            if (left != null)
            {
                int left_i = slots.FindIndex(s => SlotsEqual(left, s));
                if (left_i == -1)
                {
                    ModHelper.LogWarning(SlotNotFoundMsg("Left"));
                    printTrace = true;
                }
                else slot = slot with { NavLeftIndex = left_i };
            }
            if (down != null)
            {
                int down_i = slots.FindIndex(s => SlotsEqual(down, s));
                if (down_i == -1)
                {
                    ModHelper.LogWarning(SlotNotFoundMsg("Down"));
                    printTrace = true;
                }
                else slot = slot with { NavDownIndex = down_i };
            }

            if (printTrace)
                ModHelper.LogWarning($"{name}: Stack Trace: {GetStackTrace()}");

            slots[source_i] = slot;

            #region Local Functions
            string SlotNotFoundMsg(string identifier) =>
                $"{name}: {identifier} slot doesn't belong to this crest";

            static bool SlotsEqual(SlotInfo? one, SlotInfo? two) =>
                // null checks
                one is SlotInfo A && two is SlotInfo B
                // equality check
                && A.Position == B.Position && A.Type == B.Type
                && (
                    A.Type != ToolItemType.Red && A.Type != ToolItemType.Skill
                    || A.AttackBinding == B.AttackBinding
                );

            // Purely because I don't want any file path info or any stack frames past
            // the ones that are made by mod developers to get logged
            static string GetStackTrace() {
                StackTrace stackTrace = new(skipFrames: 2, fNeedFileInfo: true);
                StringBuilder trace = new();
                foreach (var frame in stackTrace.GetFrames()) {
                    if (!frame.HasMethod())
                        continue;
                    var method = frame.GetMethod();
                    if (method.DeclaringType.Namespace == nameof(UnityEngine))
                        break;
                    trace.AppendLine(
                        $"    at {method.DeclaringType}.{method.Name
                        }() line {frame.GetFileLineNumber()}"
                    );
                }
                return trace.ToString().Trim();
            }
            #endregion
        }

        #endregion

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
