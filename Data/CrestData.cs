using HutongGames.PlayMaker;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TeamCherry.Localization;
using UnityEngine;
using UnityEngine.UIElements;
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
        /// Sets up menu navigation between this crest's tool slots automatically. By
        /// default it will not overwrite any previously set navigation properties. The
        /// auto-navigation logic can be fine-tuned with the optional parameters. This
        /// function should only be called after all slots have been added to the crest.
        /// </summary>
        /// <param name="onlyChangeInvalidProps">
        ///     If true, auto-navigation is only applied to navigation properties which
        ///     don't point to an existing slot. If false, auto-navigation will overwrite
        ///     ALL navigation properties.
        /// </param>
        /// <param name="angleRange">
        ///     A range in degrees used to determine which cardinal direction one tool
        ///     slot is in relative to another tool slot. Clamped between 45 and 180.
        /// </param>
        /// <param name="slotDimensions">
        ///     <para>
        ///     This affects how biased the algorithm is toward making either vertical or
        ///     horizontal connections. If x > y, restrictions are tighter on left-right
        ///     connections and looser on up-down connections; the inverse is also true.
        ///     </para><para>
        ///     Default is (1, 1). Values are clamped between 0.25 and 2.
        ///     </para>
        /// </param>
        public void ApplyAutoSlotNavigation(
            bool onlyChangeInvalidProps = true,
            float angleRange = 60f,
            Vector2? slotDimensions = null
        ) {
            #region Setup
            const float
                defaultSize = 1f, // Slots in inventory UI are 1.5 x 1.5
                sMin = 0.25f,
                sMax = 2.0f,
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
                Mathf.Clamp(slotDimensions?.x ?? defaultSize, sMin, sMax),
                Mathf.Clamp(slotDimensions?.y ?? defaultSize, sMin, sMax)
            );
            angleRange = Mathf.Clamp(Mathf.Abs(angleRange), 45f, 180f);
            #endregion

            for (int A = 0; A < slots.Count; A++)
            {
                Dictionary<Vector2, NavCandidate> bestTarget = new(){
                    { Vector2.up,    new(-1, Vector2.down,  Mathf.Infinity, Vector2.down) },
                    { Vector2.right, new(-1, Vector2.left,  Mathf.Infinity, Vector2.left) },
                    { Vector2.left,  new(-1, Vector2.right, Mathf.Infinity, Vector2.right) },
                    { Vector2.down,  new(-1, Vector2.up,    Mathf.Infinity, Vector2.up) }
                };

                for (int B = 0; B < slots.Count; B++)
                {
                    if (A == B) continue;
                    #region Calculate angles & distances

                    Vector2 posA = slots[A].Position, posB = slots[B].Position;

                    objA.transform.position = posA;
                    objB.transform.position = posB;
                    ColliderDistance2D colliderDiff = Physics2D.Distance(boxB, boxA);

                    float distance = colliderDiff.distance;
                    Vector2
                        boxAngle = colliderDiff.normal,
                        pointAngle = (posB - posA).normalized;

                    if (colliderDiff.isOverlapped)
                    {
                        if (boxA.OverlapPoint(posB)) // Severe overlap; forego box model
                            boxAngle = pointAngle;
                        else                        // Shallow overlap; flip angle & continue
                            boxAngle *= -Vector2.one;
                    }
                    #endregion
                    #region Decide if & how A navigates to B

                    foreach (Vector2 direction in bestTarget.Keys.ToList())
                    {
                        NavCandidate best = bestTarget[direction];
                        float
                            boxAngleDiff = Vector2.Angle(boxAngle, direction),
                            pointAngleDiff = Vector2.Angle(pointAngle, direction),
                            bestBoxAngleDiff = Vector2.Angle(best.BoxAngle, direction),
                            bestPtAngleDiff = Vector2.Angle(best.PointAngle, direction);
                        bool
                            boxAnglesEqual = boxAngleDiff.IsWithinTolerance(angleEpsilon, bestBoxAngleDiff),
                            distancesEqual = distance.IsWithinTolerance(distanceEpsilon, best.Distance);

                        if (
                            boxAngleDiff > angleRange // on incorrect side of the reference slot
                            || (boxAnglesEqual && distancesEqual && bestPtAngleDiff <= pointAngleDiff)
                            || (boxAnglesEqual && best.Distance < distance)
                            || bestBoxAngleDiff < boxAngleDiff
                        ) {
                            continue;
                        }
                        bestTarget[direction] = new NavCandidate(B, boxAngle, distance, pointAngle);
                    }
                    #endregion
                }

                slots[A] = slots[A] with {
                    NavUpIndex = CanSet(slots[A].NavUpIndex)
                        ? bestTarget[Vector2.up].Index : slots[A].NavUpIndex,
                    NavRightIndex = CanSet(slots[A].NavRightIndex)
                        ? bestTarget[Vector2.right].Index : slots[A].NavRightIndex,
                    NavLeftIndex = CanSet(slots[A].NavLeftIndex)
                        ? bestTarget[Vector2.left].Index : slots[A].NavLeftIndex,
                    NavDownIndex = CanSet(slots[A].NavDownIndex)
                        ? bestTarget[Vector2.down].Index : slots[A].NavDownIndex
                };
            }

            GameObject.Destroy(objA);
            GameObject.Destroy(objB);

            bool CanSet(int navProp)
                => navProp < 0 || navProp >= slots.Count || !onlyChangeInvalidProps;
        }

        private record struct NavCandidate(
            int Index, Vector2 BoxAngle, float Distance, Vector2 PointAngle);

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
