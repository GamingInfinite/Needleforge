using GlobalEnums;
using TeamCherry.SharedUtils;
using UnityEngine;
using UnityEngine.Events;
using System.Diagnostics;
using EffectsTypes = EnemyHitEffectsProfile.EffectsTypes;
using static Needleforge.Utils.MathUtils;

namespace Needleforge.Attacks;

/// <summary>
/// Represents the visual, auditory, and damage properties of an attack in a crest moveset.
/// Changes to an attack's properties will update the <see cref="GameObject"/>
/// it represents, if one has been created.
/// </summary>
public abstract class AttackBase : GameObjectProxy
{
    #region API

    /// <summary>
    /// The name of the animation clip to use for this attack's effect.
    /// </summary>
    public abstract string AnimName { get; set; }

    /// <summary>
    /// A reference to the library where this attack's effect animation is found.
    /// </summary>
    public virtual tk2dSpriteAnimation? AnimLibrary
    {
        get => _animLibrary;
        set
        {
            _animLibrary = value;
            if (GameObject)
                Animator!.Library = value;
        }
    }
    private tk2dSpriteAnimation? _animLibrary;

    /// <summary>
    /// Color to tint the attack's effect animation when it's not imbued with an element.
    /// </summary>
    public Color Color { get; set; } = Color.white;

    /// <summary>
    /// Sound effect to play when this attack is used.
    /// </summary>
    public AudioClip? Sound
    {
        get => _sound;
        set
        {
            _sound = value;
            if (GameObject)
                AudioSrc!.clip = Sound;
        }
    }
    private AudioClip? _sound;

    /// <summary>
    /// Points which define the shape of this attack's damaging hitbox.
    /// This is drawing a polygon, so it should have at least 3 distinct points.
    /// </summary>
    /// <remarks>
    /// (0, 0) is at the center of Hornet's idle sprite.
    /// Negative X values are in front of Hornet.
    /// </remarks>
    public Vector2[] Hitbox
    {
        get => _hitbox;
        set {
            _hitbox = value;
            if (GameObject)
                Collider!.points = value;

            if (_autoTinkerHitbox)
            {
                _tinkerHitbox = ScalePolygon(value, 0.8f);
                if (GameObject)
                    TinkCollider!.points = _tinkerHitbox;
            }

            CheckPolygonValidity(nameof(Hitbox), _hitbox);
        }
    }
    private Vector2[] _hitbox = [];

    /// <summary>
    /// Points which define the shape of the "tinker" hitbox which causes a visual and
    /// sound effect, and sometimes recoil, when the attack hits the level geometry.
    /// This is drawing a polygon, so it should have at least 3 distinct points.
    /// </summary>
    /// <remarks>
    /// If left unset or set to <see langword="null"/>, will default to the
    /// <see cref="Hitbox"/> at 80% size.
    /// <inheritdoc cref="Hitbox" path="//remarks"/>
    /// </remarks>
    public Vector2[]? TinkerHitbox
    {
        get => _tinkerHitbox;
        set
        {
            _autoTinkerHitbox = value == null;
            if (_autoTinkerHitbox)
                _tinkerHitbox = ScalePolygon(_hitbox, 0.8f);
            else
                _tinkerHitbox = value;

            if (GameObject)
                TinkCollider!.points = _tinkerHitbox ?? [];

            if (_tinkerHitbox != null)
                CheckPolygonValidity(nameof(TinkerHitbox), _tinkerHitbox);
        }
    }
    private Vector2[]? _tinkerHitbox = [];
    private bool _autoTinkerHitbox = true;

    /// <summary>
    /// Multiplier on the overall size of the attack.
    /// </summary>
    public virtual Vector2 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            if (GameObject)
                NailAttack!.scale = value;
        }
    }
    private Vector3 _scale = Vector2.one;

    /// <summary>
    /// The style of silk generation this attack uses.
    /// <c>FirstHit</c> and <c>Full</c> are the same unless the attack is a multihitter.
    /// </summary>
    public HitSilkGeneration SilkGeneration
    {
        get => _silkGen;
        set
        {
            _silkGen = value;
            if (GameObject)
                Damager!.silkGeneration = value;
        }
    }
    private HitSilkGeneration _silkGen = HitSilkGeneration.FirstHit;

    /// <summary>
    /// Multiplier on base nail damage for this attack.
    /// </summary>
    public float DamageMult
    {
        get => _damageMult;
        set
        {
            _damageMult = value;
            if (GameObject)
                Damager!.nailDamageMultiplier = value;
        }
    }
    private float _damageMult = 1f;

    /// <summary>
    /// The amount of stun damage this attack deals when it hits a stunnable boss.
    /// If this attack is a multihitter, this value is applied to each individual hit.
    /// </summary>
    public float StunDamage
    {
        get => _stunDamage;
        set
        {
            _stunDamage = value;
            if (GameObject)
                Damager!.stunDamage = value;
        }
    }
    private float _stunDamage = 1f;

    /// <summary>
    /// A multiplier on how far away from Hornet an enemy is pushed when this attack
    /// hits them. Must be non-negative.
    /// </summary>
    public float KnockbackMult
    {
        get => _knockback;
        set
        {
            _knockback = value;
            if (GameObject)
                Damager!.magnitudeMult = value;
        }
    }
    private float _knockback = 1f;

    /// <summary>
    /// Setting this with a non-empty array marks this attack as a multi-hitter attack.
    /// The length of the array decides how many times the attack will hit.
    /// Each value in the array is a damage multiplier which is applied to that
    /// individual hit; these are usually all &lt; 1.
    /// </summary>
    public float[] MultiHitMultipliers
    {
        get => _multiHitMults;
        set
        {
            _multiHitMults = value;
            if (GameObject)
            {
                bool isMultiHitter = value.Length > 0;

                Damager!.multiHitter = isMultiHitter;
                Damager!.deathEndDamage = isMultiHitter;
                Damager!.hitsUntilDeath = value.Length;
                Damager!.damageMultPerHit = value;
            }
        }
    }
    private float[] _multiHitMults = [];

    /// <summary>
    /// Determines the visual effect applied to each hit of a multi-hitting attack after
    /// the first one.
    /// </summary>
    public EffectsTypes MultiHitEffects
    {
        get => _multiHitEffects;
        set
        {
            _multiHitEffects = value;
            if (GameObject)
                Damager!.multiHitEffects = value;
        }
    }
    private EffectsTypes _multiHitEffects = EffectsTypes.LagHit;

    /// <summary>
    /// Number of frames between individual hits of a multi-hitting attack. Make sure
    /// the effect animation (see <see cref="AnimName"/> and <see cref="AnimLibrary"/>)
    /// for this attack lasts long enough for all hits to occur.
    /// </summary>
    public int FramesBetweenMultiHits
    {
        get => _multiSteps;
        set
        {
            _multiSteps = value;
            if (GameObject)
                Damager!.stepsPerHit = value;
        }
    }
    private int _multiSteps = 2;

    #endregion

    #region Required Initialization

    /// <summary>
    /// A reference to the component responsible for animating the attack and de/activating
    /// its hitbox.
    /// </summary>
    /// <remarks>
    /// The referenced component must be added to the <see cref="GameObject"/> in
    /// <see cref="AddComponents"/>. This is needed for some standard initialization.
    /// </remarks>
    protected abstract NailAttackBase? NailAttack { get; }

    /// <summary>
    /// <para>
    /// This is called immediately after all standard attack components are added to
    /// this attack's <see cref="GameObject"/>, and should be used to add and initialize
    /// any needed additional components to the GameObject.
    /// At least one of these should be a MonoBehaviour descended from
    /// <see cref="NailAttackBase"/>, the same one returned by <see cref="NailAttack"/>.
    /// </para><para>
    /// If the added components need to reference some value from the standard components,
    /// or the standard components need to be modified in some way,
    /// that should be done in an override of <see cref="LateInitializeComponents"/>.
    /// </para>
    /// </summary>
    /// <param name="hc">A reference to the current HeroController.</param>
    protected abstract void AddComponents(HeroController hc);

    /// <summary>
    /// This is called after all standard component initialization has occurred. If the
    /// components added in <see cref="AddComponents"/> need to reference some value from
    /// the standard components, or the standard components need to modified in some way,
    /// that should be done here.
    /// </summary>
    /// <param name="hc">A reference to the current HeroController.</param>
    protected virtual void LateInitializeComponents(HeroController hc) { }

    #endregion

    #pragma warning disable CS1591 // Missing XML comment
    protected tk2dSprite? Sprite { get; private set; }
    protected tk2dSpriteAnimator? Animator { get; private set; }
    protected AudioSource? AudioSrc { get; private set; }
    protected PolygonCollider2D? Collider { get; private set; }
    protected PolygonCollider2D? TinkCollider { get; private set; }
    protected DamageEnemies? Damager { get; private set; }
    protected AudioSourcePriority? AudioPriority { get; private set; }
    #pragma warning restore CS1591 // Missing XML comment

    /// <inheritdoc/>
    public override GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        GameObject = base.CreateGameObject(parent, hc);

        GameObject.tag = NAIL_ATTACK_TAG;
        GameObject.layer = (int)PhysLayers.HERO_ATTACK;
        GameObject.SetActive(false); // VERY IMPORTANT

        // Common component initialization

        Sprite = GameObject.AddComponent<tk2dSprite>();
        Animator = GameObject.AddComponent<tk2dSpriteAnimator>();
        AudioSrc = GameObject.AddComponent<AudioSource>();
        Collider = GameObject.AddComponent<PolygonCollider2D>();
        Damager = GameObject.AddComponent<DamageEnemies>();
        AudioPriority = GameObject.AddComponent<AudioSourcePriority>();

        AddComponents(hc);

        Collider.isTrigger = true;

        DamagerInit();

        AudioSrc.outputAudioMixerGroup = hc.gameObject.GetComponent<AudioSource>().outputAudioMixerGroup;
        AudioSrc.playOnAwake = false;

        AudioPriority.sourceType = AudioSourcePriority.SourceType.Hero;

        NailAttack!.hc = hc;
        NailAttack!.enemyDamager = Damager;
        NailAttack!.activateOnSlash = [];

        // Customizations

        Collider.points = Hitbox;

        Damager.magnitudeMult = KnockbackMult;
        Damager.nailDamageMultiplier = DamageMult;
        Damager.stunDamage = StunDamage;
        Damager.silkGeneration = SilkGeneration;

        bool isMultiHitter = MultiHitMultipliers.Length > 0;
        Damager.multiHitter = isMultiHitter;
        Damager.deathEndDamage = isMultiHitter;
        Damager.hitsUntilDeath = MultiHitMultipliers.Length;
        Damager.damageMultPerHit = MultiHitMultipliers;
        Damager.stepsPerHit = FramesBetweenMultiHits;
        Damager.multiHitEffects = MultiHitEffects;

        Animator.library = AnimLibrary;
        AudioSrc.clip = Sound;

        NailAttack!.scale = Scale;
        NailAttack!.AttackStarting += TintIfNotImbued;

        AttachTinker();

        LateInitializeComponents(hc);

        GameObject.SetActive(true);
        return GameObject;
    }

    /// <summary>
    /// The value of the <see cref="GameObject.tag"/> applied to all attack objects.
    /// </summary>
    protected const string NAIL_ATTACK_TAG = "Nail Attack";

    private void DamagerInit()
    {
        // making absolutely certain this is considered needle damage from hornet
        Damager!.useNailDamage = true;
        Damager!.isHeroDamage = true;
        Damager!.sourceIsHero = true;
        Damager!.isNailAttack = true;
        Damager!.attackType = AttackTypes.Nail;
        Damager!.nailDamageMultiplier = 1f;

        // miscellaneous (some of which may need investigation for API purposes)
        Damager!.lagHitOptions = new LagHitOptions() { DamageType = LagHitDamageType.None, HitCount = 0 };
        Damager!.corpseDirection = new OverrideFloat();
        Damager!.corpseMagnitudeMult = new OverrideFloat();
        Damager!.currencyMagnitudeMult = new OverrideFloat();
        Damager!.slashEffectOverrides = [];
        Damager!.DealtDamage = new UnityEvent();
        Damager!.contactFSMEvent = "";
        Damager!.damageFSMEvent = "";
        Damager!.dealtDamageFSMEvent = "";
        Damager!.deathEvent = "";
        Damager!.targetRecordedFSMEvent = "";
        Damager!.Tinked = new UnityEvent();
        Damager!.ignoreInvuln = false;
    }

    private void AttachTinker()
    {
        GameObject clashTink = new("Clash Tink");
        Object.DontDestroyOnLoad(clashTink);
        clashTink.transform.SetParent(GameObject!.transform);
        clashTink.tag = NAIL_ATTACK_TAG;
        clashTink.layer = (int)PhysLayers.TINKER;
        clashTink.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        clashTink.SetActive(false); // VERY IMPORTANT

        TinkCollider = clashTink.AddComponent<PolygonCollider2D>();
        var tinkThunk = clashTink.AddComponent<NailSlashTerrainThunk>();
        var tinkRb = clashTink.AddComponent<Rigidbody2D>();

        TinkCollider.points = TinkerHitbox ?? [];

        tinkRb.bodyType = RigidbodyType2D.Kinematic;
        tinkRb.simulated = true;
        tinkRb.useFullKinematicContacts = true;

        tinkThunk.doRecoil = true;

        clashTink.SetActive(true);
    }

    private void TintIfNotImbued()
    {
        if (Damager!.NailElement == NailElements.None)
            Sprite!.color = Color;
    }

    /// <summary>
    /// Prints warnings with a stack trace if a set of points defining a polygon
    /// has &lt; 3 points, and/or if 2+ points are too close together.
    /// </summary>
    protected static void CheckPolygonValidity(string name, Vector2[] points)
    {
        string warning = "";

        if (points.Length < 3)
            warning += $"{name} has fewer than 3 points. ";

        bool keepLooping = true;
        for (int a = 0; a < points.Length && keepLooping; a++)
        {
            for (int b = a + 1; b < points.Length && keepLooping; b++)
            {
                Vector2 ptA = points[a], ptB = points[b];
                if (Mathf.Approximately(ptA.x, ptB.x) && Mathf.Approximately(ptA.y, ptB.y))
                {
                    warning += $"{name} has 2 or more points that are too close together. ";
                    keepLooping = false;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(warning))
            ModHelper.LogWarning(warning + '\n' + new StackTrace(fNeedFileInfo: true));
    }
}
