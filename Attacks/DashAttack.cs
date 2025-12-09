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
        /// <inheritdoc cref="AttackBase.AnimName"/>
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

        protected DashStabWithOwnAnim? dashStab;
        protected override NailAttackBase? NailAttack => dashStab;

        protected override void AddComponents(HeroController hc)
        {
            dashStab = GameObject!.AddComponent<DashStabWithOwnAnim>();
        }

        protected override void LateInitializeComponents(HeroController hc)
        {
            dashStab!.animName = AnimName;
            Collider!.enabled = false;

            if (string.IsNullOrWhiteSpace(Name))
                Name = $"Dash Stab {GameObject!.transform.GetSiblingIndex() + 1}";
        }
    }
}
