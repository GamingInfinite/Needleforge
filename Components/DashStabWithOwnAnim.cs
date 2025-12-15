using UnityEngine;

namespace Needleforge.Components;

/// <summary>
/// A <see cref="DashStabNailAttack"/> component which plays its own sound effects
/// and effect animations.
/// </summary>
public class DashStabWithOwnAnim : DashStabNailAttack
{
    /// <summary>
    /// The name of the animation clip in this GameObject's <see cref="tk2dSpriteAnimator"/>'s
    /// library to use as an effect animation for an attack.
    /// </summary>
    public string animName = "";

    private AudioSource audio;
    private PolygonCollider2D poly;
    private MeshRenderer mesh;

    #pragma warning disable CS1591 // Missing XML comment
    public override void Awake()
    #pragma warning restore CS1591 // Missing XML comment
    {
        base.Awake();
        audio = transform.GetComponent<AudioSource>();
        poly = transform.GetComponent<PolygonCollider2D>();
        mesh = transform.GetComponent<MeshRenderer>();
    }

	/// <summary>
	/// Convenience method similar to <see cref="NailSlash.StartSlash"/>, which calls
    /// <see cref="OnSlashStarting"/> and <see cref="NailAttackBase.OnPlaySlash"/> and
    /// plays the sound effect for the attack.
	/// </summary>
	public void StartSlash()
    {
        OnSlashStarting();
        audio.Play();
        OnPlaySlash();
    }

    /// <summary>
    /// Enables the GameObject's renderers and and plays its animation.
    /// </summary>
    public override void OnSlashStarting()
    {
        base.OnSlashStarting();
        poly.enabled = true;
        clashTinkPoly.enabled = true;
        IsDamagerActive = true;
        mesh.enabled = true;
        animator.PlayFromFrame(animName, 0);
    }

    /// <summary>
    /// Disables the GameObject's hitboxes and renderers.
    /// </summary>
    public override void OnAttackCancelled()
    {
        poly.enabled = false;
        clashTinkPoly.enabled = false;
        IsDamagerActive = false;
        mesh.enabled = false;
    }
}
