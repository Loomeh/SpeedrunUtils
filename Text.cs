﻿using BepInEx;
using System.IO;
using System.Linq;
using UnityEngine;


namespace SpeedrunUtils
{
    public class Text : MonoBehaviour
    {
        private static string configFolder = $@"{Paths.ConfigPath}\SpeedrunUtils\";
        private readonly string langPath = Path.Combine(configFolder, "LANG.txt");

        public string lang = "";

        public string amEnabledHint = "";
        public string amDisabledHint = "";
        public string keys = "";
        public string lockFps = "";
        public string unlockFps = "";
        public string toggleAM = "";
        public string toggleMenu = "";
        public string fpsCap = "";
        public string setFpsCap = "";
        public string connectToLS = "";
        public string connectedToLS = "";
        public string notConnectedToLS = "";
        public string settings = "";
        public string credits = "";
        public string displayFPS = "";
        public string amStartsEnabled = "";
        public string startFPSCapped = "";
        public string displayFpsSize = "";
        public string displayFpsX = "";
        public string displayFpsY = "";
        public string save = "";
        public string cancel = "";
        public string enabledTXT = "";
        public string DisabledTXT = "";

        public bool initialised = false;

        public static Text Instance;

        public Text()
        {
            Instance = this;
        }


        public void Awake()
        {
            if (!File.Exists(langPath))
            {
                File.WriteAllText(langPath, "Language selector\nSupported languages: EN,JPN,IT\n---------------------------\n");

                switch (Application.systemLanguage)
                {
                    case SystemLanguage.English:
                        File.AppendAllText(langPath, "EN");
                        break;
                    case SystemLanguage.Japanese:
                        File.AppendAllText(langPath, "JPN");
                        break;
                    case SystemLanguage.Italian:
                        File.AppendAllText(langPath, "IT");
                        break;
                    default:
                        File.AppendAllText(langPath, "EN");
                        break;
                }
            }
            else
            {
                lang = File.ReadLines(langPath).Skip(3).Take(1).First();
            }
            Debug.Log($"Current language: {lang}");

            
            if (lang == "EN")
            {
                amEnabledHint = "AutoMash Enabled";
                amDisabledHint = "AutoMash Disabled";
                keys = "Keys:";
                lockFps = "Lock FPS";
                unlockFps = "Unlock FPS";
                toggleAM = "Toggle Automash";
                toggleMenu = "Toggle Menu";
                fpsCap = "FPS Cap";
                setFpsCap = "Set FPS Cap";
                connectToLS = "Connect to LiveSplit";
                connectedToLS = "Connected to LiveSplit";
                notConnectedToLS = "Not connected to LiveSplit";
                settings = "Settings...";
                credits = "Developed by <color=purple>Loomeh</color> and <color=#F97CE4>Ninja Cookie</color>";
                displayFPS = "Display FPS";
                amStartsEnabled = "AutoMash Starts Enabled";
                startFPSCapped = "Start FPS Capped";
                displayFpsSize = "Display FPS Size";
                displayFpsX = "Display FPS x";
                displayFpsY = "Display FPS y";
                save = "Save...";
                cancel = "Cancel";
                enabledTXT = "Enabled";
                DisabledTXT = "Disabled";
            }
            else if (lang == "JPN")
            {
                amEnabledHint = "自動マッシング有効";
                amDisabledHint = "自動マッシング無効";
                keys = "キー：";
                lockFps = "FPSを制限する";
                unlockFps = "FPSの制限を解除する";
                toggleAM = "自動マッシングの切り替え";
                toggleMenu = "メニューの切り替え";
                fpsCap = "FPS制限";
                setFpsCap = "FPS制限設定";
                connectToLS = "LiveSplitに接続";
                connectedToLS = "LiveSplitに接続済み";
                notConnectedToLS = "LiveSplitに未接続";
                settings = "設定...";
                credits = "<color=purple>Loomeh</color>と<color=#F97CE4>Ninja Cookie</color>によって開発。";
                displayFPS = "FPSを表示";
                amStartsEnabled = "自動マッシングは有効で開始します。";
                startFPSCapped = "FPSは制限されて開始します。";
                displayFpsSize = "FPSサイズを表示";
                displayFpsX = "FPSを x で表示";
                displayFpsY = "FPSを y で表示";
                save = "保存...";
                cancel = "キャンセル";
                enabledTXT = "有効。";
                DisabledTXT = "無効。";
            }
            else if (lang == "IT") //by Erisrine
            {
                amEnabledHint = "AutoMash Abilitato";
                amDisabledHint = "AutoMash Disabilitato";
                keys = "Tasti:";
                lockFps = "Blocca FPS";
                unlockFps = "Sblocca FPS";
                toggleAM = "Abilita Automash";
                toggleMenu = "Abilita Menu";
                fpsCap = "Limite FPS";
                setFpsCap = "Imposta Limite FPS";
                connectToLS = "Connetti a LiveSplit";
                connectedToLS = "Connesso a LiveSplit";
                notConnectedToLS = "Non connesso a LiveSplit";
                settings = "Impostazioni...";
                credits = "Sviluppato da <color=purple>Loomeh</color> e <color=#F97CE4>Ninja Cookie</color>\nTradotto da Erisrine";
                displayFPS = "Mostra FPS";
                amStartsEnabled = "AutoMash Inizia Abilitato";
                startFPSCapped = "FPS Iniziano Limitati";
                displayFpsSize = "Dimensione Display FPS";
                displayFpsX = "Display FPS x";
                displayFpsY = "Display FPS y";
                save = "Salva...";
                cancel = "Cancella";
                enabledTXT = "Abilita";
                DisabledTXT = "Disabilita";
            }

            initialised = true;
            Debug.Log("Text initalised");
            
        }

    }
}
