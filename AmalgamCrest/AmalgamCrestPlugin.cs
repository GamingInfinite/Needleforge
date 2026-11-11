using BepInEx;
using Needleforge;
using Needleforge.Attacks;
using Needleforge.Data;
using TeamCherry.Localization;
using UnityEngine;

namespace AmalgamCrest;

[BepInAutoPlugin(id: "io.github.amalgamcrest")]
[BepInDependency("org.silksong-modding.i18n", "1.0.2")]
public partial class AmalgamCrestPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);

        // The purpose of this example is to demonstrate vanilla moveset
        // cloning and modification, so we'll be skipping usual basics like images
        // or animations.

        #region Registration
        //As usual, registering a crest has to be done within Awake.
        CrestData amalgamCrest = NeedleforgePlugin.AddCrest(
            name:"AmalgamCrest",
            // Won't skip text loading, though.
            displayName: new LocalisedString($"Mods.{Id}", "CREST_NAME"),
            description: new LocalisedString($"Mods.{Id}", "CREST_DESC"));

        amalgamCrest.HudFrame.Preset = VanillaCrest.SHAMAN;
        #endregion

        //Only moveset is implemented in this example.

        #region Moveset - Vanilla Cloning

        // When working with vanilla movesets, you can use the VanillaAttacks class to get a copy of the attack.
        // The following line is an example of that. It will give you a copy of Architect's slash attack.
        // These are versions of the attacks represented as the Attack class, which you can modify to your liking.
        amalgamCrest.Moveset.Slash = VanillaAttacks.Architect.Slash();

        // If you'd like to modify the attack, you can do so by changing the properties of the Attack object.
        // Here, I'll give my Reaper slash higher damage output like the Higher Beings intended.
        Attack ReaperSlash = VanillaAttacks.Reaper.Slash();
        ReaperSlash.DamageMult = 1.5f;

        amalgamCrest.Moveset.AltSlash = ReaperSlash;

        // I'll quickly fill in the rest of the slots.
        amalgamCrest.Moveset.UpSlash = VanillaAttacks.Witch.UpSlash();
        amalgamCrest.Moveset.AltUpSlash = VanillaAttacks.Beast.UpSlashRage();
        amalgamCrest.Moveset.WallSlash = VanillaAttacks.Hunter.WallSlash();


        // For the downslash, dashslash, and charged slash, you can use the corresponding properties.
        // These enums are used to tell the moveset to use the vanilla attack and animation for that crest.
        // The animation can be overridden - I'll demonstrate soon.
        amalgamCrest.Moveset.UseVanillaDashSlash = VanillaCrest.WITCH;

        amalgamCrest.Moveset.UseVanillaChargedSlash = VanillaCrest.WANDERER;

        amalgamCrest.Moveset.UseVanillaDownSlash = VanillaCrest.SHAMAN;

        // You may have noted that these are enums, and not an Attack object.
        // This means to modify them, you'll need the moveset to have already initialised.
        // Luckily, there's an event for that.

        amalgamCrest.Moveset.OnInitialized += () =>
        {
            // This is where you can modify the properties of the attacks directly through
            // the GameObject.
            // Here's a few examples.

            //Making Shaman's downslash move a little faster, and it a little smaller.
            GameObject? downSlash = amalgamCrest.Moveset.DownSlash?.GameObject;
            if (downSlash != null)
            {
                NailSlash slash = downSlash.GetComponent<NailSlash>();
                slash.scale = new Vector2(0.75f, 0.75f);

                NailSlashTravel slashTravel = downSlash.GetComponent<NailSlashTravel>();
                slashTravel.travelDuration *= 0.75f;
                slashTravel.travelDistance *= 1.2f;
            }


            // Modifying dash slash to heal you on hit.
            // I'll make it smaller to balance it a little.
            // Note that since the dash slashes use the DashAttack object, the GameObject is 
            // accessed through the Steps field. Guidance on which Step is which attack can
            // be found in the summary of UseVanillaDashSlash.

            // In our case, Witch's Step 0 is the first slash and Step 1 is the second.
            // I'll work with the second.

            GameObject? dashSlashFinal = amalgamCrest.Moveset.DashSlash?.Steps[1].GameObject;
            if (dashSlashFinal != null)
            {
                dashSlashFinal.transform.localScale -= new Vector3(0.4f, 0.2f, 0);
                dashSlashFinal.GetComponent<DamageEnemies>().DamagedEnemy += HealForOne;
            }

            void HealForOne()
            {
                HeroController.instance.AddHealth(1);
            }

            // Finally, modifying charged slash scale and hit time.
            GameObject? chargeSlash = amalgamCrest.Moveset.ChargedSlash?.GameObject;
            if (chargeSlash != null)
            {
                chargeSlash.transform.localScale = new Vector3(1.2f, 1.8f, 1);
                chargeSlash.transform.localPosition += new Vector3(-0.5f, 0.5f, 0);
                chargeSlash.GetComponent<DamageEnemies>().stepsPerHit /= 2;
                chargeSlash.GetComponent<DamageEnemies>().nailDamageMultiplier = 0.25f;
            }


            #region Mini Segment - Animations
            // All animations should be plug-and-play, if you don't feel like adding your own.
            // If you do want to use a custom one, you can add them whenever you'd like.
            // To clone from Hornet, it's likely you'd wait until here, but not necessarily.
            // Just note that if you're using a vanilla charged, dash, or downslash, Needleforge may
            // automatically create an animation library for you. If you do wait until now - now meaning
            // after the moveset has initialised - to add animations,
            // don't create a new library. Just add to the existing one. If you create a library at Awake(),
            // there's no issue.
            #endregion

        };

        #endregion

        #region Moveset - Hero Config
        // As a quick reminder, HeroConfig controls a lot of Hornet's behaviour.
        // Attack speed, properties, animations, etc.
        // This example is a follow-up to Neo Crest, which will have more info on the HeroConfig.
        // Here, we'll stick to basics.

        var cfg = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();
        amalgamCrest.Moveset.HeroConfig = cfg;

        cfg.canBind = true;
        cfg.SetCanUseAbilities(true);
        cfg.SetAttackFields(
            time: 0.35f, recovery: 0.15f, cooldown: 0.41f, // Attack speed
            quickSpeedMult: 1.5f, quickCooldown: 0.205f // Flea Brew attack speed
        );
        cfg.wallSlashSlowdown = true;


        // Important note.
        // The vanilla clones are a bit of a mixed bag when it comes to these properties.
        // For example, Reaper's downslash ignores the downspike fields, Hunter's does not.
        // Dashstab fields, too, have similar inconsistencies.
        // Generally, it'll be Hunter and Cloakless that use downspike.
        // Hunter, Cloakless, partially Witch use dashstab.
        // To edit most of the clones, you'll need to edit the FSMs directly.
        // Or recreate the attacks from scratch. Just be aware that editing FSMs
        // will also change the vanilla behaviour, so ensure you're adding checks for if
        // your own crest is equipped.


        // You will likely have to tinker with these values to get the feel you want.
        // You don't need to set the DownSpike type if using a vanilla clone, but
        // downspike fields might still be used.
        cfg.SetDownspikeFields(
            anticTime: 0.1f, time: 0.15f, recoveryTime: 0.05f,
            doesThrust: true, speed: 15, acceleration: new Vector2(20, 30),
            doesBurstEffect: true
        );

        // Time and speed might be ignored by certain types, bounce is sometimes used.
        // Hunter, Cloakless, and partially Witch use these the most.
        // Some other may use bounce.
        // For example, Witch uses the 'time' as the time between slashes.
        // Too high and you skip half the attack.
        cfg.SetDashStabFields(time: 0.1f, speed: -20, bounceJumpSpeed: 10);

        // It should be possible to edit the 'chain' property of charged slash to allow
        // for mashing to extend the attack.
        // The vanilla attacks weren't made for this behaviour, so it looks janky if you
        // go beyond what the vanilla attacks were made for. But you can try it if you want.
        // Wanderer and Beast can't have their chain modified, as they use different logic.
        // The charged slash properties are almost always used by all clones.
        cfg.SetChargedSlashFields(doesKickoff: true, chain: 3);
        cfg.chargeSlashLungeDeceleration = 1;
        cfg.chargeSlashLungeSpeed = 0;

        #endregion

        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}