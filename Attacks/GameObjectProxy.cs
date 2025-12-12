using UnityEngine;

namespace Needleforge.Attacks;

/// <summary>
/// Represents a <see cref="UnityEngine.GameObject"/> and provides an API to set up its
/// components before its actual creation.
/// </summary>
/// <remarks>
/// All API properties' setters should update the <see cref="GameObject"/>, if it exists.
/// </remarks>
public abstract class GameObjectProxy
{

	/// <summary>
	/// A name for the created <see cref="UnityEngine.GameObject"/>.
	/// </summary>
	public string Name
    {
        get => _name;
        set
        {
            _name = value;
            if (GameObject)
                GameObject.name = value;
        }
    }
    private string _name = "";

    /// <summary>
    /// After this object has been initialized, this will reference
    /// the <see cref="UnityEngine.GameObject"/> that this object represents.
    /// </summary>
    /// <remarks>
    /// Modifications made directly to this object will not be reproduced if a player
    /// quits to menu and loads a new save.
    /// </remarks>
    public GameObject? GameObject { get; protected set; }

	/// <summary>
	/// Creates and sets up a <see cref="UnityEngine.GameObject"/> with the properties
	/// specified on this object.
	/// If this object was already created, the old GameObject is destroyed; be sure to
	/// update all old references to the new GameObject.
	/// </summary>
	/// <param name="parent">A parent for the created GameObject.</param>
	/// <param name="hc">
	///     A reference to the current HeroController, which is often needed
	///     for initialization.
	/// </param>
	/// <returns>
    ///     The same <see cref="UnityEngine.GameObject"/> now referenced by
    ///     the GameObject property.
    /// </returns>
	public virtual GameObject CreateGameObject(GameObject parent, HeroController hc) {
        if (GameObject)
            Object.DestroyImmediate(GameObject);

        GameObject = new(Name);
        GameObject.transform.SetParent(parent.transform);
        GameObject.transform.localPosition = Vector3.zero;
        GameObject.transform.localScale = Vector3.one;

        return GameObject;
    }
}
