using GlobalSettings;
using Needleforge.Components;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using Camera = GlobalSettings.Camera;

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
    public ChargedAttack() {
        ScreenFlashColors.CollectionChanged += ScreenFlashColorsChanged;
        CameraShakeProfiles.CollectionChanged += CameraShakeProfilesChanged;
    }

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
    /// <remarks>
    /// A charged attack's GameObject should always be disabled when not attacking. It's
    /// recommended to keep one of the automatic disable options on unless you're making
    /// an FSM edit which disables the GameObject manually.
    /// </remarks>
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
    /// <inheritdoc cref="DisableAfterLastStep" path="/remarks"/>
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

    /// <summary>
    /// Which <see cref="RandomAudioClipTable"/> of Hornet voice clips from the vanilla
    /// game's charged attacks to use for this attack.
    /// Overidden by <see cref="VoiceTable"/>.
    /// </summary>
    public VanillaVoiceTable VoiceTablePreset
    {
        get => _voicePreset;
        set
        {
            _voicePreset = value;
            if (GameObject && !VoiceTable)
                playRandomAudio!.table = GetPresetAudioTable(HeroController.instance, value);
        }
    }
    private VanillaVoiceTable _voicePreset = VanillaVoiceTable.DEFAULT;

    /// <summary>
    /// A <see cref="RandomAudioClipTable"/> of Hornet voice clips to pick from when
    /// this attack is activated.
    /// Overiddes <see cref="VoiceTablePreset"/>.
    /// </summary>
    public RandomAudioClipTable? VoiceTable
    {
        get => _voiceTable;
        set
        {
            _voiceTable = value;
            if (GameObject)
                playRandomAudio!.table = value;
        }
    }
    private RandomAudioClipTable? _voiceTable = null;

    /// <summary>
    /// Whether or not this attack is marked as creating noise which NPCs and enemies
    /// hear and react to.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool MakesNoise
    {
        get => _makesNoise;
        set
        {
            _makesNoise = value;
            if (GameObject)
                noiseMaker!.enabled = value;
        }
    }
    private bool _makesNoise = true;

    /// <summary>
    /// <see cref="CameraShakeProfile"/>s which members of <see cref="Steps"/> use to
    /// create a camera shake when they're played. See <see cref="Step.CameraShakeIndex"/>.
    /// </summary>
    /// <remarks>
    /// The profiles the game uses can be found in <see cref="Camera"/>.
    /// The standard profile for charged attacks is <see cref="Camera.EnemyKillShake"/>.
    /// </remarks>
    public ObservableCollection<CameraShakeProfile> CameraShakeProfiles
    {
        get => _cameraShakeProfiles;
        set
        {
            _cameraShakeProfiles = value;
            _cameraShakeProfiles.CollectionChanged += CameraShakeProfilesChanged;
            if (GameObject)
                cameraShaker!.cameraShakeTargets = CreateShakeTargets(value);
        }
    }
    private ObservableCollection<CameraShakeProfile> _cameraShakeProfiles = [];

    /// <summary>
    /// Colors which members of <see cref="Steps"/> use to create a screen flashes when
    /// they're played. See <see cref="Step.ScreenFlashIndex"/>.
    /// </summary>
    public ObservableCollection<Color> ScreenFlashColors
    {
        get => _screenFlashColors;
        set
        {
            _screenFlashColors = value;
            _screenFlashColors.CollectionChanged += ScreenFlashColorsChanged;
            if (GameObject)
                screenFlasher!.screenFlashColours = [.. value];
        }
    }

    private ObservableCollection<Color> _screenFlashColors = [];

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

    protected NoiseMaker? noiseMaker;
    protected PlayRandomAudioEvent? playRandomAudio;
    protected AudioSource? audio;
    protected AudioSourceGamePause? audioPause;

    protected CameraShakeAnimator? cameraShaker;
    protected ScreenFlashAnimator? screenFlasher;

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

        disableTimer = GameObject.AddComponent<DisableAfterTime>();
        disableTimer.enabled = (DisableAfterTime != null);
        disableTimer.waitTime = DisableAfterTime ?? 0;

        noiseMaker = GameObject.AddComponent<NoiseMaker>();
        noiseMaker.radius = 3;
        noiseMaker.intensity = NoiseMaker.Intensities.Normal;
        noiseMaker.allowOffScreen = true;
        noiseMaker.createNoiseOnEnable = true;
        noiseMaker.enabled = MakesNoise;

        audio = GameObject.AddComponent<AudioSource>();
        audioPause = GameObject.AddComponent<AudioSourceGamePause>();

        playRandomAudio = GameObject.AddComponent<PlayRandomAudioEvent>();
        playRandomAudio.playOnEnable = true;
        playRandomAudio.useOwnAudio = true;
        playRandomAudio.table = VoiceTable ? VoiceTable : GetPresetAudioTable(hc, VoiceTablePreset);

        cameraShaker = GameObject.AddComponent<CameraShakeAnimator>();
        cameraShaker.cameraShakeTargets = CreateShakeTargets(CameraShakeProfiles);

        screenFlasher = GameObject.AddComponent<ScreenFlashAnimator>();
        screenFlasher.screenFlashColours = [.. ScreenFlashColors];

        return GameObject;
    }

    #region Utils

    private static RandomAudioClipTable GetPresetAudioTable(HeroController hc, VanillaVoiceTable preset)
    {
        string objName = preset switch
        {
            VanillaVoiceTable.BEAST_RAGE => "Warrior Rage",
            _ => "Basic",
        };

        return hc.transform
            .Find($"Attacks/Charge Slash {objName}/Charge Slash Hornet Voice")
            .GetComponent<PlayRandomAudioEvent>()
            .table;
    }

    private static CameraShakeTarget[] CreateShakeTargets(IEnumerable<CameraShakeProfile> profiles)
    {
        return [.. profiles.Select(x => new CameraShakeTarget() {
            camera = Camera.MainCameraShakeManager,
            profile = x
        })];
    }

    #endregion

    #region Observable Collection Handlers

    private void CameraShakeProfilesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (GameObject)
            cameraShaker!.cameraShakeTargets = CreateShakeTargets(CameraShakeProfiles);
    }

    private void ScreenFlashColorsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (GameObject)
            screenFlasher!.screenFlashColours = [.. ScreenFlashColors];
    }

    #endregion


    /// <summary>
    /// Represents a choice of a <see cref="RandomAudioClipTable"/> of Hornet voice
    /// clips from the vanilla game.
    /// </summary>
    public enum VanillaVoiceTable
    {
        DEFAULT,
        BEAST_RAGE,
    }

    /// <summary>
    /// Represents the visual, auditory, and damage properties of
    /// one part of a <see cref="ChargedAttack"/>.
    /// </summary>
    public class Step : AttackBase
    {
        /// <inheritdoc cref="Step"/>
        public Step() { }

        internal bool disableIfLastStep = true;
        internal bool startNextStepWhenDone = true;

        #region API

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

        /// <summary>
        /// The index in <see cref="ChargedAttack.CameraShakeProfiles"/> this step uses
        /// to shake the camera when it plays.
        /// If <see langword="null"/>, no camera shake occurs.
        /// Default is <see langword="null"/>.
        /// </summary>
        public int? CameraShakeIndex { get; set; } = null;

        /// <summary>
        /// The index in <see cref="ChargedAttack.ScreenFlashColors"/> this step uses
        /// to create a screen flash when it plays.
        /// If <see langword="null"/>, no screen flash occurs.
        /// Default is <see langword="null"/>.
        /// </summary>
        public int? ScreenFlashIndex { get; set; } = null;

        #endregion

        private NailSlashWithEndEvent? nailSlash;
        protected override NailAttackBase? NailAttack => nailSlash;

        protected override void AddComponents(HeroController hc)
        {
            nailSlash = GameObject!.AddComponent<NailSlashWithEndEvent>();
        }

        protected override void LateInitializeComponents(HeroController hc)
        {
            nailSlash!.animName = AnimName;
            nailSlash!.AttackStarting += DoShakeAndFlash;
            nailSlash!.AttackEnding += DisableIfLast;
            nailSlash!.AttackEnding += ActivateNextStep;

            if (string.IsNullOrWhiteSpace(Name))
                Name = $"Charge Step {GameObject!.transform.GetSiblingIndex() + 1}";
        }

        private void DoShakeAndFlash() {
            if (GameObject)
            {
                var parent = GameObject.transform.parent;

                if (CameraShakeIndex != null)
                    parent.SendMessage(
                        nameof(CameraShakeAnimator.DoCameraShake),
                        (int)CameraShakeIndex,
                        SendMessageOptions.DontRequireReceiver
                    );

                if (ScreenFlashIndex != null)
                    parent.SendMessage(
                        nameof(ScreenFlashAnimator.DoScreenFlash),
                        (int)ScreenFlashIndex,
                        SendMessageOptions.DontRequireReceiver
                    );
            }
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
