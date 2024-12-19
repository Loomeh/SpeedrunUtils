using BepInEx;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpeedrunUtils
{
    public class TextManager : MonoBehaviour
    {
        public static string langPath = Path.Combine(Paths.ConfigPath, "SpeedrunUtils", "LANG.txt");
        public string lang = "EN"; // Assume the user is an English speaker by default.

        public Dictionary<string, string> guiText = new Dictionary<string, string>(16);

        private void SetLanguage()
        {
            switch (lang)
            {
                case ("EN"):
                    guiText.Add("Splits", "Splits");
                    guiText.Add("Credits", "Credits");
                    guiText.Add("Automash Enabled", "Automash Enabled");
                    guiText.Add("Automash Disabled", "Automash Disabled");
                    guiText.Add("FPS", "FPS");
                    guiText.Add("FPS Limit", "FPS Limit");
                    guiText.Add("On", "On");
                    guiText.Add("Off", "Off");
                    guiText.Add("Uncapped", "Uncapped");
                    guiText.Add("Connected to LiveSplit", "Connected to LiveSplit");
                    guiText.Add("Not connected to LiveSplit", "Not connected to LiveSplit");
                    guiText.Add("Set FPS", "Set FPS");
                    guiText.Add("Connect to LiveSplit", "Connect to LiveSplit");
                    guiText.Add("Report any issues in #technical-help", "Report any issues in #technical-help");
                    guiText.Add("Developed by", "Developed by");
                    guiText.Add("Save Splits", "Save Splits");
                    guiText.Add("Close", "Close");
                    break;

                case ("IT"):
                    guiText.Add("Splits", "Parziali");
                    guiText.Add("Credits", "Crediti");
                    guiText.Add("Automash Enabled", "AutoMash Abilitato");
                    guiText.Add("Automash Disabled", "AutoMash Disabilitato");
                    guiText.Add("FPS", "FPS");
                    guiText.Add("FPS Limit", "Limite FPS");
                    guiText.Add("On", "Su");
                    guiText.Add("Off", "Spento");
                    guiText.Add("Uncapped", "Senza limiti");
                    guiText.Add("Connected to LiveSplit", "Connesso a LiveSplit");
                    guiText.Add("Not connected to LiveSplit", "Non connesso a LiveSplit");
                    guiText.Add("Set FPS", "Imposta Limite FPS");
                    guiText.Add("Connect to LiveSplit", "Connetti a LiveSplit");
                    guiText.Add("Report any issues in #technical-help", "Segnala eventuali problemi in #technical-help");
                    guiText.Add("Developed by", "Sviluppato da");
                    guiText.Add("Save Splits", "Salva i parziali");
                    guiText.Add("Close", "Chiudi");
                    break;

                case ("PT"):
                    guiText.Add("Splits", "Splits");
                    guiText.Add("Credits", "Créditos");
                    guiText.Add("Automash Enabled", "Automash Ativado");
                    guiText.Add("Automash Disabled", "Automash Desativado");
                    guiText.Add("FPS", "FPS");
                    guiText.Add("FPS Limit", "Limite de FPS");
                    guiText.Add("On", "Ligado");
                    guiText.Add("Off", "Desligado");
                    guiText.Add("Uncapped", "Ilimitado");
                    guiText.Add("Connected to LiveSplit", "Conectado ao LiveSplit");
                    guiText.Add("Not connected to LiveSplit", "Não conectado ao LiveSplit");
                    guiText.Add("Set FPS", "Definir FPS");
                    guiText.Add("Connect to LiveSplit", "Conectar ao LiveSplit");
                    guiText.Add("Report any issues in #technical-help", "Informe qualquer problema em #technical-help");
                    guiText.Add("Developed by", "Desenvolvido por");
                    guiText.Add("Save Splits", "Salvar Splits");
                    guiText.Add("Close", "Fechar");
                    break;

                default:
                    guiText.Add("Splits", "Splits");
                    guiText.Add("Credits", "Credits");
                    guiText.Add("Automash Enabled", "Automash Enabled");
                    guiText.Add("Automash Disabled", "Automash Disabled");
                    guiText.Add("FPS", "FPS");
                    guiText.Add("FPS Limit", "FPS Limit");
                    guiText.Add("On", "On");
                    guiText.Add("Off", "Off");
                    guiText.Add("Connected to LiveSplit", "Connected to LiveSplit");
                    guiText.Add("Not connected to LiveSplit", "Not connected to LiveSplit");
                    guiText.Add("Set FPS", "Set FPS");
                    guiText.Add("Connect to LiveSplit", "Connect to LiveSplit");
                    guiText.Add("Report any issues in #technical-help", "Report any issues in #technical-help");
                    guiText.Add("Developed by", "Developed by");
                    guiText.Add("Save Splits", "Save Splits");
                    guiText.Add("Close", "Close");
                    break;
            }
        }

        private void Awake()
        {
            // Check if the LANG.txt file exists. If it doesn't, determine the user's system language, write it into the file and update lang variable.
            if(!File.Exists(langPath))
            {
                switch(Application.systemLanguage)
                {
                    case SystemLanguage.English:
                        SettingsManager.CheckAndAddSetting(langPath, "LANG", "EN");
                        lang = "EN";
                        break;

                    case SystemLanguage.Italian:
                        SettingsManager.CheckAndAddSetting(langPath, "LANG", "IT");
                        lang = "IT";
                        break;

                    case SystemLanguage.Portuguese:
                        SettingsManager.CheckAndAddSetting(langPath, "LANG", "PT");
                        lang = "PT";
                        break;

                    // Use English as default as the user's language is not supported yet.
                    default:
                        SettingsManager.CheckAndAddSetting(langPath, "LANG", "EN");
                        lang = "EN";
                        break;
                }

                SetLanguage();
            }
            else
            {
                lang = SettingsManager.GetSetting(langPath, "LANG", "EN");
                SetLanguage();
            }
        }
    }
}
