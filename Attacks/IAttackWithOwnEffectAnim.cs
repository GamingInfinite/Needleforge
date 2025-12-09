namespace Needleforge.Attacks;

/// <summary>
/// Specifies that an Attack class controls when its own effect animation plays,
/// and therefore holds a reference to the name of the appropriate animation.
/// </summary>
public interface IAttackWithOwnEffectAnim
{
    public string AnimName { get; set; }
}
