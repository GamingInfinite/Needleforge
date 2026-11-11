using BepInEx;
using GlobalEnums;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Needleforge;
using Needleforge.Attacks;
using Needleforge.Data;
using Silksong.FsmUtil;
using Silksong.UnityHelper.Util;
using System.Collections;
using System.Linq;
using System.Reflection;
using TeamCherry.Localization;
using UnityEngine;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;
using WrapMode = tk2dSpriteAnimationClip.WrapMode;

namespace ExampleCrest;

[BepInAutoPlugin(id: "io.github.examplecrest")]
[BepInDependency("io.github.needleforge", "0.9.0")]
[BepInDependency("org.silksong-modding.i18n", "1.0.2")]
[BepInDependency("org.silksong-modding.fsmutil", "0.3.16")]
[BepInDependency("org.silksong-modding.unityhelper", "1.1.1")]
public partial class ExampleCrestPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
        Assembly asm = Assembly.GetExecutingAssembly();
        string path = "ExampleCrest.Assets";

        #region Making Custom Animations
        /*
        We need several custom animations for our custom crest. We'll use UnityHelper to
        make the animation clips, then put them in a library.
        */

        Texture2D
            slashTex = SpriteUtil.LoadEmbeddedTexture(asm, $"{path}.slash.png"),
            hornetTex = SpriteUtil.LoadEmbeddedTexture(asm, $"{path}.hornet.png"),
            hudTexAp = SpriteUtil.LoadEmbeddedTexture(asm, $"{path}.hud_ingame_ap.png"),
            hudTexId = SpriteUtil.LoadEmbeddedTexture(asm, $"{path}.hud_ingame_id.png"),
            hudTexDs = SpriteUtil.LoadEmbeddedTexture(asm, $"{path}.hud_ingame_ds.png");

        // A tk2dSpriteCollectionData is a MonoBehaviour, so this creates a GameObject.
        // We'll reuse that same GameObject for our animation library.
        tk2dSpriteCollectionData frameData = Tk2dUtil.CreateTk2dSpriteCollection(
            sprites: [
                slashTex,
                hornetTex,
                hudTexAp,
                hudTexId,
                hudTexDs,
            ],
            spriteCenters: [
                new Vector2(208, 76),
                new Vector2(hornetTex.width, hornetTex.height) * 0.5f,
                new Vector2(146.5f, 84.5f),
                new Vector2(146.5f, 84.5f),
                new Vector2(146.5f, 84.5f),
            ]
        );
        Object.DontDestroyOnLoad(frameData.gameObject);
        frameData.gameObject.name = "NeoCrest Anims";

        tk2dSpriteAnimation neoAnimations = frameData.gameObject.AddComponent<tk2dSpriteAnimation>();
        neoAnimations.clips = [
            // Some kinds of attacks need trigger frames...
            new tk2dSpriteAnimationClip {
                name = "Slash Effect With Triggers",
                fps = 10,
                wrapMode = WrapMode.Once,
                frames = [
                    frameData.CreateFrame(slashTex.name, triggerEvent: true),
                    .. frameData.CreateFrames(Enumerable.Repeat(slashTex.name, 3)),
                    frameData.CreateFrame(slashTex.name, triggerEvent: true),
                ]
            },
            // ...and some kinds of attacks need there to be NO trigger frames.
            new tk2dSpriteAnimationClip {
                name = "Slash Effect Without Triggers",
                fps = 10,
                wrapMode = WrapMode.Once,
                frames = frameData.CreateFrames(Enumerable.Repeat(slashTex.name, 5)),
            },

            // This is an override animation for Hornet; it has to have the same name as
            // the default animation it's overriding. The documentation for each attack
            // in a Moveset tells you the names of its related Hornet animations.
            new tk2dSpriteAnimationClip {
                name = "Slash",
                fps = 2,
                wrapMode = WrapMode.Once,
                frames = [frameData.CreateFrame(hornetTex.name)],
            },

            // These are our HUD animations.
            new tk2dSpriteAnimationClip {
                name = "Neo HUD Appear",
                fps = 1,
                wrapMode = WrapMode.Once,
                frames = [frameData.CreateFrame(hudTexAp.name)],
            },
            new tk2dSpriteAnimationClip {
                name = "Neo HUD Idle",
                fps = 1,
                wrapMode = WrapMode.Once,
                frames = [frameData.CreateFrame(hudTexId.name)],
            },
            new tk2dSpriteAnimationClip {
                name = "Neo HUD Disappear",
                fps = 1,
                wrapMode = WrapMode.Once,
                frames = [frameData.CreateFrame(hudTexDs.name)],
            },
        ];
        neoAnimations.ValidateLookup();

        #endregion


        #region Tools

        // Needleforge has the ability to define custom tool colours.
        // It also defines some custom colours itself that you can choose to use.
        ColorData orange = NeedleforgePlugin.AddToolColor(
            name: "Orange",
            color: new Color(0.7f, 0.6f, 0.2f),
            isAttackType: true
        );
        orange.AddValidTypes(ToolItemType.Yellow, ToolItemType.Red);

        // This registers new basic tools with Needleforge. Basic tools don't do anything
        // on their own; patch the game to give them functionality.
        // Registration *must* happen during your plugin's Awake() function.
        ToolData yellowTool = NeedleforgePlugin.AddTool(
            name: "NeoYellowTool",
            type: ToolItemType.Yellow,
            // We're using I18N to load text.
            displayName: new LocalisedString($"Mods.{Id}", "YELLOW_NAME"),
            description: new LocalisedString($"Mods.{Id}", "YELLOW_DESC")
        );
        ToolData greenTool = NeedleforgePlugin.AddTool(
            name: "NeoGreenTool",
            type: NeedleforgePlugin.GreenTools.Type,
            displayName: new LocalisedString($"Mods.{Id}", "GREEN_NAME"),
            description: new LocalisedString($"Mods.{Id}", "GREEN_DESC")
        );

        // This registers a new liquid tool; these tools have a limited supply of refills
        // aside from shards. Note that they start with no uses and no refills.
        // They can be given functionality with their data object.
        // Registration *must* happen during your plugin's Awake() function.
        LiquidToolData blackTool = NeedleforgePlugin.AddLiquidTool(
            name: "NeoBlackTool",
            maxRefills: 20,
            storageAmount: 10,
            liquidColor: Color.magenta
        );
        blackTool.type = NeedleforgePlugin.BlackTools.Type;
        blackTool.displayName = new LocalisedString($"Mods.{Id}", "BLACK_NAME");
        blackTool.description = new LocalisedString($"Mods.{Id}", "BLACK_DESC");
        blackTool.beforeAnim = () => Logger.LogInfo("Starting NeoBlackTool");
        blackTool.afterAnim = () => Logger.LogInfo("Ended NeoBlackTool");

        #endregion


        #region Crests - The Basics

        // This registers a new crest with Needleforge and returns an object used
        // to customize it.
        // Registration *must* happen during your plugin's Awake() function.
        CrestData neoCrest = NeedleforgePlugin.AddCrest(
            name: "NeoCrest",
            // We're using I18N to load text.
            displayName: new LocalisedString($"Mods.{Id}", "CREST_NAME"),
            description: new LocalisedString($"Mods.{Id}", "CREST_DESC"),
            // We're using UnityHelper to load images.
            RealSprite: SpriteUtil.LoadEmbeddedSprite(asm, $"{path}.inventory_line.png", pixelsPerUnit: 100),
            Silhouette: SpriteUtil.LoadEmbeddedSprite(asm, $"{path}.inventory_fill.png", pixelsPerUnit: 200),
            CrestGlow: SpriteUtil.LoadEmbeddedSprite(asm, $"{path}.inventory_glow.png", pixelsPerUnit: 284)
        );

        #endregion

        #region Crests - Adding Tool Slots

        // Crests can have as many tool slots as you want, of any of the base game colours...
        neoCrest.AddBlueSlot(new Vector2(0, 2), false);
        neoCrest.AddYellowSlot(new Vector2(-2, 1), false);
        neoCrest.AddRedSlot(AttackToolBinding.Neutral, new Vector2(2, 1), false);
        neoCrest.AddSkillSlot(AttackToolBinding.Neutral, Vector2.zero, false);

        // ...and of custom colours, too!
        // Needleforge defines a few new colours you can use, or you can make your own.
        neoCrest.AddToolSlot(NeedleforgePlugin.GreenTools.Type, AttackToolBinding.Neutral, new Vector2(-2, -1), false);
        neoCrest.AddToolSlot(NeedleforgePlugin.PinkTools.Type, AttackToolBinding.Up, new Vector2(2, -1), false);
        neoCrest.AddToolSlot(NeedleforgePlugin.BlackTools.Type, AttackToolBinding.Down, new Vector2(0, -2), false);

        neoCrest.ApplyAutoSlotNavigation(angleRange: 80f);

        #endregion

        #region Crests - Customizing the HUD Frame

        // Custom crests can reuse the HUD of any base game crest.
        // If you don't want to bother with custom HUDs, you can skip to the next section now.
        neoCrest.HudFrame.Preset = VanillaCrest.BEAST;


        // Alternatively, custom crests can define completely custom HUD sprites and animations.
        neoCrest.HudFrame.ProfileIcon = SpriteUtil.LoadEmbeddedSprite(asm, $"{path}.hud_profile.png");
        neoCrest.HudFrame.Appear = neoAnimations.GetClipByName("Neo HUD Appear");
        neoCrest.HudFrame.Idle = neoAnimations.GetClipByName("Neo HUD Idle");
        neoCrest.HudFrame.Disappear = neoAnimations.GetClipByName("Neo HUD Disappear");

        // Any HUD, preset or custom, can add extra GameObjects to itself.
        // These can be used to create extra visual elements; e.g. Hunter's combo meter.
        neoCrest.HudFrame.OnRootCreated += OnHudRootCreated;
        void OnHudRootCreated()
        {
            neoCrest.HudFrame.Root!.transform.localScale = Vector2.zero;

            GameObject extra = new GameObject("extra") { layer = (int)PhysLayers.UI };
            extra.transform.SetParent(neoCrest.HudFrame.Root!.transform);
            extra.transform.localPosition = new Vector2(0.2f, -0.3f);
            extra.transform.localScale = Vector2.one * 0.5f;

            SpriteRenderer renderer = extra.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteUtil.LoadEmbeddedSprite(asm, $"{path}.hornet.png");
        }

        // Any HUD, preset or custom, can have a coroutine. These can affect the HUD
        // in any way; play animations, control elements of the HUD root, etc.
        neoCrest.HudFrame.Coroutine = HudCoroutine;
        IEnumerator HudCoroutine(BindOrbHudFrame hudInstance)
        {
            neoCrest.HudFrame.Root!.transform.localScale = Vector2.one;

            int prevHP = 0;
            Transform extra = neoCrest.HudFrame.Root!.transform.Find("extra");
            while(true)
            {
                if (!neoCrest.IsEquipped)
                    break;
                if (HeroController.instance.IsPaused())
                {
                    yield return null;
                    continue;
                }

                if (prevHP > PlayerData.instance.health)
                    extra.FlipLocalScale(y: true);

                prevHP = PlayerData.instance.health;
                yield return null;
            }

            neoCrest.HudFrame.Root!.transform.localScale = Vector2.zero;
        }

        #endregion

        #region Crests - Moveset - Creating Attacks
        /*
        The attack objects in a moveset determine attacks' hitboxes, sounds, effect
        animations, and other details about how they look and what damage they deal.

        These properties are separate from how *Hornet* looks and behaves while using
        the crest. See the below section on Hero Configuration.
        */

        // This is the most basic definition for a custom attack.
        // A name, an effect animation, and a hitbox with at least 3 points.
        neoCrest.Moveset.Slash = new Attack {
            Name = "NeoSlash",
            AnimName = "Slash Effect With Triggers",
            AnimLibrary = neoAnimations,
            Hitbox = [new Vector2(0, 1.5f), new Vector2(0, -1.5f), new Vector2(-3.5f, -0.4f)]
        };

        Vector2[] standardHitbox = neoCrest.Moveset.Slash.Hitbox;

        // This attack has more customization; it's tinted a colour, moves over time,
        // can hit 3 times, pierces shields, and deals way more stun damage to bosses.
        neoCrest.Moveset.AltSlash = new Attack {
            Name = "NeoSlashAlt",
            AnimName = "Slash Effect With Triggers",
            AnimLibrary = neoAnimations,
            Hitbox = standardHitbox,
            Color = Color.magenta,
            SpecialDamageTypes = SpecialTypes.Piercer,
            MultiHitMultipliers = [0.3f, 0.25f, 0.2f],
            StunDamage = 2f,
            KnockbackMult = 0.2f,
            TravelDistance = new Vector2(-3, 0),
            TravelDuration = 0.3f,
        };

        // You can individually scale, position, and rotate attacks relative to Hornet;
        // this can be nice for reusing basic effect animations.
        neoCrest.Moveset.UpSlash = new Attack {
            Name = "NeoSlashUp",
            AnimName = "Slash Effect With Triggers",
            AnimLibrary = neoAnimations,
            Hitbox = standardHitbox,
            Color = Color.yellow,
            Scale = new Vector2(1, 2),
            Position = new Vector2(0, 0.5f),
            Rotation = Quaternion.Euler(0, 0, 270),
        };
        neoCrest.Moveset.DownSlash = new DownAttack {
            Name = "NeoSlashDown",
            AnimName = "Slash Effect Without Triggers",
            AnimLibrary = neoAnimations,
            Hitbox = standardHitbox,
            Color = Color.red,
            Scale = new Vector2(1.5f, 2),
            Position = new Vector2(0, -0.5f),
            Rotation = Quaternion.Euler(0, 0, 45),
        };

        // Dash and Charged attacks are multi-step attacks; by default, they trigger
        // several regular attacks in sequence. Each step has all the same customization
        // options that regular attacks do.
        neoCrest.Moveset.DashSlash = new DashAttack {
            Name = "NeoSlashDash",
            Steps = [
                new DashAttack.Step {
                    AnimName = "Slash Effect Without Triggers",
                    AnimLibrary = neoAnimations,
                    Hitbox = standardHitbox,
                    Color = Color.cyan,
                    Scale = new Vector2(2, -0.5f),
                },
            ],
        };

        neoCrest.Moveset.ChargedSlash = new ChargedAttack {
            Name = "NeoSlashCharged",
            CameraShakeProfiles = [GlobalSettings.Camera.EnemyKillShake],
            ScreenFlashColors = [new Color(1, 1, 1, 0.5f)],
            PlayOnActivation = false,
            PlayStepsInSequence = false,
            Steps = [
                new ChargedAttack.Step {
                    AnimName = "Slash Effect With Triggers",
                    Hitbox = standardHitbox,
                    Color = Color.yellow,
                    Scale = new Vector2(2, 1),
                    KeepWorldPosition = true,
                    CameraShakeIndex = 0,
                    ScreenFlashIndex = 0,
                },
                new ChargedAttack.Step {
                    AnimName = "Slash Effect With Triggers",
                    Hitbox = standardHitbox,
                    Color = Color.magenta,
                    Scale = new Vector2(3, -1.5f),
                    KeepWorldPosition = true,
                    CameraShakeIndex = 0,
                },
            ],
        };
        neoCrest.Moveset.ChargedSlash.SetAnimLibrary(neoAnimations);


        // Notice that we didn't define a wall slash. Because all crests require one,
        // Needleforge will use a copy of Hunter's wall slash for this crest.


        #endregion

        #region Crests - Moveset - Hero Configuration
        /*
        Hero Configuration determines how Hornet will behave when using this crest.
        This includes but isn't limited to:
            - Her animations
            - How quickly she attacks
            - Whether or not she can bind
            - Whether or not she can use each of her movement abilities
            - How her dash, down, and charged attacks function.

        There are several "bulk setter" functions on a config object that explain what
        each option they affect does, but be aware that some options don't have a setter
        function and must be changed directly.
        */

        var cfg = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();
        neoCrest.Moveset.HeroConfig = cfg;

        // If you want to add or replace any Hornet animations for this crest,
        // set this field to a tk2dSpriteAnimation library.
        cfg.heroAnimOverrideLib = neoAnimations;

        cfg.canBind = true;
        cfg.SetCanUseAbilities(true);
        cfg.SetAttackFields(
            time: 0.35f, recovery: 0.15f, cooldown: 0.41f, // Regular attack speeds
            quickSpeedMult: 1.5f, quickCooldown: 0.205f // Flea Brew
        );
        cfg.wallSlashSlowdown = true;
        cfg.SetDashStabFields(time: 0.3f, speed: -30, bounceJumpSpeed: 40);

        // There are three different kinds of down attack which behave differently.
        // This crest's is a Downspike; Hornet moves during it, similar to Hunter crest.
        cfg.downSlashType = DownSlashTypes.DownSpike;
        cfg.SetDownspikeFields(
            anticTime: 0.1f, time: 0.15f, recoveryTime: 0.05f,
            doesThrust: true, velocity: new Vector2(-15, -15), doesBurstEffect: true
        );

        // Instead of using the default charged attack behaviour,
        // lets make a simple FSM edit that does something slightly different.
        cfg.ChargedSlashFsmEdit = ChargedFsmEdit;

        void ChargedFsmEdit(PlayMakerFSM fsm, FsmState startState, out FsmState[] endStates)
        {
            FsmState attackState = fsm.AddState("Neo Slash");
            endStates = [attackState];

            startState.AddMethod(() => {
                HeroController.instance.RelinquishControlNotVelocity();
                HeroController.instance.SetStartWithDownSpikeEnd();
                HeroController.instance.SpriteFlash.flashFocusHeal();
                neoCrest.Moveset.ChargedSlash!.GameObject!.SetActive(true);
                foreach (var step in neoCrest.Moveset.ChargedSlash!.Steps)
                    step.EndAttack();
            });
            startState.AddActions(
                new Tk2dPlayAnimationWithEvents {
                    gameObject = new(),
                    clipName = "Slash_Charged",
                    animationTriggerEvent = FsmEvent.Finished,
                },
                new DecelerateV2 {
                    gameObject = new(),
                    deceleration = 0.6f,
                    brakeOnExit = true,
                }
            );
            startState.AddTransition(FsmEvent.Finished.name, attackState.name);

            attackState.AddMethod(() => {
                HeroController.instance.StartCoroutine(PlayStepsFaster());
                IEnumerator PlayStepsFaster()
                {
                    foreach (var step in neoCrest.Moveset.ChargedSlash!.Steps)
                    {
                        step.StartAttack();
                        yield return new WaitForSeconds(0.15f);
                    }
                }
            });
            attackState.AddAction(new Tk2dWatchAnimationEvents {
                gameObject = new(),
                animationCompleteEvent = FsmEvent.Finished,
            });
        }

        #endregion


        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}