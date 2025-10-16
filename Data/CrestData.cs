using System;
using System.Collections.Generic;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Needleforge.Data
{
    public class CrestData
    {
        public Sprite? RealSprite;
        public Sprite? Silhouette;
        public HeroControllerConfig? AttackConfig;
        public string name = "";

        public Action<FsmInt, FsmInt, FsmFloat> bindEvent
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

        public ToolCrest? toolCrest
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

        public CrestData(string name, Sprite? RealSprite, Sprite? Silhouette)
        {
            this.name = name;
            this.RealSprite = RealSprite;
            this.Silhouette = Silhouette;
        }
    }
}
