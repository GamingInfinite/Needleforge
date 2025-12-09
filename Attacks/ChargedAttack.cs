using Needleforge.Components;
using UnityEngine;

namespace Needleforge.Attacks;

public class ChargedAttack : GameObjectProxy {

	/*
	Look into components:
	- HeroExtraNailSlash - seems to be in charge of applying nail imbuements to subordinate damaging objects and renderers.
	- HeroNailImbuementEffect - 2 of them - ...in charge of activating particle effects for fire and/or poison?
	- ScreenFlashAnimator
	- CameraShakeAnimator
	- KeepWorldPosition

	and then a list of children that i still have to inspect....
	*/

	public bool PlayStepsInSequence {
		get => _playInSequence;
		set {
			_playInSequence = value;
			if (GameObject)
				foreach (var attack in AttackSteps)
					attack.StartNextStepOnEnd = value;
		}
	}
	private bool _playInSequence = true;

	/// <summary>
	/// Sets the AnimLibrary for all <see cref="AttackSteps"/> belonging to this Attack.
	/// </summary>
	public void SetAnimLibrary(tk2dSpriteAnimation value) {
		foreach (var attack in AttackSteps)
			attack.AnimLibrary = value;
	}


	protected DisableAfterTime? disabler;

	public override GameObject CreateGameObject(GameObject parent, HeroController hc) {
		GameObject = base.CreateGameObject(parent, hc);
		GameObject.SetActive(false);

		GameObject.AddComponent<StartChargedAttackOnActivation>();

		disabler = GameObject.AddComponent<DisableAfterTime>();
		disabler.waitTime = 1;

		foreach (var attack in AttackSteps){
			attack.StartNextStepOnEnd = PlayStepsInSequence;
			attack.CreateGameObject(GameObject, hc);
		}

		return GameObject;
	}


	public AttackStep[] AttackSteps = [];

	public class AttackStep : AttackBase, IAttackWithOwnEffectAnim {
		public string AnimName
		{
			get => _animName;
			set
			{
				_animName = value;
				if (GameObject)
					nailSlash!.animName = value;
			}
		}
		private string _animName = "";

		public bool StartNextStepOnEnd
		{
			get => _startNext;
			set
			{
				if (GameObject)
				{
					if (value && !_startNext)
						nailSlash!.AttackEnding += ActivateNextStep;
					else if (!value)
						nailSlash!.AttackEnding -= ActivateNextStep;
				}
				_startNext = value;
			}
		}
		private bool _startNext = true;

		private NailSlashWithEndEvent? nailSlash;
		protected override NailAttackBase? NailAttack => nailSlash;

		protected override void AddComponents(HeroController hc)
		{
			nailSlash = GameObject!.AddComponent<NailSlashWithEndEvent>();
		}

		protected override void LateInitializeComponents(HeroController hc)
		{
			nailSlash!.animName = AnimName;
			if (StartNextStepOnEnd)
				nailSlash!.AttackEnding += ActivateNextStep;
		}

		private void ActivateNextStep()
		{
			Debug.Log("weeeeeeeeeee");
			var transform = GameObject!.transform;
			var parent = transform.parent;
			var nextIdx = 1 + transform.GetSiblingIndex();
			Debug.Log($"weeeeeeeeeee nextidx is {nextIdx}");
			NailSlash? nextSlash = null;
			while (nextIdx < parent.childCount && !parent.GetChild(nextIdx).TryGetComponent<NailSlash>(out nextSlash)) {
				Debug.Log("a");
				nextIdx++;
			}
			if (nextSlash)
			{
				nextSlash.StartSlash();
			}
		}
	}

}
