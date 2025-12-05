using System.IO;
using UnityEngine;

namespace Needleforge.Data;

public class DashAttack : AttackBase, IAttackWithHeroConfigAccess
{
    #region API

    /// <summary>
    /// Defines the visual and damage properties of each step of a multi-step dash attack.
    /// </summary>
    /// <remarks>
    /// The length of this array should match <see cref="HeroControllerConfig.dashStabSteps"/>
    /// and all members should be different objects; references to the same object
    /// multiple times can cause incorrect behaviour.
    /// </remarks>
    public AttackStep[] AttackSteps { get; set; } = [];

    /// <summary>
    /// A reference to the library where this attack's effect animations are found.
    /// <inheritdoc cref="AttackBase.Name" path="//*[@id='prop-updates-go']"/>
    /// This will also update all objects in <see cref="AttackSteps"/> and their GameObjects.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="AttackBase.AnimLibrary" path="//*[@id='anim-info']"/>
    /// <para>
    /// If the associated <see cref="MovesetData.HeroConfig"/> doesn't set an FSM edit
    /// for dash attacks, which would control which animations play,
    /// Hornet's <see cref="HeroControllerConfig.heroAnimOverrideLib"/> and this attack's
    /// animation library <i>must</i> contain a set of animations with specific names.
    /// </para><para>
    /// Single-step attacks require "Dash Attack Antic" and "Dash Attack" for Hornet,
    /// and "DashStabEffect" for the effect.
    /// </para><para>
    /// Multi-step attacks require "Dash Attack Antic X", "Dash Attack X", and
    /// "DashStabEffect X" for each step of the attack, where X is a number starting from 1.
    /// </para>
    /// </remarks>
    public override tk2dSpriteAnimation? AnimLibrary
    {
        get => _animLibrary;
        set
        {
            _animLibrary = value;
            if (GameObject)
            {
                if (HeroConfig!.dashStabSteps <= 1)
                    Animator!.Library = value;
                else
                {
                    foreach(var attack in AttackSteps)
                        attack.AnimLibrary = value;
                }
            }
        }
    }
    private tk2dSpriteAnimation? _animLibrary = null;

    #endregion

    public HeroControllerConfig? HeroConfig { get; set; }


    protected DashStabNailAttack? dashStabAttack;
    protected override NailAttackBase? NailAttack => dashStabAttack;

    protected override void AddComponents(HeroController hc)
    {
        dashStabAttack = GameObject!.AddComponent<DashStabNailAttack>();
    }

    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        if (!HeroConfig)
        {
            throw new System.InvalidOperationException(
                $"DashAttacks must be provided a valid {nameof(HeroConfig)} to initialize properly."
            );
        }

        if (HeroConfig.dashStabSteps <= 1)
            return base.CreateGameObject(parent, hc);

        GameObject = new(Name);
        Object.DontDestroyOnLoad(GameObject);
        GameObject.transform.SetParent(parent.transform);
        GameObject.transform.localPosition = Vector3.zero;
        GameObject.SetActive(false);

        for(int i = 0; i < AttackSteps.Length; i++) {
            AttackSteps[i].Name = $"Dash Stab {i + 1}";
            AttackSteps[i].CreateGameObject(GameObject, hc);
            if (!AttackSteps[i].AnimLibrary)
                AttackSteps[i].AnimLibrary = AnimLibrary;
        }

        GameObject.SetActive(true);
        return GameObject;
    }

    public class AttackStep : AttackBase
    {
        protected DashStabNailAttack? dashStabAttack;
        protected override NailAttackBase? NailAttack => dashStabAttack;
        protected override void AddComponents(HeroController hc)
        {
            dashStabAttack = GameObject!.AddComponent<DashStabNailAttack>();
        }
    }
}
