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
            if (GameObject)
            {
                foreach (var attack in value)
                    attack.CreateGameObject(GameObject, HeroController.instance);
            }
        }
    }
    private T[] _steps = [];

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
    public override GameObject CreateGameObject(GameObject parent, HeroController hc) {
        GameObject = base.CreateGameObject(parent, hc);
        GameObject.SetActive(false);

        foreach (var attack in Steps)
            attack.CreateGameObject(GameObject, hc);

        return GameObject;
    }
}
