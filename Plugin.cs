using BepInEx;
using UnityEngine;

namespace SpeedrunUtils
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject _mod;
        private Tools _tools;

        private void Awake()
        {
            _tools = new();

            _mod = new();
            _mod.AddComponent<LiveSplitControl>();
            _mod.AddComponent<DoAutoMash>();
            _mod.AddComponent<NinjaConfigUI>();
            _mod.AddComponent<Tools>();
            DontDestroyOnLoad(_mod);
        }
    }
}

