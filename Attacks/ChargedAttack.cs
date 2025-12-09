using Needleforge.Components;
using System.Collections;
using UnityEngine;

namespace Needleforge.Attacks;

/// <summary>
/// Represents the visual, auditory, and damage properties of a charged attack in a crest
/// moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </summary>
/// <remarks>
/// The type and behaviour of charged attacks are partly determined by properties of
/// <see cref="Data.MovesetData.HeroConfig"/>, and partly determined by properties of the
/// attack itself.
/// </remarks>
public class ChargedAttack : MultiStepAttack<ChargedAttack.Step>
{
    /// <inheritdoc cref="ChargedAttack"/>
    public ChargedAttack() { }

    /*
    Look into components:
    - HeroExtraNailSlash - seems to be in charge of applying nail imbuements to subordinate damaging objects and renderers.
    - HeroNailImbuementEffect - 2 of them - ...in charge of activating particle effects for fire and/or poison?
    - ScreenFlashAnimator
    - CameraShakeAnimator

    and then a list of children that i still have to inspect....
    */

    #region API

    /// <summary>
    /// Whether or not this attack's first Step will play when this attack's
    /// GameObject is activated.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool PlayOnActivation
    {
        get => _playOnActive;
        set
        {
            _playOnActive = value;
            if (GameObject)
                startOnActivation!.enabled = value;
        }
    }
    private bool _playOnActive = true;

    /// <summary>
    /// Whether or not this attack's Steps will play one another in sequence after
    /// one is played.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool PlayStepsInSequence
    {
        get => _playInSequence;
        set
        {
            _playInSequence = value;
            if (GameObject)
            {
                foreach (var attack in Steps)
                    attack.startNextStepWhenDone = value;
            }
        }
    }
    private bool _playInSequence = true;

    /// <summary>
    /// Whether or not this attack's last Step will deactivate this attack's GameObject
    /// when it's done playing.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool DisableAfterLastStep
    {
        get => _disableAfterLast;
        set
        {
            _disableAfterLast = value;
            if (GameObject)
            {
                foreach (var attack in Steps)
                    attack.disableIfLastStep = value;
            }
        }
    }
    private bool _disableAfterLast = true;

    /// <summary>
    /// Time limit in seconds after which this attack's GameObject will deactivate.
    /// If null, there is no time limit.
    /// Default is <see langword="null"/>.
    /// </summary>
    public float? DisableAfterTime
    {
        get => _disableTime;
        set
        {
            _disableTime = value;
            if (GameObject)
            {
                disableTimer!.enabled = (value != null);
                disableTimer!.waitTime = value ?? 0;
            }
        }
    }
    private float? _disableTime = null;

    /// <summary>
    /// Whether or not this attack's X position in world space will be constant after it
    /// activates, instead of following Hornet's movement.
    /// Default is <see langword="false"/>.
    /// </summary>
    public bool KeepXPosition
    {
        get => _staticX;
        set
        {
            _staticX = value;
            if (GameObject)
                keepPos!.keepX = value;
        }
    }
    private bool _staticX = false;

    /// <summary>
    /// Whether or not this attack's Y position in world space will be constant after it
    /// activates, instead of following Hornet's movement.
    /// Default is <see langword="false"/>.
    /// </summary>
    public bool KeepYPosition
    {
        get => _staticY;
        set
        {
            _staticY = value;
            if (GameObject)
                keepPos!.keepY = value;
        }
    }
    private bool _staticY = false;

    public override Step[] Steps
    {
        get => base.Steps;
        set
        {
            base.Steps = value;
            if (GameObject)
            {
                foreach (var attack in value)
                {
                    attack.disableIfLastStep = DisableAfterLastStep;
                    attack.startNextStepWhenDone = PlayStepsInSequence;
                }
            }
        }
    }

    #endregion

    protected StartChargedAttackOnActivation? startOnActivation;
    protected DisableAfterTime? disableTimer;
    protected KeepWorldPosition? keepPos;

    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        foreach (var attack in Steps)
        {
            attack.disableIfLastStep = DisableAfterLastStep;
            attack.startNextStepWhenDone = PlayStepsInSequence;
        }

        GameObject = base.CreateGameObject(parent, hc);
        GameObject.SetActive(false);

        startOnActivation = GameObject.AddComponent<StartChargedAttackOnActivation>();
        startOnActivation.enabled = PlayOnActivation;

        keepPos = GameObject.AddComponent<KeepWorldPosition>();
        keepPos.getPositionOnEnable = true;
        keepPos.resetOnDisable = true;
        keepPos.keepX = KeepXPosition;
        keepPos.keepY = KeepYPosition;

        disableTimer = GameObject.AddComponent<DisableAfterTime>(); // potentially replace this with a deactivation event on the last attack step's attack ended event...
        disableTimer.enabled = (DisableAfterTime != null);
        disableTimer.waitTime = DisableAfterTime ?? 0;

        GameObject.AddComponent<PlayRandomAudioEvent>(); // set to do uhhhhh a hornet voice of some kind on activation? requires also an AudioSource and an AudioSourceGamePause. table for this thang should be "Attack Needle Art Hornet Voice" and we need to figure out where this is stored so we can reference it tbh.
        // consider also a NoiseMaker set to create noise on enable...? with a radius of 3?????
        // each attack step should have an option to do a configurable camera shake and screen flash upon activation. look into the animators mentioned above or something idk.

        return GameObject;
    }

    /// <summary>
    /// Represents the visual, auditory, and damage properties of
    /// one part of a <see cref="ChargedAttack"/>.
    /// </summary>
    public class Step : AttackBase {

        internal bool disableIfLastStep = true;
        internal bool startNextStepWhenDone = true;

        /// <inheritdoc cref="Attack.AnimName"/>
        public override string AnimName
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

        private NailSlashWithEndEvent? nailSlash;
        protected override NailAttackBase? NailAttack => nailSlash;

        protected override void AddComponents(HeroController hc)
        {
            nailSlash = GameObject!.AddComponent<NailSlashWithEndEvent>();
        }

        protected override void LateInitializeComponents(HeroController hc)
        {
            nailSlash!.animName = AnimName;
            nailSlash!.AttackEnding += DisableIfLast;
            nailSlash!.AttackEnding += ActivateNextStep;

            if (string.IsNullOrWhiteSpace(Name))
                Name = $"Charge Step {GameObject!.transform.GetSiblingIndex() + 1}";
        }

        private void ActivateNextStep()
        {
            if (!startNextStepWhenDone)
                return;

            var transform = GameObject!.transform;
            var parent = transform.parent;
            var nextIdx = 1 + transform.GetSiblingIndex();

            for(int i = nextIdx; i < parent.childCount; i++)
            {
                if (parent.GetChild(nextIdx).TryGetComponent<NailSlashWithEndEvent>(out var nextSlash))
                {
                    nailSlash!.StartCoroutine(SlashOneFrameLater(nextSlash));
                    break;
                }
            }
        }

        private void DisableIfLast()
        {
            if (!disableIfLastStep)
                return;

            var transform = GameObject!.transform;
            var parent = transform.parent;
            var nextIdx = 1 + transform.GetSiblingIndex();

            bool shouldDisable = true;
            for(int i = nextIdx; i < parent.childCount; i++)
            {
                if (parent.GetChild(nextIdx).TryGetComponent<NailSlashWithEndEvent>(out _))
                {
                    shouldDisable = false;
                    break;
                }
            }

            if (shouldDisable)
                parent.gameObject.SetActive(false);
        }

        private static IEnumerator SlashOneFrameLater(NailSlashWithEndEvent slash)
        {
            yield return null;
            slash.StartSlash();
        }

    }

}
