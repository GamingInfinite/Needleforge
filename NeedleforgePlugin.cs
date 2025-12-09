using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HutongGames.PlayMaker;
using Needleforge.Data;
using PrepatcherPlugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TeamCherry.Localization;
using UnityEngine;
#if DEBUG
using HutongGames.PlayMaker.Actions;
using Needleforge.Attacks;
using Needleforge.Components;
using Silksong.FsmUtil;
using System.Linq;
#endif

namespace Needleforge
{
    // TODO - adjust the plugin guid as needed
    [BepInAutoPlugin(id: "io.github.needleforge")]
    public partial class NeedleforgePlugin : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        public static Harmony harmony;

        public static List<ToolData> newToolData = new();
        public static List<CrestData> newCrestData = new();
        public static List<ToolCrest> newCrests = new();
        public static List<ToolItem> newTools = new();
        public static ObservableCollection<ColorData> newColors = new();

        public static Dictionary<string, GameObject> hudRoots = new();
        public static Dictionary<string, Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM>> bindEvents = new();
        public static Dictionary<string, Action> bindCompleteEvents = new();
        public static Dictionary<string, UniqueBindEvent> uniqueBind = new();

        public static Dictionary<string, Action> toolEventHooks = new();

        /// <summary>
        /// A custom tool color. Green slots can accept green, blue and yellow tools;
        /// and green tools can be equipped into green, blue, and yellow slots.
        /// </summary>
        public static readonly ColorData GreenTools = AddToolColor(
            "Green",
            new Color(0.57f, 0.86f, 0.59f, 1f)
        );

        /// <summary>
        /// A custom tool color. Pink slots can accept pink, white and red tools;
        /// and pink tools can be equipped into pink, white and red slots.
        /// </summary>
        public static readonly ColorData PinkTools = AddToolColor(
            "Pink",
            new Color(1f, 0.59f, 0.78f, 1f),
            true
        );

        /// <summary>
        /// A custom tool color. Black slots can accept tools of any color;
        /// and black tools can be equipped into slots of any color.
        /// </summary>
        public static readonly ColorData BlackTools = AddToolColor(
            "Black",
            new Color(0.40f, 0.40f, 0.40f, 1f),
            true
        );

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
            harmony = new("com.example.patch");
            harmony.PatchAll();

            newColors.CollectionChanged += NewColors_CollectionChanged;

            GreenTools.AddValidTypes(ToolItemType.Yellow, ToolItemType.Blue);
            PinkTools.AddValidTypes(ToolItemType.Red, ToolItemType.Skill);
            BlackTools.allColorsValid = true;

#if DEBUG
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);

            var neoCrest = AddCrest("NeoCrest");
            neoCrest.AddToolSlot(GreenTools.Type, AttackToolBinding.Neutral, Vector2.zero, false);
            neoCrest.AddToolSlot(PinkTools.Type, AttackToolBinding.Up, new(0, 2), false);
            neoCrest.AddToolSlot(BlackTools.Type, AttackToolBinding.Down, new(0, -2), false);
            neoCrest.AddBlueSlot(new(-2, -1), false);
            neoCrest.AddYellowSlot(new(2f, -1), false);
            neoCrest.AddRedSlot(AttackToolBinding.Neutral, new(-2, 1), false);
            neoCrest.AddSkillSlot(AttackToolBinding.Neutral, new(2f, 1), false);
            neoCrest.ApplyAutoSlotNavigation(angleRange: 80f);

            neoCrest.HudFrame.Preset = VanillaCrest.BEAST;

            neoCrest.Moveset.AltSlash = new Attack() {
                Name = "NeoSlashAlt",
                Hitbox = [new(0, 0), new(0, -1), new(-3, -1), new(-3, 0)],
                Scale = new(2, 1.25f),
                DamageMult = 4,
                KnockbackMult = 4,
                Color = Color.magenta,
                AnimName = "NeoSlashEffect",
			};

            neoCrest.Moveset.UpSlash = new Attack() {
                Name = "NeoSlashUp",
                Hitbox = [new(1, 0), new(1, 3), new(-1, 3), new(-1, 0)],
                KnockbackMult = 0.1f,
                MultiHitMultipliers = [0.3f, 0.25f, 0.25f, 0.25f],
                SilkGeneration = HitSilkGeneration.Full,
                Color = Color.yellow,
                AnimName = "NeoSlashEffect",
            };

            neoCrest.Moveset.WallSlash = new Attack() {
                Name = "NeoSlashWall",
                Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-3, -1.5f), new(-3, 1.5f)],
                Color = Color.blue,
                AnimName = "NeoSlashEffect",
            };

            neoCrest.Moveset.DownSlash = new DownAttack() {
                Name = "NeoSlashDown",
                Hitbox = [new(1, 0), new(1, -3), new(-1, -3), new(-1, 0)],
                Scale = new(2, 1),
                Color = Color.red,
                AnimName = "NeoSlashEffect",
            };

            neoCrest.Moveset.DashSlash = new DashAttack() {
                Name = "NeoSlashDash",
                Steps = [
                    new DashAttack.Step() {
                        Name = "NeoDash A",
                        Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-1, 0)],
                        Scale = new(2, 0.2f),
                        Color = Color.cyan,
                        AnimName = "DownSlash",
                    },
                    new DashAttack.Step() {
                        Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-1, 0)],
                        Scale = new(2, 0.2f),
                        Color = Color.yellow,
                        AnimName = "DownSlash",
                    },
                ],
            };

            neoCrest.Moveset.ChargedSlash = new ChargedAttack() {
                Name = "NeoSlashCharged",
                Steps = [
                    new ChargedAttack.Step() {
                        Name = "NeoCharge Alpha",
                        Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-1, 0)],
                        Scale = new(2, 0.2f),
                        Color = Color.cyan,
                        AnimName = "NeoSlashEffect",
                    },
					new ChargedAttack.Step() {
						Name = "NeoCharge Beta",
						Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-2, 0)],
						Scale = new(2, 0.2f),
						Color = Color.yellow,
						AnimName = "DownSlash",
					},
					new ChargedAttack.Step() {
						Hitbox = [new(0, 1.5f), new(0, -1.5f), new(-1, 0)],
						Scale = new(2, 0.2f),
						Color = Color.red,
						AnimName = "NeoSlashEffect",
					},
				],
            };

            var cfg = ScriptableObject.CreateInstance<HeroConfigNeedleforge>();

            cfg.canBind = true;
            cfg.SetCanUseAbilities(true);
            cfg.SetAttackFields(
                time: 0.35f, recovery: 0.15f, cooldown: 0.41f,
                quickSpeedMult: 1.5f, quickCooldown: 0.205f
            );
            cfg.wallSlashSlowdown = true;
            cfg.SetCustomDownslash("NEO DOWNSLASH", DownslashFsmTest);
            //cfg.SetDashSlashFields(time: 0.2f, speed: -10, bounceJumpSpeed: 40);
            cfg.DashSlashFsmEdit = DashSlashFsmTest;
            cfg.SetChargedSlashFields(doesKickoff: true, lungeSpeed: 0.5f, lungeDeceleration: 0.5f);

            void DownslashFsmTest(PlayMakerFSM fsm, FsmState startState, out FsmState[] endStates)
            {
                startState.AddLambdaMethod(actionFinished => {
                    Debug.Log("custom downslash fsm edit");
                    var hc = HeroController.instance;
                    hc.QueueCancelDownAttack();
                    hc.cState.downAttacking = true;
                    hc.rb2d.linearVelocity = new(5, 20);
                    neoCrest.Moveset.DownSlash!.GameObject!.GetComponent<NailSlash>().StartSlash();
                    actionFinished();
                });
                endStates = [startState];
            }

            void DashSlashFsmTest(PlayMakerFSM fsm, FsmState startState, out FsmState[] endStates) {
                startState.AddLambdaMethod(finished => {
                    Debug.Log("custom dashslash fsm edit");
                    var hc = HeroController.instance;
                    hc.RelinquishControl();
                    hc.CancelDash(false);
                    hc.rb2d.linearVelocity = new(10, 20);
                    if (hc.cState.facingRight)
                        hc.rb2d.linearVelocityX *= -1;
                    hc.animCtrl.PlayFromFrame("Dash Attack 1", 0, true);
                    var attackobj = neoCrest.Moveset.DashSlash!.GameObject!.transform.GetChild(0);
                    var attackComponent = attackobj.GetComponent<DashStabWithOwnAnim>();
                    attackComponent.OnCancelAttack();
                    attackComponent.StartSlash();
                    finished();
                });
                startState.AddAction(new Tk2dWatchAnimationEvents() { BlocksFinish = true, gameObject = new() { OwnerOption = OwnerDefaultOption.UseOwner } });
                startState.AddLambdaMethod(finished => {
                    var hc = HeroController.instance;
                    var attackobj = neoCrest.Moveset.DashSlash!.GameObject!.transform.GetChild(0);
                    var attackComponent = attackobj.GetComponent<DashStabWithOwnAnim>();
                    attackComponent.OnCancelAttack();
                    hc.ResetGravity();
                    hc.CrestAttackRecovery();
                    finished();
                });
                endStates = [startState];
            }

            neoCrest.Moveset.HeroConfig = cfg;

            // Attacks require an animation to function and adding test assets
            // to needleforge itself seemed unnecessary
            neoCrest.Moveset.OnInitialized += () => {
                if (GameObject.Find("NeoAnimLib") is GameObject libobj)
                    return;

                var hc = HeroController.instance;
                libobj = new GameObject("NeoAnimLib");
                DontDestroyOnLoad(libobj);
                var lib = libobj.AddComponent<tk2dSpriteAnimation>();

                var frames = hc.animCtrl.animator.Library.GetClipByName("Sprint").frames;
                tk2dSpriteAnimationFrame[] CloneFrames() => [..
                    frames.Select(f => new tk2dSpriteAnimationFrame() {
                        spriteCollection = f.spriteCollection, spriteId = f.spriteId, triggerEvent = false
                    })
                ];

                var standardclip = new tk2dSpriteAnimationClip() {
                    name = "NeoSlashEffect", fps = 20, frames = CloneFrames(),
                    wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
                };
                standardclip.frames[0].triggerEvent = true;
                standardclip.frames[^1].triggerEvent = true;

                var downspikeclip = new tk2dSpriteAnimationClip() {
                    name = "NeoSlashDownEffect", fps = 20, frames = CloneFrames(),
                    wrapMode = tk2dSpriteAnimationClip.WrapMode.Once,
                };

                var wanderer = hc.configs.First(c => c.Config.name == "Wanderer").Config.heroAnimOverrideLib;
                var witch = hc.configs.First(c => c.Config.name == "Whip").Config.heroAnimOverrideLib;

                lib.clips = [
                    // effect animations
                    standardclip,
                    downspikeclip,

                    // hornet anim for testing the regular downslash
                    wanderer.GetClipByName("DownSlash"),

                    // anims for testing dash attacks
                    witch.GetClipByName("Dash Attack Antic 1"),
                    witch.GetClipByName("Dash Attack Antic 2"),
                    witch.GetClipByName("Dash Attack 1"),
                    witch.GetClipByName("Dash Attack 2"),
                ];

                AttackBase[] atks = [
                    neoCrest.Moveset.AltSlash,
                    neoCrest.Moveset.UpSlash,
                    neoCrest.Moveset.WallSlash,
                    neoCrest.Moveset.DownSlash,
                ];

                for (int i = 0; i < atks.Length; i++)
                    atks[i].AnimLibrary = lib;
                //neoCrest.Moveset.DownSlash.AnimName = downspikeclip.name;
                neoCrest.Moveset.DashSlash?.SetAnimLibrary(lib);
				neoCrest.Moveset.ChargedSlash?.SetAnimLibrary(lib);
				neoCrest.Moveset.HeroConfig.heroAnimOverrideLib = lib;
            };

            AddTool("NeoGreenTool", GreenTools.Type);
            AddTool("NeoBlackTool", BlackTools.Type);
#endif
        }

        private void NewColors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Doing this so the static variable updates
            // whenever a user of needleforge adds a color, in theory
            InventoryToolCrest.TOOL_TYPES = (ToolItemType[])Enum.GetValues(typeof(ToolItemType));
        }

        public static ToolData GetToolDataByName(string name)
        {
            foreach (var t in newToolData)
            {
                if (t.name == name)
                {
                    return t;
                }
            }

            return null;
        }

        public static ColorData AddToolColor(string name, Color color, bool isAttackType = false)
        {
            ColorData newColor = new()
            {
                name = name,
                color = color,
                isAttackType = isAttackType
            };
            newColors.Add(newColor);
            return newColor;
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
            string InfiniteRefillsPD, Color liquidColor,
            ToolItem.ReplenishResources resource, ToolItem.ReplenishUsages replenishUsage, float replenishMult,
            StateSprites? fullSprites, StateSprites? emptySprites,
            string clip = "Charge Up")
        {
            LiquidToolData data = new()
            {
                type = ToolItemType.Red,
                name = name,

                color = liquidColor,
                maxRefills = maxRefills,
                storageAmount = storageAmount,
                infiniteRefills = InfiniteRefillsPD,

                clip = clip,

                resource = resource,
                replenishUsage = replenishUsage,
                replenishMult = replenishMult,

                FullSprites = fullSprites,
                EmptySprites = emptySprites
            };

            newToolData.Add(data);
            toolEventHooks[$"{data.name} BEFORE ANIM"] = () => { ModHelper.Log($"BEFORE ANIM for {data.name}"); };
            toolEventHooks[$"{data.name} AFTER ANIM"] = () => { ModHelper.Log($"AFTER ANIM for {data.name}"); };

            return data;
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
            string InfiniteRefillsPD, Color liquidColor, ToolItem.ReplenishResources resource,
            ToolItem.ReplenishUsages replenishUsage, float replenishMult)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor, resource,
                replenishUsage, replenishMult, null, null, "Charge Up");
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount,
            string InfiniteRefillsPD, Color liquidColor)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, InfiniteRefillsPD, liquidColor,
                ToolItem.ReplenishResources.Shard, ToolItem.ReplenishUsages.Percentage, 1f);
        }

        public static LiquidToolData AddLiquidTool(string name, int maxRefills, int storageAmount, Color liquidColor)
        {
            return AddLiquidTool(name, maxRefills, storageAmount, "", liquidColor);
        }

        public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName,
            LocalisedString description, Sprite? InventorySprite)
        {
            ToolData data = new()
            {
                inventorySprite = InventorySprite,
                type = type,
                name = name,
                displayName = displayName,
                description = description,
            };

            PlayerDataVariableEvents.OnGetBool += (pd, fieldname, current) =>
            {
                if (fieldname == data.unlockedPDString)
                {
                    return data.UnlockedAtStart;
                }

                return current;
            };

            newToolData.Add(data);
            return data;
        }

        public static ToolData AddTool(string name, ToolItemType type, Sprite? InventorySprite)
        {
            return AddTool(name, type, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" },
                new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, InventorySprite);
        }

        public static ToolData AddTool(string name, ToolItemType type)
        {
            return AddTool(name, type, null);
        }

        public static ToolData AddTool(string name, ToolItemType type, LocalisedString displayName,
            LocalisedString description)
        {
            return AddTool(name, type, displayName, description, null);
        }

        public static ToolData AddTool(string name, LocalisedString displayName, LocalisedString description)
        {
            return AddTool(name, ToolItemType.Yellow, displayName, description, null);
        }

        public static ToolData AddTool(string name)
        {
            return AddTool(name, ToolItemType.Yellow, null);
        }

        /// <summary>
        /// Adds your class with the sprites already attached.
        /// <para/>
        /// IMPORTANT: <br/>
        /// for obvious reasons certain data will return either null or some default value (eg. false for bool) until a Save is loaded.
        /// </summary>
        /// <param name="name">Name of the Crest</param>
        /// <param name="RealSprite">Inventory Sprite</param>
        /// <param name="Silhouette">Crest List Sprite</param>
        /// <returns><see cref="CrestData"/></returns>
        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
            Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
        {
            CrestData crestData = new(name, displayName, description, RealSprite, Silhouette, CrestGlow);

            newCrestData.Add(crestData);
            bindEvents[name] = (value, amount, time, fsm) => { ModHelper.Log($"Running Bind for {name} Crest"); };
            bindCompleteEvents[name] = () => { ModHelper.Log($"Bind for {name} Crest Complete"); };

            return crestData;
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette, Sprite? CrestGlow)
        {
            return AddCrest(name, new() { Key = $"{name}LocalKey", Sheet = "Mods.your.mod.id" },
                new() { Key = $"{name}LocalKeyDesc", Sheet = "Mods.your.mod.id" }, RealSprite, Silhouette, CrestGlow);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
            Sprite? RealSprite, Sprite? Silhouette)
        {
            return AddCrest(name, displayName, description, RealSprite, Silhouette, null);
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite, Sprite? Silhouette)
        {
            return AddCrest(name, RealSprite, Silhouette, null);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description,
            Sprite? RealSprite)
        {
            return AddCrest(name, displayName, description, RealSprite, null);
        }

        public static CrestData AddCrest(string name, Sprite? RealSprite)
        {
            return AddCrest(name, RealSprite, null);
        }

        public static CrestData AddCrest(string name, LocalisedString displayName, LocalisedString description)
        {
            return AddCrest(name, displayName, description, null);
        }

        /// <summary>
        /// Adds a named Crest
        /// </summary>
        /// <param name="name">Name of the Crest</param>
        /// <returns><see cref="CrestData"/></returns>
        public static CrestData AddCrest(string name)
        {
            return AddCrest(name, null);
        }
    }
}