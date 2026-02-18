
namespace Needleforge.Components;

/// <summary>
/// A <see cref="Downspike"/> component with its own custom bounce configuration.
/// </summary>
public class DownspikeWithBounceConfig : Downspike
{
    /// <summary>
    /// Controls the feel of the bounce when this down attack hits some obstacles/enemies.
    /// </summary>
    public HeroSlashBounceConfig bounceConfig;
}
