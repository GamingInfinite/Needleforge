using GlobalEnums;
using UnityEngine;
using EffectsTypes = EnemyHitEffectsProfile.EffectsTypes;
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
    /// these frames determine when the attack's hitbox is enabled and disabled.
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
    /// Color to tint the attack's effect animation when it's not imbued with an element.
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
    public Vector2[] Hitbox {
        get => _hitbox;
        set {
            _hitbox = value;
            if (GameObject)
                collider!.points = value;

            if (_autoTinkerHitbox)
            {
                _tinkerHitbox = ScalePolygon(value, 0.8f);
                if (GameObject)
                    tinkCollider!.points = _tinkerHitbox;
            }
        }
    }
    private Vector2[] _hitbox = [];

    /// <summary>
    /// <para>
    /// Points which define the shape of the "tinker" hitbox which causes a visual and
    /// sound effect, and sometimes recoil, when the attack hits the level geometry.
    /// <inheritdoc cref="Hitbox" path="//*[@id='collider-info']"/>
    /// </para><para>
    /// If left unset or set to <c>null</c>,
    /// will default to the <see cref="Hitbox"/> at 80% size.
    /// </para>
    /// </summary>
    public Vector2[]? TinkerHitbox {
        get => _tinkerHitbox;
        set {
            _autoTinkerHitbox = value == null;
            if (_autoTinkerHitbox)
            {
                _tinkerHitbox = ScalePolygon(_hitbox, 0.8f);
                if (GameObject)
                    tinkCollider!.points = _tinkerHitbox;
            }
            else {
                _tinkerHitbox = value;
                if (GameObject)
                    tinkCollider!.points = value ?? [];
            }
        }
    }
    private Vector2[]? _tinkerHitbox = [];
    private bool _autoTinkerHitbox = true;

    /// <summary>
    /// Multiplier on the overall size of the attack.
    /// </summary>
    public Vector2 Scale {
        get => _scale;
        set {
            _scale = value;
            if (GameObject)
                nailSlash!.scale = value.MultiplyElements(_wallSlashFlipper);
        }
    }
    private Vector3 _scale = Vector2.one;

    /// <summary>
    /// The style of silk generation this attack uses.
    /// <c>FirstHit</c> and <c>Full</c> are the same unless the attack is a multihitter.
    /// </summary>
    public HitSilkGeneration SilkGeneration {
        get => _silkGen;
        set {
            _silkGen = value;
            if (GameObject)
                damager!.silkGeneration = value;
        }
    }
    private HitSilkGeneration _silkGen = HitSilkGeneration.FirstHit;

    /// <summary>
    /// Multiplier on base nail damage for this attack.
    /// </summary>
    public float DamageMult {
        get => _damageMult;
        set {
            _damageMult = value;
            if (GameObject)
                damager!.nailDamageMultiplier = value;
        }
    }
    private float _damageMult = 1f;

    /// <summary>
    /// The amount of stun damage this attack deals when it hits a stunnable boss.
    /// If this attack is a multihitter, this value is applied to each individual hit.
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
        get => _knockback;
        set {
            _knockback = value;
            if (GameObject)
                damager!.magnitudeMult = value;
        }
    }
    private float _knockback = 1f;

    /// <summary>
    /// Setting this with a non-empty array marks this attack as a multi-hitter attack
    /// which damages enemies as many times as the array's length. Each value in the
    /// array is a damage multiplier on Hornet's base needle damage which is applied to
    /// that individual hit; for balance reasons these are usually all &lt; 1.
    /// </summary>
    public float[] MultiHitMultipliers {
        get => _multiHitMults;
        set {
            _multiHitMults = value;
            if (GameObject) {
                bool isMultiHitter = value.Length > 0;

                damager!.multiHitter = isMultiHitter;
                damager!.deathEndDamage = isMultiHitter;
                damager!.hitsUntilDeath = value.Length;
                damager!.damageMultPerHit = value;
            }
        }
    }
    private float[] _multiHitMults = [];

    /// <summary>
    /// Determines the visual effect applied to each hit of a multi-hitting attack after
    /// the first one.
    /// </summary>
    public EffectsTypes MultiHitEffects {
        get => _multiHitEffects;
        set {
            _multiHitEffects = value;
            if (GameObject)
                damager!.multiHitEffects = value;
        }
    }
    private EffectsTypes _multiHitEffects = EffectsTypes.LagHit;

    /// <summary>
    /// Number of frames between individual hits of a multi-hitting attack. Make sure
    /// the effect animation (see <see cref="AnimName"/> and <see cref="AnimLibrary"/>)
    /// for this attack lasts long enough for all hits to occur.
    /// </summary>
    public int FramesBetweenMultiHits {
        get => _multiSteps;
        set {
            _multiSteps = value;
            if (GameObject)
                damager!.stepsPerHit = value;
        }
    }
    private int _multiSteps = 2;

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

    /// <summary>
    /// <para>
    /// Whether or not this attack is a wall slash. Setting this to <c>true</c> causes
    /// the attack's scale to flip on the X axis, so that the attack actually points in
    /// front of Hornet when she's wall-sliding.
    /// </para><para>
    /// When setting this attack on a <see cref="MovesetData.WallSlash"/> property,
    /// this will be set automatically.
    /// </para>
    /// </summary>
    internal bool IsWallSlash {
        get => _wallSlashFlipper.x < 0;
        set =>
            _wallSlashFlipper = Vector3.one with {
                x = value ? -1 : 1
            };
    }
    private Vector3 _wallSlashFlipper = Vector3.one;

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
        collider.points = Hitbox;

        nailSlash.hc = hc;
        nailSlash.activateOnSlash = [];
        nailSlash.enemyDamager = damager;
        nailSlash.scale = _scale.MultiplyElements(_wallSlashFlipper);

        DamagerInit();
        damager.magnitudeMult = KnockbackMult;
        damager.nailDamageMultiplier = DamageMult;
        damager.stunDamage = StunDamage;
        damager.silkGeneration = SilkGeneration;

        bool isMultiHitter = MultiHitMultipliers.Length > 0;
        damager.multiHitter = isMultiHitter;
        damager.deathEndDamage = isMultiHitter;
        damager.hitsUntilDeath = MultiHitMultipliers.Length;
        damager.damageMultPerHit = MultiHitMultipliers;
        damager.stepsPerHit = FramesBetweenMultiHits;

        // Set up - visuals & sound

        animator.library = AnimLibrary;
        nailSlash.animName = AnimName;

        nailSlash.AttackStarting += TintIfNotImbued;

        audioSrc.outputAudioMixerGroup = hc.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
        audioSrc.playOnAwake = false;
        audioSrc.clip = Sound;

        audioPriority.sourceType = AudioSourcePriority.SourceType.Hero;

        AttachTinker();

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

        tinkCollider.points = TinkerHitbox ?? [];

        tinkRb.bodyType = RigidbodyType2D.Kinematic;
        tinkRb.simulated = true;
        tinkRb.useFullKinematicContacts = true;

        clashTink.SetActive(true);
    }

    private void TintIfNotImbued() {
        if (damager!.NailElement == NailElements.None)
            sprite!.color = Color;
    }

}
