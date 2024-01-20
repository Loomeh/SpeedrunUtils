using BepInEx;
using UnityEngine;

namespace SpeedrunUtils
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject _mod;

        private void Awake()
        {
            _mod = new();
            _mod.AddComponent<Text>();
            _mod.AddComponent<LiveSplitControl>();
            _mod.AddComponent<DoAutoMash>();
            _mod.AddComponent<NinjaConfigUI>();
            DontDestroyOnLoad(_mod);
        }
    }
}

