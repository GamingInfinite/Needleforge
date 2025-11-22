using GlobalEnums;
using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Represents a custom attack in a crest moveset.
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
    /// <para id="animreqs">
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
    /// <inheritdoc cref="AnimLibrary" path="//para[@id='animreqs']"/>
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
    /// Points which define the shape of this attack's hitbox.
    /// (0, 0) is at the center of Hornet's idle sprite.
    /// Negative X values are in front of Hornet.
    /// </summary>
    public Vector2[] ColliderPoints {
        get => _colliderPoints;
        set {
            _colliderPoints = value;
            if (GameObject)
                collider!.points = ColliderPoints;
        }
    }
    private Vector2[] _colliderPoints = [];

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

    internal GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        if (GameObject)
            Object.DestroyImmediate(GameObject);

        GameObject = new(Name);
        Object.DontDestroyOnLoad(GameObject);
        GameObject.transform.SetParent(parent.transform);
        GameObject.tag = "Nail Attack";
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
        collider.points = ColliderPoints;

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

        // TODO clash tink
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

}
