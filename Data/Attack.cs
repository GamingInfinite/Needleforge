using GlobalEnums;
using UnityEngine;

namespace Needleforge.Data;

/// <summary>
/// Represents a custom attack in a crest moveset.
/// </summary>
public class Attack {

    #region API

    /// <summary>
    /// A name for the <see cref="GameObject"/> which will be created for this Attack.
    /// </summary>
    public string Name = "";

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
    public tk2dSpriteAnimation? AnimLibrary;

    /// <summary>
    /// <para>
    /// The name of the animation clip to use for this attack's effect.
    /// </para>
    /// <inheritdoc cref="AnimLibrary" path="//para[@id='animreqs']"/>
    /// </summary>
    public string AnimName = "";

    /// <summary>
    /// Color to tint the attack's effect animation.
    /// </summary>
    public Color Color = Color.white;

    /// <summary>
    /// Sound effect to play when this attack is used.
    /// </summary>
    public AudioClip? Sound;

    public bool IsDownSlash = false;
    public bool IsWallSlash = false;

    /// <summary>
    /// Points which define the shape of this attack's hitbox.
    /// (0, 0) is at the center of Hornet's idle sprite.
    /// Negative X values are in front of Hornet.
    /// </summary>
    public Vector2[] ColliderPoints = [];

    /// <summary>
    /// The style of silk generation this attack uses.
    /// By default, silk will generate only on first hit.
    /// </summary>
    public HitSilkGeneration SilkGeneration = HitSilkGeneration.FirstHit;
    public float StunDamage = 1f;
    public float Magnitude = 1f;

    #endregion

    internal GameObject CreateGameObject(GameObject parent, HeroController hc)
    {
        GameObject attack = new(Name);
        Object.DontDestroyOnLoad(attack);
        attack.transform.SetParent(parent.transform);
        attack.tag = "Nail Attack";
        attack.layer = (int)PhysLayers.HERO_ATTACK;
        attack.transform.localPosition = new(0, 0, 0);

        attack.SetActive(false); // VERY IMPORTANT

        // Create components

        var sprite = attack.AddComponent<tk2dSprite>();
        var animator = attack.AddComponent<tk2dSpriteAnimator>();
        var nailSlash = attack.AddComponent<NailSlash>();
        var audioSrc = attack.AddComponent<AudioSource>();
        var collider = attack.AddComponent<PolygonCollider2D>();
        var damager = attack.AddComponent<DamageEnemies>();
        var audioPriority = attack.AddComponent<AudioSourcePriority>();

        // Set up - damage

        collider.isTrigger = true;
        collider.points = ColliderPoints;

        nailSlash.hc = hc;
        nailSlash.activateOnSlash = [];
        nailSlash.enemyDamager = damager;
        nailSlash.scale = Vector3.one;
        if (IsWallSlash)
            nailSlash.scale = Vector3.one with { x = -1 };

        damager.useNailDamage = true;
        damager.isHeroDamage = true;
        damager.sourceIsHero = true;
        damager.isNailAttack = true;
        damager.attackType = AttackTypes.Nail;
        damager.silkGeneration = SilkGeneration;
        damager.nailDamageMultiplier = 1f;
        damager.magnitudeMult = Magnitude;
        damager.damageMultPerHit = [];
        damager.stunDamage = StunDamage;
        damager.corpseDirection = new TeamCherry.SharedUtils.OverrideFloat();
        damager.corpseMagnitudeMult = new TeamCherry.SharedUtils.OverrideFloat();
        damager.currencyMagnitudeMult = new TeamCherry.SharedUtils.OverrideFloat();
        damager.slashEffectOverrides = [];
        damager.DealtDamage = new UnityEngine.Events.UnityEvent();
        damager.damageFSMEvent = "";
        damager.dealtDamageFSMEvent = "";
        damager.stunDamage = 1f;
        damager.Tinked = new UnityEngine.Events.UnityEvent();

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

        attack.SetActive(true);
        return attack;
    }

}
