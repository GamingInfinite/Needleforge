using Needleforge.Patches.HUD;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using BasicFrameAnims = BindOrbHudFrame.BasicFrameAnims;

namespace Needleforge.Data;

public class HudFrameData
{
    internal readonly CrestData Crest;

    public HudFrameData(CrestData owner)
    {
        Crest = owner;
        ExtraAnims.CollectionChanged += ExtraAnimsChanged;
        SteelExtraAnims.CollectionChanged += ExtraAnimsChanged;
    }

    private void ExtraAnimsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Reset:
                AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
                break;
            case NotifyCollectionChangedAction.Move:
                // We do not care about the list order, only what's in it
                break;
        }
    }

    #region Sprites

    /// <summary>
    /// Crest icon which will appear in the corner of regular save files on the menu.
    /// </summary>
    public Sprite? ProfileIcon { get; set; }

    
    /// <summary>
    /// Crest icon which will appear in the corner of steel soul save files on the menu.
    /// </summary>
    public Sprite? SteelProfileIcon { get; set; }

    #endregion

    #region Animations

    /// <summary>
    /// <para>
    /// Changes the look of this crest's HUD frame to match one of the base game crests.
    /// This doesn't include unique animations like Beast's rage mode, which must be
    /// controlled with a <see cref="Coroutine"/>.
    /// </para><para>
    /// This preset will be completely overridden if <b>any</b> custom animations are set
    /// on <see cref="Appear"/>, <see cref="AppearFromNone"/>, <see cref="Idle"/>, or
    /// <see cref="Disappear"/>.
    /// </para>
    /// </summary>
    public VanillaCrest Preset { get; set; } = VanillaCrest.HUNTER;

    private tk2dSpriteAnimationClip?
        appear, appearNone, idle, disappear,
        steelAppear, steelAppearNone, steelIdle, steelDisappear;

    /// <summary>
    /// A custom animation for when this crest is equipped.
    /// <reqs>All custom HUD animations must have a unique name.</reqs>
    /// </summary>
    public tk2dSpriteAnimationClip? Appear
    {
        get => appear;
        set {
            appear = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation for when this crest is equipped in <b>steel soul</b> mode.
    /// <reqs>All custom steel soul HUD animations must have a name identical to their
    /// regular counterpart. If a custom animation has no steel soul version, the regular
    /// version will be used in its place.</reqs>
    /// </summary>
    public tk2dSpriteAnimationClip? SteelAppear {
        get => steelAppear;
        set {
            steelAppear = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation for when the HUD first appears upon loading a save where
    /// this crest is equipped.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? AppearFromNone
    {
        get => appearNone;
        set {
            appearNone = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation for when the HUD first appears upon loading a
    /// <b>steel soul</b> save where this crest is equipped.
    /// <inheritdoc cref="SteelAppear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? SteelAppearFromNone {
        get => steelAppearNone;
        set {
            steelAppearNone = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation which determines this crest's idle appearance.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? Idle
    {
        get => idle;
        set {
            idle = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation which determines this crest's idle appearance in <b>steel
    /// soul</b> mode.
    /// <inheritdoc cref="SteelAppear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? SteelIdle {
        get => steelIdle;
        set {
            steelIdle = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation for when this crest is unequipped.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? Disappear
    {
        get => disappear;
        set {
            disappear = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// A custom animation for when this crest is unequipped in <b>steel soul</b> mode.
    /// <inheritdoc cref="SteelAppear" path="/summary/reqs"/>
    /// </summary>
    public tk2dSpriteAnimationClip? SteelDisappear {
        get => steelDisappear;
        set {
            steelDisappear = value;
            AddHudRootsAndAnims.UpdateHudAnimLibrary(this);
        }
    }

    /// <summary>
    /// Any extra custom animations for this crest's HUD.
    /// <inheritdoc cref="Appear" path="/summary/reqs"/>
    /// Add a <see cref="Coroutine"/> to use these animations.
    /// </summary>
    public ObservableCollection<tk2dSpriteAnimationClip> ExtraAnims { get; } = [];

    /// <summary>
    /// Any extra custom animations for this crest's HUD in <b>steel soul</b> mode.
    /// <inheritdoc cref="SteelAppear" path="/summary/reqs"/>
    /// Add a <see cref="Coroutine"/> to use these animations.
    /// </summary>
    public ObservableCollection<tk2dSpriteAnimationClip> SteelExtraAnims { get; } = [];

    #region Animations - Internal Helpers
    internal IEnumerable<tk2dSpriteAnimationClip> AllRegularCustomAnims() =>
        new List<tk2dSpriteAnimationClip?>
        {
            Appear, AppearFromNone, Idle, Disappear
        }
        .Where(x => x != null)
        .Cast<tk2dSpriteAnimationClip>()
        .Concat(ExtraAnims.Distinct());

    internal IEnumerable<tk2dSpriteAnimationClip> AllSteelCustomAnims() =>
        new List<tk2dSpriteAnimationClip?>
        {
            SteelAppear, SteelAppearFromNone, SteelIdle, SteelDisappear
        }
        .Where(x => x != null)
        .Cast<tk2dSpriteAnimationClip>()
        .Concat(SteelExtraAnims.Distinct());

    internal bool HasRegularCustomBasicAnims =>
        Appear != null
        || AppearFromNone != null
        || Idle != null
        || Disappear != null;

    internal bool HasSteelCustomBasicAnims =>
        SteelAppear != null
        || SteelAppearFromNone != null
        || SteelIdle != null
        || SteelDisappear != null;

    internal bool HasAnyRegularCustomAnims =>
        HasRegularCustomBasicAnims || ExtraAnims.Count > 0;
    
    internal bool HasAnySteelCustomAnims =>
        HasSteelCustomBasicAnims || SteelExtraAnims.Count > 0;

    internal BasicFrameAnims CustomBasicFrameAnims() =>
        new()
        {
            Appear = Appear?.name ?? "",
            AppearFromNone = AppearFromNone?.name ?? "",
            Idle = Idle?.name ?? "",
            Disappear = Disappear?.name ?? "",
        };
    #endregion

    #endregion

    #region Structure & Behaviour

    /// <summary>
    /// <para>
    /// After the HUD has been created, this returns a dedicated GameObject for this
    /// crest which is attached to the HUD. This can be used to add extra HUD elements
    /// which require separate control from the animations; for an example, Hunter's
    /// combo meters. Add a <see cref="Coroutine"/> to control them during gameplay.
    /// </para><para>
    /// This will be destroyed and recreated every time the player quits to the main
    /// menu; any modifications or additions to this GameObject should be made in a
    /// handler attached to <see cref="OnRootCreated"/>.
    /// </para>
    /// </summary>
    public GameObject? Root
    {
        get {
            NeedleforgePlugin.hudRoots.TryGetValue(Crest.name, out var result);
            return result;
        }
    }

    /// <summary>
    /// Called every time the HUD is created. This should be used to attach extra HUD
    /// elements which require separate control from the animations to your crest's
    /// <see cref="Root"/>. To control the behaviour of these additional elements, add a
    /// <see cref="Coroutine"/>.
    /// </summary>
    public event Action? OnRootCreated;

    /// <inheritdoc cref="OnRootCreated"/>
    internal void InitializeRoot() => OnRootCreated?.DynamicInvoke();

    /// <summary>
    /// <para>
    /// An optional coroutine function which will run continuously when this crest is
    /// equipped, and should be used to control any extra animations or visual
    /// effects for the crest's HUD frame. This function generally takes the form of
    /// an infinite "while(true)" loop which calls "yield return null" at least once
    /// per loop iteration.
    /// </para><para>
    /// Access to the HUD frame component is provided for convenience, e.x. for calling
    /// <see cref="BindOrbHudFrame.PlayFrameAnim"/> to trigger extra HUD animations that
    /// were added to <see cref="ExtraAnims"/>.
    /// Any extra elements added to the <see cref="Root"/> via
    /// <see cref="OnRootCreated"/> will also be available to this function.
    /// </para><para>
    /// For examples, see the source code of <see cref="BindOrbHudFrame"/>.
    /// </para>
    /// </summary>
    public HudCoroutine? Coroutine { get; set; }

    /// <inheritdoc cref="Coroutine"/>
    public delegate IEnumerator HudCoroutine(BindOrbHudFrame hudInstance);

    #endregion

}
