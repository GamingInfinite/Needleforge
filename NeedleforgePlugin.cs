using System.Collections.Generic;
using BepInEx;
using Needleforge.Makers;
using UnityEngine;

namespace Needleforge
{
    // TODO - adjust the plugin guid as needed
    [BepInAutoPlugin(id: "io.github.needleforge")]
    public partial class NeedleforgePlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Put your initialization logic here
            Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
        }
    }
}