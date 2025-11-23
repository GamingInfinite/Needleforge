using GlobalEnums;
using UnityEngine;
using static Needleforge.Utils.MathUtils;

namespace Needleforge.Data;

/// <summary>
/// Represents the visual, auditory, and damage properties of an attack in a crest moveset.
/// </summary>
public class Attack {

    #region API

    /// <summary>
    /// A name for this attack's <see cref="GameObject"/>.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            if (GameObject)
                GameObject.name = value;
        }
    }
    private string _name = "";

    /// <summary>
    /// <para>
    /// A reference to the library where this attack's effect animation is found.
    /// </para>
    /// <para id="anim-info">
    /// The effect animation for an attack should not loop, and MUST have two frames for
    /// which <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> = <c>true</c>;
    /// these frames determine when the attack's collider is enabled and disabled.
    /// </para>
    /// </summary>
    public tk2dSpriteAnimation? AnimLibrary
    {
        get => _animLibrary;
        set {
            _animLibrary = value;
            if (GameObject)
                animator!.Library = value;
        }
    }
    private tk2dSpriteAnimation? _animLibrary;

    /// <summary>
    /// <para>
    /// The name of the animation clip to use for this attack's effect.
    /// </para>
    /// <inheritdoc cref="AnimLibrary" path="//*[@id='anim-info']"/>
    /// </summary>
    public string AnimName {
        get => _animName;
        set {
            _animName = value;
            if (GameObject)
                nailSlash!.animName = value;
        }
    }
    private string _animName = "";

    /// <summary>
    /// Color to tint the attack's effect animation.
    /// </summary>
    public Color Color { get; set; } = Color.white;

    /// <summary>
    /// Sound effect to play when this attack is used.
    /// </summary>
    public AudioClip? Sound {
        get => _sound;
        set {
            _sound = value;
            if (GameObject)
                audioSrc!.clip = Sound;
        }
    }
    private AudioClip? _sound;

    /// <summary>
    /// Points which define the shape of this attack's damaging hitbox.
    /// <span id="collider-info">
    /// (0, 0) is at the center of Hornet's idle sprite.
    /// Negative X values are in front of Hornet.
    /// </span>
    /// </summary>
    public Vector2[] HitboxPoints {
        get => _hitboxPoints;
        set {
            _hitboxPoints = value;
            if (GameObject)
                collider!.points = value;

            if (_autoTinkerPoints)
            {
                _tinkerPoints = ScalePolygon(value, 0.8f);
                if (GameObject)
                    tinkCollider!.points = _tinkerPoints;
            }
        }
    }
    private Vector2[] _hitboxPoints = [];

    /// <summary>
    /// <para>
    /// Points which define the shape of the "tinker" hitbox which causes a visual and
    /// sound effect, and sometimes recoil, when the attack hits the level geometry.
    /// <inheritdoc cref="HitboxPoints" path="//*[@id='collider-info']"/>
    /// </para><para>
    /// If left unset or set to <c>null</c>, will automatically use a scaled-down copy
    /// of <see cref="HitboxPoints"/>.
    /// </para>
    /// </summary>
    public Vector2[]? TinkerPoints {
        get => _tinkerPoints;
        set {
            _autoTinkerPoints = value == null;
            if (_autoTinkerPoints)
            {
                _tinkerPoints = ScalePolygon(_hitboxPoints, 0.8f);
                if (GameObject)
                    tinkCollider!.points = _tinkerPoints;
            }
            else {
                _tinkerPoints = value;
                if (GameObject)
                    tinkCollider!.points = value ?? [];
            }
        }
    }
    private Vector2[]? _tinkerPoints = [];
    private bool _autoTinkerPoints = true;

    /// <summary>
    /// The style of silk generation this attack uses.
    /// </summary>
    public HitSilkGeneration SilkGeneration {
        get => _silkGen;
        set {
            _silkGen = value;
            if (GameObject)
                damager!.silkGeneration = value;
        }
    }
    private HitSilkGeneration _silkGen = HitSilkGeneration.Full;

    /// <summary>
    /// The amount of stun damage this attack deals when it hits a stunnable boss.
    /// </summary>
    public float StunDamage {
        get => _stunDamage;
        set {
            _stunDamage = value;
            if (GameObject)
                damager!.stunDamage = value;
        }
    }
    private float _stunDamage = 1f;

    /// <summary>
    /// A multiplier on how far away from Hornet an enemy is pushed when this attack
    /// hits them. Must be non-negative.
    /// </summary>
    public float KnockbackMult {
        get => _magnitude;
        set {
            _magnitude = value;
            if (GameObject)
                damager!.magnitudeMult = value;
        }
    }
    private float _magnitude = 1f;

    #endregion

    /// <summary>
    /// <para>
    /// After the moveset this attack is attached to has initialized
    /// (see <see cref="MovesetData.OnInitialized"/>), this will reference the GameObject
    /// that this Attack represents.
    /// </para><para>
    /// Modifying this object directly should only be done with caution and if no other
    /// properties of this class can make the modification you need.
    /// </para>
    /// </summary>
    public GameObject? GameObject { get; private set; }

    internal bool IsWallSlash = false;

    private tk2dSprite? sprite;
    private tk2dSpriteAnimator? animator;
    private NailSlash? nailSlash;
    private AudioSource? audioSrc;
    private PolygonCollider2D? collider;
    private DamageEnemies? damager;
    private AudioSourcePriority? audioPriority;

    private PolygonCollider2D? tinkCollider;

    private const string NAIL_ATTACK_TAG = "Nail Attack";

    internal GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        if (GameObject)
            Object.DestroyImmediate(GameObject);

        GameObject = new(Name);
        Object.DontDestroyOnLoad(GameObject);
        GameObject.transform.SetParent(parent.transform);
        GameObject.tag = NAIL_ATTACK_TAG;
        GameObject.layer = (int)PhysLayers.HERO_ATTACK;
        GameObject.transform.localPosition = new(0, 0, 0);

        GameObject.SetActive(false); // VERY IMPORTANT

        sprite = GameObject.AddComponent<tk2dSprite>();
        animator = GameObject.AddComponent<tk2dSpriteAnimator>();
        nailSlash = GameObject.AddComponent<NailSlash>();
        audioSrc = GameObject.AddComponent<AudioSource>();
        collider = GameObject.AddComponent<PolygonCollider2D>();
        damager = GameObject.AddComponent<DamageEnemies>();
        audioPriority = GameObject.AddComponent<AudioSourcePriority>();

        // Set up - damage

        collider.isTrigger = true;
        collider.points = HitboxPoints;

        nailSlash.hc = hc;
        nailSlash.activateOnSlash = [];
        nailSlash.enemyDamager = damager;
        nailSlash.scale = Vector3.one;
        if (IsWallSlash)
            nailSlash.scale = Vector3.one with { x = -1 };

        DamagerInit();
        damager.magnitudeMult = KnockbackMult;
        damager.stunDamage = StunDamage;
        damager.silkGeneration = SilkGeneration;

        //if (IsDownSlash) {
        //    var downAttack = attack.AddComponent<HeroDownAttack>();
        //    downAttack.hc = hc;
        //    downAttack.attack = nailSlash;
        //}

        // Set up - visuals & sound

        animator.library = AnimLibrary;
        nailSlash.animName = AnimName;

        nailSlash.AttackStarting += () => sprite.color = Color;

        audioSrc.outputAudioMixerGroup = hc.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
        audioSrc.playOnAwake = false;
        audioSrc.clip = Sound;

        audioPriority.sourceType = AudioSourcePriority.SourceType.Hero;

        AttachTinker();

        // TODO imbuement

        GameObject.SetActive(true);
        return GameObject;
    }

    private void DamagerInit()
    {
        // making absolutely certain this is considered needle damage from hornet
        damager!.useNailDamage = true;
        damager!.isHeroDamage = true;
        damager!.sourceIsHero = true;
        damager!.isNailAttack = true;
        damager!.attackType = AttackTypes.Nail;
        damager!.nailDamageMultiplier = 1f;

        // miscellaneous (some of which may need investigation for API purposes)
        damager!.lagHitOptions = new LagHitOptions() { DamageType = LagHitDamageType.None, HitCount = 0 };
        damager!.damageMultPerHit = [];
        damager!.corpseDirection = new TeamCherry.SharedUtils.OverrideFloat();
        damager!.corpseMagnitudeMult = new TeamCherry.SharedUtils.OverrideFloat();
        damager!.currencyMagnitudeMult = new TeamCherry.SharedUtils.OverrideFloat();
        damager!.slashEffectOverrides = [];
        damager!.DealtDamage = new UnityEngine.Events.UnityEvent();
        damager!.contactFSMEvent = "";
        damager!.damageFSMEvent = "";
        damager!.dealtDamageFSMEvent = "";
        damager!.deathEvent = "";
        damager!.targetRecordedFSMEvent = "";
        damager!.Tinked = new UnityEngine.Events.UnityEvent();
        damager!.ignoreInvuln = false;
    }

    private void AttachTinker() {
        GameObject clashTink = new("Clash Tink");
        Object.DontDestroyOnLoad(clashTink);
        clashTink.transform.SetParent(GameObject!.transform);
        clashTink.tag = NAIL_ATTACK_TAG;
        clashTink.layer = (int)PhysLayers.TINKER;
        clashTink.transform.localPosition = new(0, 0, 0);

        clashTink.SetActive(false); // VERY IMPORTANT

        clashTink.AddComponent<NailSlashTerrainThunk>();
        tinkCollider = clashTink.AddComponent<PolygonCollider2D>();
        var tinkRb = clashTink.AddComponent<Rigidbody2D>();

        tinkCollider.points = TinkerPoints ?? [];

        tinkRb.bodyType = RigidbodyType2D.Kinematic;
        tinkRb.simulated = true;
        tinkRb.useFullKinematicContacts = true;

        clashTink.SetActive(true);
    }

}
