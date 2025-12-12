using UnityEngine;

namespace Needleforge.Components;

/// <summary>
/// A <see cref="DashStabNailAttack"/> component which plays its own sound effects
/// and effect animations.
/// </summary>
public class DashStabWithOwnAnim : DashStabNailAttack
{
    public string animName = "";

    private AudioSource audio;
    private PolygonCollider2D poly;
    private MeshRenderer mesh;

    public override void Awake()
    {
        base.Awake();
        audio = transform.GetComponent<AudioSource>();
        poly = transform.GetComponent<PolygonCollider2D>();
        mesh = transform.GetComponent<MeshRenderer>();
    }

    public void StartSlash()
    {
        OnSlashStarting();
        audio.Play();
        OnPlaySlash();
    }

    public override void OnSlashStarting()
    {
        base.OnSlashStarting();
        poly.enabled = true;
        clashTinkPoly.enabled = true;
        IsDamagerActive = true;
        mesh.enabled = true;
        animator.PlayFromFrame(animName, 0);
    }

    public override void OnAttackCancelled()
    {
        poly.enabled = false;
        clashTinkPoly.enabled = false;
        IsDamagerActive = false;
        mesh.enabled = false;
    }
}
