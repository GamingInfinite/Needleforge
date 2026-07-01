using Needleforge.Data;
using UnityEngine;

namespace Needleforge.Attacks;

/// <summary>
/// Represents an already existing attack with a NailSlash component, such as vanilla slashes, that will be cloned.
/// Used mainly to allow existing GameObjects to be used as a GameObjectProxy.
/// </summary>
public class ClonedAttack : GameObjectProxy
{
    #region API
    /// <summary>
    /// The <see cref="GameObject"/> that this attack will clone. 
    /// This can be set to an existing attack's GameObject, such as a vanilla slash.
    /// </summary>
    public GameObject? OriginalObject { get; set; }
    #endregion

    /// <summary>
    /// Creates a clone of the <see cref="OriginalObject"/> and sets it up as an attack.
    /// The OriginalObject should have a NailSlash component on it, but may still work without one.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="hc"></param>
    /// <returns></returns>
    public override GameObject CreateGameObject(GameObject parent, HeroController hc) {
        if (OriginalObject == null)
        {
            ModHelper.LogWarning("A ClonedAttack was asked to create a GameObject, but its OriginalObject property was null. Returning an empty GameObject instead.");
            return new GameObject();
        }

        GameObject clonedSlash = Object.Instantiate(OriginalObject, parent.transform);

        #region Fixing cloned attack
        NailAttackBase slash = clonedSlash.GetComponent<NailAttackBase>();
        NailSlashRecoil nailSlashRecoil = clonedSlash.GetComponent<NailSlashRecoil>();
        if (nailSlashRecoil != null)
        {
            nailSlashRecoil.heroCtrl = hc;
        }
        
        if (slash != null)
        {
            slash.hc = hc;
            slash.enabled = true;
        }
        #endregion

        if (GameObject)
            Object.DestroyImmediate(GameObject);

        GameObject = clonedSlash;
        GameObject.name = Name;
        GameObject.transform.SetParent(parent.transform);
        // POTENTIAL TODO: Figure out a way to have a custom scale, rotation,
        // and localposition without defaulting to 0,0,0 and identity rotation.
        // For now, this will do since the usecase is charged and downslashes,
        // which should just match original and can be changed in the Initialized event
        // of their crest.
        Position = OriginalObject.transform.localPosition;
        Scale = OriginalObject.transform.localScale;
        Rotation = OriginalObject.transform.localRotation;

        return GameObject;
    }
}
