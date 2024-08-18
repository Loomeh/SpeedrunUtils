using BepInEx;
using UnityEngine;
using HarmonyLib;

namespace SpeedrunUtils
{
    [BepInPlugin("brc.loomeh.speedrunutils", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject _mod;


        private void Awake()
        {
            _mod = new();
            _mod.AddComponent<TextManager>();
            _mod.AddComponent<LiveSplitControl>();
            _mod.AddComponent<DoAutoMash>();
            _mod.AddComponent<ConfigUi>();
            _mod.AddComponent<Tools>();
            GameObject.DontDestroyOnLoad(_mod);

            new Harmony("brc.loomeh.speedrunutils").PatchAll();
        }
    }
}

