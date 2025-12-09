using System;

namespace Needleforge.Components;

public class NailSlashWithEndEvent : NailSlash
{
    public event Action? AttackEnding;

    public new void StartSlash()
    {
        base.StartSlash();
        anim.AnimationCompleted = this.OnAnimationCompleted;
    }

    public new void PlaySlash()
    {
        base.PlaySlash();
		anim.AnimationCompleted = this.OnAnimationCompleted;
	}

    public new void OnAnimationCompleted(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
    {
        base.OnAnimationCompleted(animator, clip);
        AttackEnding?.Invoke();
    }
}
