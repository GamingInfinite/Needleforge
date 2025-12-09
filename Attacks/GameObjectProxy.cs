using GlobalEnums;
using UnityEngine;

namespace Needleforge.Attacks;

public abstract class GameObjectProxy
{

    /// <summary>
    /// A name for the created <see cref="GameObject"/>.
    /// <span id="prop-updates-go">
    /// Changing this property will update the <see cref="UnityEngine.GameObject"/>
    /// this object represents, if one has been created.
    /// </span>
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
