using BepInEx;
using Needleforge;
using Needleforge.Attacks;
using Needleforge.Data;
using System.Linq;
using UnityEngine;

namespace ExampleCrest;

[BepInAutoPlugin(id: "io.github.examplecrest")]
public partial class ExampleCrestPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);


        NeedleforgePlugin.AddTool("NeoGreenTool", NeedleforgePlugin.GreenTools.Type);
        NeedleforgePlugin.AddTool("NeoBlackTool", NeedleforgePlugin.BlackTools.Type);


        var neoCrest = NeedleforgePlugin.AddCrest("NeoCrest");

        neoCrest.HudFrame.Preset = VanillaCrest.BEAST;

        #region Tool Slots

        neoCrest.AddToolSlot(NeedleforgePlugin.GreenTools.Type, AttackToolBinding.Neutral, Vector2.zero, false);
        neoCrest.AddToolSlot(NeedleforgePlugin.PinkTools.Type, AttackToolBinding.Up, new(0, 2), false);
        neoCrest.AddToolSlot(NeedleforgePlugin.BlackTools.Type, AttackToolBinding.Down, new(0, -2), false);
        neoCrest.AddBlueSlot(new(-2, -1), false);
        neoCrest.AddYellowSlot(new(2f, -1), false);
        neoCrest.AddRedSlot(AttackToolBinding.Neutral, new(-2, 1), false);
        neoCrest.AddSkillSlot(AttackToolBinding.Neutral, new(2f, 1), false);

        neoCrest.ApplyAutoSlotNavigation(angleRange: 80f);

        #endregion

        #region Moveset

        // The HeroConfig controls Hornet's behaviour when attacking with this crest;
        // this includes things like her animations, attack speeds, and several properties
        // of how down, dash, and charged attacks behave.
        var cfg = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();
        neoCrest.Moveset.HeroConfig = cfg;

        cfg.canBind = true;
        cfg.SetCanUseAbilities(true);
        cfg.SetAttackFields(
            time: 0.35f, recovery: 0.15f, cooldown: 0.41f,
            quickSpeedMult: 1.5f, quickCooldown: 0.205f
        );
        cfg.wallSlashSlowdown = true;

        cfg.downSlashType = HeroControllerConfig.DownSlashTypes.DownSpike;
        cfg.SetDownspikeFields(
            anticTime: 0.1f, time: 0.15f, recoveryTime: 0.05f,
            doesThrust: true, speed: 15, acceleration: new Vector2(20, 30),
            doesBurstEffect: true
        );

        cfg.SetDashStabFields(time: 0.4f, speed: -20, bounceJumpSpeed: 40);
        cfg.SetChargedSlashFields(doesKickoff: true, chain: 3);


        // NOTE: Because we're not specifying a regular Slash attack,
        // the crest will automatically use Hunter crest's regular slash.


        // The minimum requirements for creating a custom attack are
        // a Hitbox and a valid animation. (AnimLibrary is required; we set it later on)
        neoCrest.Moveset.AltSlash = new Attack() {
            Name = "NeoSlashAlt",
            Hitbox = [new(0, 0), new(0, -1), new(-3, -1), new(-3, 0)],
            AnimName = "NeoSlashEffect",
            Color = Color.magenta,
        };

        // Attacks have several customizable properties aside from the required ones.
        // This one has reduced knockback, hits up to 4 times, and generates silk every hit.
        neoCrest.Moveset.UpSlash = new Attack() {
            Name = "NeoSlashUp",
            Hitbox = [new(1, 0), new(1, 3), new(-1, 3), new(-1, 0)],
            AnimName = "NeoSlashEffect",
            Color = Color.yellow,
            KnockbackMult = 0.1f,
            MultiHitMultipliers = [0.3f, 0.25f, 0.25f, 0.25f],
            SilkGeneration = HitSilkGeneration.Full,
        };

        neoCrest.Moveset.WallSlash = new Attack() {
            Name = "NeoSlashWall",
            Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-3, -1.5f), new(-3, 1.5f)],
            AnimName = "NeoSlashEffect",
            Color = Color.blue,
        };

        neoCrest.Moveset.DownSlash = new DownAttack() {
            Name = "NeoSlashDown",
            Hitbox = [new(1, 0), new(1, -2), new(-1, -2), new(-1, 0)],
            AnimName = "NeoSlashDownEffect",
            Color = Color.red,
            Scale = new(2, 1),
        };

        // Dash and Charged slashes are multi-step attacks. Each step has all the
        // customization options of a regular slash.
        // By default each step of a multi-step attack plays in sequence. If you want to
        // change that behaviour you need to add an FSM edit to the HeroConfig.
        neoCrest.Moveset.DashSlash = new DashAttack() {
            Name = "NeoSlashDash",
            Steps = [
                new DashAttack.Step() {
                    Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-1, 0)],
                    AnimName = "NeoSlashEffect",
                    Color = Color.cyan,
                    Scale = new(2, 0.4f),
                },
                new DashAttack.Step() {
                    Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-2, 0)],
                    AnimName = "NeoSlashEffect",
                    Color = Color.magenta,
                    Scale = new(2, 0.5f),
                },
            ],
        };

        neoCrest.Moveset.ChargedSlash = new ChargedAttack() {
            Name = "NeoSlashCharged",
            // Charged attacks can have their steps shake the camera or cause the screen
            // to flash a particular colour, too.
            CameraShakeProfiles = [
                GlobalSettings.Camera.EnemyKillShake,
            ],
            ScreenFlashColors = [
                new(1, 1, 1, 0.5f),
            ],
            Steps = [
                new ChargedAttack.Step() {
                    Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-2, 0)],
                    AnimName = "NeoSlashEffect",
                    Color = Color.yellow,
                    Scale = new(2, 0.3f),
                    CameraShakeIndex = 0,
                    ScreenFlashIndex = 0,
                },
                new ChargedAttack.Step() {
                    Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-3, 0)],
                    AnimName = "NeoSlashEffect",
                    Color = Color.magenta,
                    Scale = new(2, 0.3f),
                },
            ],
        };

        // Attacks require effect animations to function. For a quick test, we'll borrow
        // some animations for Hornet to make some "attack effects". The easiest time to
        // find them is after a save has been loaded.
        // If you're importing your own animation assets, you can do this in your mod's
        // Awake function with the rest of the crest set up.
        neoCrest.Moveset.OnInitialized += () => {
            if (GameObject.Find("NeoAnimLib") is GameObject libobj)
                return;

            var hc = HeroController.instance;
            libobj = new GameObject("NeoAnimLib");
            DontDestroyOnLoad(libobj);

            var animLibrary = libobj.AddComponent<tk2dSpriteAnimation>();
            AddTestAnimationsToLibrary(animLibrary, hc);

            neoCrest.Moveset.AltSlash.AnimLibrary = animLibrary;
            neoCrest.Moveset.UpSlash.AnimLibrary = animLibrary;
            neoCrest.Moveset.WallSlash.AnimLibrary = animLibrary;
            neoCrest.Moveset.DownSlash.AnimLibrary = animLibrary;

            neoCrest.Moveset.DashSlash.SetAnimLibrary(animLibrary);
            neoCrest.Moveset.ChargedSlash.SetAnimLibrary(animLibrary);

            // The test animations also include some animations Hornet needs to perform
            // the example dash slash, so we pass the library to Hornet as well.
            neoCrest.Moveset.HeroConfig.heroAnimOverrideLib = animLibrary;
        };

        #endregion

    }

    private static void AddTestAnimationsToLibrary(tk2dSpriteAnimation animLibrary, HeroController hc)
    {
        // Just for a quick test, we're using Hornet's sprint animation for our "attack effect" animations.
        // TODO: add actual custom assets for test animations

        var sprintFrames = hc.animCtrl.animator.Library.GetClipByName("Sprint").frames;
        
        // Most attacks need frames that trigger events to tell the hitbox when to appear and disappear.
        var testSlashEffect = new tk2dSpriteAnimationClip() {
            name = "NeoSlashEffect",
            fps = 20,
            frames = CloneFrames(sprintFrames, 4),
            wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
        };
        testSlashEffect.frames[0].triggerEvent = true;
        testSlashEffect.frames[^1].triggerEvent = true;

        // Downspike-type down attacks, specifically, need to have *zero* event frames.
        var testDownspikeEffect = new tk2dSpriteAnimationClip() {
            name = "NeoSlashDownEffect",
            fps = 20,
            frames = CloneFrames(sprintFrames),
            wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop,
        };

        // We also need to borrow some animations from Witch crest for Hornet in order to
        // test out dash slashes.
        var witch = hc.configs.First(c => c.Config.name == "Whip").Config.heroAnimOverrideLib;

        animLibrary.clips = [
            // attack effects
            testSlashEffect,
            testDownspikeEffect,

            // hero override anims
            witch.GetClipByName("Dash Attack Antic 2"),
            witch.GetClipByName("Dash Attack 2"),
        ];
        animLibrary.isValid = false;
        animLibrary.ValidateLookup();
    }

    /// <summary>
    /// Creates an array of new frame objects with the same sprites as the original frames.
    /// </summary>
    private static tk2dSpriteAnimationFrame[] CloneFrames(tk2dSpriteAnimationFrame[] frames, int? count = null)
    {
        count ??= frames.Length;
        return [..
            frames
                .Select(f =>
                    new tk2dSpriteAnimationFrame() {
                        spriteCollection = f.spriteCollection,
                        spriteId = f.spriteId,
                        triggerEvent = false
                    }
                )
                .Take((int)count)
        ] ;
    }

}
