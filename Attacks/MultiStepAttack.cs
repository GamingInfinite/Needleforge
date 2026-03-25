using Needleforge.Components;
using System.Linq;
using UnityEngine;

namespace Needleforge.Attacks;

/// <summary>
/// Represents an attack in a crest moveset which is comprised of several subordinate
/// attack objects which represent an individual step in the overall move.
/// </summary>
/// <typeparam name="T">The type of the subordinate attacks.</typeparam>
public abstract class MultiStepAttack<T> : GameObjectProxy where T : AttackBase
{
    #region API

    /// <summary>
    /// Defines the visual, auditory, and damage properties of each step of
    /// a multi-step attack.
    /// </summary>
    /// <remarks>
    /// All members of this array should be different objects; references to the same
    /// object multiple times will cause incorrect behaviour.
    /// </remarks>
    public virtual T[] Steps
    {
        get => _steps;
        set
        {
            _steps = value;
            if (GameObject) SyncSteps();

            if (_steps.IsNullOrEmpty())
                ModHelper.LogWarning($"{GetType().Name}.{nameof(Steps)} is null or " +
                    $"empty; the attack won't do anything.", true);
        }
    }
    private T[] _steps = [];
    private T[] oldSteps = [];

    /// <summary>
    /// Sets the <see cref="AttackBase.AnimLibrary"/> of all Steps of this attack.
    /// </summary>
    public void SetAnimLibrary(tk2dSpriteAnimation value) {
        foreach (var attack in Steps)
            attack.AnimLibrary = value;
    }

    /// <summary>
    /// Sets the <see cref="AttackBase.Color"/> of all Steps of this attack.
    /// </summary>
    public void SetColor(Color value) {
        foreach (var attack in Steps)
            attack.Color = value;
    }

    #endregion

    /// <inheritdoc/>
    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        GameObject = base.CreateGameObject(parent, hc);
        GameObject.SetActive(false);

        foreach (var attack in Steps)
            attack.CreateGameObject(GameObject, hc);

        oldSteps = [.. _steps];
        parent.AddComponentIfNotPresent<UpdateRunner>().OnUpdate += SyncSteps;

        return GameObject;
    }

    /// <summary>
    /// Ensures the actual GameObjects that the attack's Steps are proxying reflect
    /// the contents of the Steps array if it changes mid-game.
    /// </summary>
    /// <remarks>
    /// Didn't make the Steps property an ObservableCollection, don't want to introduce
    /// a binary breaking change, so... reinventing the wheel it is!
    /// </remarks>
    private void SyncSteps()
    {
        if (!oldSteps.SequenceEqual(Steps))
        {
            int i = 0;
            foreach (var attack in Steps)
            {
                if (
                    !oldSteps.Contains(attack) || !attack.GameObject
                    || attack.GameObject.transform.parent.gameObject != GameObject!
                ) {
                    attack.CreateGameObject(GameObject!, HeroController.instance);
                }
                attack.GameObject!.transform.SetSiblingIndex(i++);
            }
        }
        oldSteps = [.. Steps];
    }

}
