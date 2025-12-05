
namespace Needleforge.Data;

/// <summary>
/// Specifies that an Attack class requires a reference to a valid
/// <see cref="HeroControllerConfig"/> (or derived class) in order to create
/// its GameObject(s).
/// </summary>
public interface IAttackWithHeroConfigAccess
{
    public HeroControllerConfig? HeroConfig { get; set; }
}
