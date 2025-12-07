using Needleforge.Components;
using System.IO;
using UnityEngine;

namespace Needleforge.Data;

public class DashAttack : GameObjectProxy
{
    #region API

    /// <summary>
    /// Defines the visual and damage properties of each step of a multi-step dash attack.
    /// </summary>
    /// <remarks>
    /// All members of this array should be different objects; references to the same object
    /// multiple times can cause incorrect behaviour.
    /// </remarks>
    public AttackStep[] AttackSteps
    {
        get => _attackSteps;
        set
        {
            _attackSteps = value;
            if (GameObject)
            {
                foreach (var attack in value)
                    attack.CreateGameObject(GameObject, HeroController.instance);
            }
        }
    }
    private AttackStep[] _attackSteps = [];

	/// <summary>
    /// Sets the AnimLibrary for all <see cref="AttackSteps"/> belonging to this Attack.
	/// </summary>
	/// <remarks>
	/// <inheritdoc cref="AttackBase.AnimLibrary" path="//*[@id='anim-info']"/>
	/// <para>
	/// Note that the <see cref="HeroControllerConfig.heroAnimOverrideLib"/> in the associated <see cref="MovesetData.HeroConfig"/> should contain animations named "Dash Attack Antic X" and "Dash Attack X", where X is a number starting from 1, for as many <see cref="AttackSteps"/> as the animation has.
	/// </para>
	/// </remarks>
	public void SetAnimLibrary(tk2dSpriteAnimation value)
    {
        foreach(var attack in AttackSteps)
            attack.AnimLibrary = value;
    }

    #endregion

    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        GameObject = base.CreateGameObject(parent, hc);
        GameObject.SetActive(false);

		foreach (var attack in AttackSteps)
            attack.CreateGameObject(GameObject, hc);

		GameObject.SetActive(true);
        return GameObject;
    }


    public class AttackStep : AttackBase, IAttackWithOwnEffectAnim
    {
		public string AnimName {
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
		}
	}
}
