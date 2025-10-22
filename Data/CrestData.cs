using System;
using System.Collections.Generic;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Needleforge.Data
{
    public class UniqueBindEvent
    {
        public string Direction;
        public Action<Action> lambdaMethod;

        public UniqueBindEvent(string direction, Action<Action> lambdaMethod)
        {
            this.Direction = direction;
            this.lambdaMethod = lambdaMethod;
        }
    }
    public class CrestData
    {
        public Sprite? RealSprite;
        public Sprite? Silhouette;
        public HeroControllerConfig? AttackConfig;
        public List<ToolCrest.SlotInfo> slots = [];
        public int bindCost = 9;
        public string name = "";

        public Action<FsmInt, FsmInt, FsmFloat, PlayMakerFSM> BindEvent
        {
            get
            {
                return NeedleforgePlugin.bindEvents[name];
            }
            set
            {
                NeedleforgePlugin.bindEvents[name] = value;
            }
        }
        public UniqueBindEvent uniqueBindEvent
        {
            get
            {
                return NeedleforgePlugin.uniqueBind[name];
            }
            set
            {
                NeedleforgePlugin.uniqueBind[name] = value;
            }
        }

        public ToolCrest? ToolCrest
        {
            get
            {
                if (HeroController.instance != null)
                {
                    foreach(var crest in NeedleforgePlugin.newCrests)
                    {
                        if (crest.name == name)
                        {
                            return crest;
                        }
                    }
                }
                return null;
            }
        }

        public bool IsEquipped
        {
            get
            {
                if (ToolCrest != null)
                {
                    return ToolCrest.IsEquipped;
                }
                return false;
            }
        }

        public CrestData(string name, Sprite? RealSprite, Sprite? Silhouette)
        {
            this.name = name;
            this.RealSprite = RealSprite;
            this.Silhouette = Silhouette;
        }
    }
}
