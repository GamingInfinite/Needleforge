using Needleforge.Components;
using UnityEngine;

namespace Needleforge.Attacks;

/// <summary>
/// Represents the visual, auditory, and damage properties of a dash attack in a crest
/// moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </summary>
/// <remarks>
/// The type and behaviour of dash attacks are determined by properties of
/// <see cref="Data.MovesetData.HeroConfig"/>.
/// </remarks>
public class DashAttack : MultiStepAttack<DashAttack.Step>
{
    /// <inheritdoc cref="DashAttack"/>
    public DashAttack() { }

    /// <inheritdoc/>
    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        GameObject = base.CreateGameObject(parent, hc);
        GameObject.SetActive(true);
        return GameObject;
    }

    /// <summary>
    /// Represents the visual, auditory, and damage properties of
    /// one part of a <see cref="DashAttack"/>.
    /// </summary>
    public class Step : AttackBase
    {
        /// <inheritdoc/>
        /// <remarks>
        /// Effect animations for these attacks should not loop.
        /// </remarks>
        public override string AnimName {
            get => _animName;
            set {
                _animName = value;
                if (GameObject)
                    dashStab!.animName = value;
            }
        }
        private string _animName = "";

        /// <summary>
        /// The component responsible for animating the attack and de/activating its hitbox.
        /// </summary>
        protected DashStabWithOwnAnim? dashStab;

        /// <inheritdoc/>
        protected override NailAttackBase? NailAttack => dashStab;

        /// <inheritdoc/>
        protected override void AddComponents(HeroController hc)
        {
            dashStab = GameObject!.AddComponent<DashStabWithOwnAnim>();
        }

        /// <inheritdoc/>
        protected override void LateInitializeComponents(HeroController hc)
        {
            dashStab!.animName = AnimName;
            Collider!.enabled = false;

            if (string.IsNullOrWhiteSpace(Name))
                Name = $"Dash Stab {GameObject!.transform.GetSiblingIndex() + 1}";
        }
    }
}
