using HarmonyLib;
using Reptile;
using UnityEngine;
using BepInEx;
using System;

namespace SpeedrunUtils
{
    [HarmonyPatch]
    internal class MouseMenuFix
    {
        public static readonly string settingsPath = Paths.ConfigPath + @"\SpeedrunUtils\Settings.txt";
        private static bool isMouseMenuFixEnabled;

        static MouseMenuFix()
        {
            // Read the mouse menu fix setting
            isMouseMenuFixEnabled = bool.Parse(SettingsManager.GetSetting(settingsPath, "Mouse Menu Fix", "true"));
        }

        [HarmonyPatch(typeof(TextMeshProMenuButton), "OnPointerEnter")]
        [HarmonyPrefix]
        public static bool OnPointerEnter_Prefix()
        {
            return !isMouseMenuFixEnabled || Cursor.visible;
        }

        [HarmonyPatch(typeof(TextMeshProMenuButton), "OnPointerExit")]
        [HarmonyPrefix]
        public static bool OnPointerExit_Prefix()
        {
            return !isMouseMenuFixEnabled || Cursor.visible;
        }
    }
}