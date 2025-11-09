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

        /// <summary>
        /// <para>
        /// Sets up menu navigation between this crest's tool slots automatically. By
        /// default it will not overwrite any navigation properties which have already
        /// been set to a valid slot.
        /// </para><para>
        /// The algorithm may sometimes produce undesirable results. You can adjust its
        /// behaviour with the optional parameters <paramref name="directionAngleRange"/>
        /// and <paramref name="slotDimensions"/>, or you can override any connections
        /// it makes by setting specific slot connections manually.
        /// </para>
        /// </summary>
        /// <param name="onlyChangeInvalidProps">
        ///     If true, auto-navigation is only applied to navigation properties which
        ///     don't point to an existing slot. If false, auto-navigation will overwrite
        ///     ALL navigation properties.
        /// </param>
        /// <param name="directionAngleRange">
        ///     This is a range, in degrees, used to determine which cardinal direction
        ///     one tool slot is in relative to another tool slot. Higher values loosen
        ///     the restriction, smaller values tighten it. Setting the value below 45
        ///     degrees may rarely introduce "blind spots" that prevent a tool slot from
        ///     being reachable.
        /// </param>
        /// <param name="slotDimensions">
        ///     <para>
        ///     This affects how biased the algorithm is toward making either vertical
        ///     or horizontal connections between slots. If W is greater than H, there
        ///     are tighter angle restrictions on left-right connections and looser angle
        ///     restrictions on up-down connections; the reverse is also true.
        ///     </para><para>
        ///     Default is (1.5, 1.5). Values are clamped between 0.75 and 3.
        ///     </para>
        /// </param>
        public void ApplyAutoSlotNavigation(
            bool onlyChangeInvalidProps = true,
            float directionAngleRange = 46f,
            (float W, float H)? slotDimensions = null
        ) {
            #region Setup
            const float
                slotSize = 1.5f, // Slots in inventory UI are 1.5 x 1.5
                sMin = slotSize / 2f,
                sMax = slotSize * 2f,
                angleEpsilon = 2f,
                distanceEpsilon = 0.05f;

            // Using physics calcs for convenience; need colliders to represent slots
            GameObject
                objA = new("", typeof(Rigidbody2D)),
                objB = new("", typeof(Rigidbody2D));
            BoxCollider2D
                boxA = objA.AddComponent<BoxCollider2D>(),
                boxB = objB.AddComponent<BoxCollider2D>();

            boxA.size = boxB.size = new(
                Mathf.Clamp(slotDimensions?.W ?? slotSize, sMin, sMax),
                Mathf.Clamp(slotDimensions?.H ?? slotSize, sMin, sMax)
            );
            directionAngleRange = Mathf.Clamp(Mathf.Abs(directionAngleRange), 0f, 180f);
            #endregion

            for (int A = 0; A < slots.Count; A++)
            {
                // Current best candidate for navigation from slot A in each direction
                NavCandidate
                    up = new(-1, Vector2.down, Mathf.Infinity),
                    right = new(-1, Vector2.left, Mathf.Infinity),
                    left = new(-1, Vector2.right, Mathf.Infinity),
                    down = new(-1, Vector2.up, Mathf.Infinity);

                for (int B = 0; B < slots.Count; B++)
                {
                    if (A == B) continue;
                    Vector2 posA = slots[A].Position, posB = slots[B].Position;

                    objA.transform.position = posA;
                    objB.transform.position = posB;
                    ColliderDistance2D colliderDiff = Physics2D.Distance(boxB, boxA);

                    Vector2 angle = colliderDiff.normal;
                    float distance = colliderDiff.distance;

                    if (colliderDiff.isOverlapped)
                    {
                        // Severe overlap; switch to center-point-based angle
                        if (boxA.OverlapPoint(posB))
                            angle = (posB - posA).normalized;
                        // Shallow overlap; flip the angle the right way and continue
                        else
                            angle *= -Vector2.one;
                    }

                    NavCandidate candidate = new(B, angle, distance);

                    up = StrongerCandidate(candidate, up, Vector2.up);
                    right = StrongerCandidate(candidate, right, Vector2.right);
                    left = StrongerCandidate(candidate, left, Vector2.left);
                    down = StrongerCandidate(candidate, down, Vector2.down);
                }

                SlotInfo s = slots[A];
                slots[A] = s with {
                    NavUpIndex = CanSet(s.NavUpIndex) ? up.Index : s.NavUpIndex,
                    NavRightIndex = CanSet(s.NavRightIndex) ? right.Index : s.NavRightIndex,
                    NavLeftIndex = CanSet(s.NavLeftIndex) ? left.Index : s.NavLeftIndex,
                    NavDownIndex = CanSet(s.NavDownIndex) ? up.Index : s.NavDownIndex
                };
            }

            GameObject.Destroy(objA);
            GameObject.Destroy(objB);

            #region Local Functions
            NavCandidate StrongerCandidate(NavCandidate nuu, NavCandidate old, Vector2 direction){
                // check if nuu's angle is valid at all
                if (!AngleCloseTo(nuu.Angle, direction, directionAngleRange))
                    return old;

                // if the angles are nearly equivalent, favour the better distance
                if (AngleCloseTo(nuu.Angle, old.Angle, angleEpsilon))
                    return GTE(nuu.Distance, old.Distance, distanceEpsilon)
                        ? old : nuu;

                // otherwise, favour the better angle
                return old.Angle == CloserAngle(nuu.Angle, old.Angle, direction)
                        ? old : nuu;
            }

            static bool AngleCloseTo(Vector2 angle, Vector2 direction, float degreeRange)
                => Vector2.Angle(angle, direction) <= degreeRange;

            static Vector2 CloserAngle(Vector2 angleA, Vector2 angleB, Vector2 direction)
                => Vector2.Angle(angleA, direction) <= Vector2.Angle(angleB, direction)
                    ? angleA : angleB;

            static bool GTE(float lhs, float rhs, float epsilon)
                => Mathf.Abs(lhs - rhs) <= epsilon || lhs >= rhs;

            bool CanSet(int currentNav)
                => currentNav < 0 || currentNav >= slots.Count || !onlyChangeInvalidProps;
            #endregion
        }

        private record struct Size(float w, float h);
        private record struct NavCandidate(int Index, Vector2 Angle, float Distance);

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
