using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BasicFrameAnims = BindOrbHudFrame.BasicFrameAnims;

namespace Needleforge.Data;

public class HudData(CrestData owner) {

    private readonly CrestData Crest = owner;

    /// <summary>
    /// Changes the look of this crest's HUD frame to match one of the base game
    /// crests. This doesn't include unique animations like Beast's rage mode HUD.
    /// </summary>
    public VanillaCrest PresetFrame { get; set; } = VanillaCrest.HUNTER;

    /// <summary>
    /// A custom animation for when this crest is equipped.
    /// <reqs>All custom HUD animations must have a unique name.</reqs>
    /// </summary>
    public tk2dSpriteAnimationClip? Appear { get; set; }

    /// <summary>
    /// A custom animation for when the HUD first appears upon loading a save where
    /// this crest is equipped.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? AppearFromNone { get; set; }

    /// <summary>
    /// A custom animation which determines this crest's idle appearance.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? Idle { get; set; }

    /// <summary>
    /// A custom animation for when this crest is unequipped.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? Disappear { get; set; }

    /// <summary>
    /// Any extra custom animations for this crest's HUD.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// Add a <see cref="Coroutine"/> to use these animations.
    /// </summary>
    public List<tk2dSpriteAnimationClip> ExtraAnims { get; } = [];

    internal IEnumerable<tk2dSpriteAnimationClip> AllCustomAnims =>
        new List<tk2dSpriteAnimationClip?> {
            Appear, AppearFromNone, Idle, Disappear
        }
        .Where(x => x != null)
        .Cast<tk2dSpriteAnimationClip>()
        .Concat(ExtraAnims);

    internal bool HasCustomAnims =>
        Appear != null
        || AppearFromNone != null
        || Idle != null
        || Disappear != null
        || ExtraAnims.Count > 0;

    internal BasicFrameAnims HudBasicFrameAnims =>
        new() {
            Appear = Appear?.name ?? "",
            AppearFromNone = AppearFromNone?.name ?? "",
            Idle = Idle?.name ?? "",
            Disappear = Disappear?.name ?? "",
            // TODO figure out what ActivateEvent does
        };

    /// <summary>
    /// After the HUD has been created, this returns a dedicated GameObject for this
    /// crest which is attached to the HUD. <b>This will be destroyed and recreated
    /// every time the player quits to the main menu;</b> any modifications or
    /// additions to this GameObject should be made in a handler attached to
    /// <see cref="OnInitializing"/>.
    /// </summary>
    public GameObject? Root {
        get => NeedleforgePlugin.hudRoots[Crest.name];
    }

    /// <summary>
    /// Called every time the HUD is created. This should be used to attach
    /// <see cref="MonoBehaviour"/>s and/or <see cref="GameObject"/>s to this crest's
    /// <see cref="Root"/> which will provide extra visuals or functionality to
    /// the HUD. To control the behaviour of these additional elements, add a
    /// <see cref="UnityEngine.Coroutine"/> to the crest.
    /// </summary>
    public event Action? OnInitializing;

    /// <inheritdoc cref="OnInitializing"/>
    internal void Initialize() => OnInitializing?.Invoke();

    /// <summary>
    /// <para>
    /// An optional coroutine function which will run continuously when this crest is
    /// equipped, and should be used to control any extra animations or visual
    /// effects for the crest's HUD frame. This function generally takes the form of
    /// an infinite "while(true)" loop which calls "yield return null" at least once
    /// per loop iteration.
    /// </para><para>
    /// Access to the HUD frame is provided for convenience, e.x. for calling
    /// <see cref="BindOrbHudFrame.PlayFrameAnim"/> to trigger extra HUD animations.
    /// Any extra elements added to the <see cref="Root"/> via
    /// <see cref="OnInitializing"/> will be available to this function as well.
    /// </para><para>
    /// For examples, see the source code of <see cref="BindOrbHudFrame"/>.
    /// </para>
    /// </summary>
    public HudCoroutine? Coroutine { get; set; }

    /// <inheritdoc cref="Coroutine"/>
    public delegate IEnumerator HudCoroutine(BindOrbHudFrame hudInstance);

}
