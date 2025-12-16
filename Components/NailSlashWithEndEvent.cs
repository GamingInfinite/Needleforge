using System;

namespace Needleforge.Components;

/// <summary>
/// A <see cref="NailSlash"/> component which has an event for when the attack ends.
/// </summary>
/// <remarks>
/// Note that the event will not run if the object reference is cast to a base type when
/// StartSlash is called.
/// </remarks>
public class NailSlashWithEndEvent : NailSlash
{
    /// <summary>
    /// Event which occurs when the animation for an attack completes.
    /// </summary>
    public event Action? AttackEnding;

    /// <summary>
    /// Override of <see cref="NailSlash.StartSlash"/> which invokes <see cref="AttackEnding"/>
    /// when the attack animation completes.
    /// </summary>
    public new void StartSlash()
    {
        base.StartSlash();
        anim.AnimationCompleted = this.OnAnimationCompleted;
    }

    /// <summary>
    /// Override of <see cref="NailSlash.PlaySlash"/> which invokes <see cref="AttackEnding"/>
    /// when the attack animation completes.
    /// </summary>
    public new void PlaySlash()
    {
        base.PlaySlash();
        anim.AnimationCompleted = this.OnAnimationCompleted;
    }

    /// <summary>
    /// Animation completed event handler which invokes <see cref="AttackEnding"/>.
    /// </summary>
    public new void OnAnimationCompleted(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
    {
        base.OnAnimationCompleted(animator, clip);
        AttackEnding?.Invoke();
    }
}
