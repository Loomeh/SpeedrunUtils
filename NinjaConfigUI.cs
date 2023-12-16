using BepInEx;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpeedrunUtils
{
    internal class NinjaConfigUI : MonoBehaviour
    {
        private static string configFolder = $@"{Paths.ConfigPath}\SpeedrunUtils\";

        private readonly string splitsPath = Path.Combine(configFolder, "splits.txt");
        private static string settingsPath = Path.Combine(configFolder, "settings.txt");

        private LiveSplitControl lsCon = LiveSplitControl.Instance;

        public static bool setting_displayFPS = true;
        public static bool setting_automash = true;
        public static bool setting_shouldCapFPS = true;

        public static int setting_displayFPS_size = 30;
        public static int setting_displayFPS_x = 10;
        public static int setting_displayFPS_y = 5;

        public int setting_FPScap = 300;

        public KeyCode key_limitFPS     = KeyCode.L;
        public KeyCode key_uncapFPS     = KeyCode.U;
        public KeyCode key_automash     = KeyCode.T;
        public KeyCode key_toggleMenu   = KeyCode.Quote;

        private const char split_key = '=';

        private bool setupEnded = false;
        private bool singleSetup = false;
        private bool initStartup = true;

        private async void Start()
        {
            if (!Directory.Exists(configFolder))
                await Task.Run(() => { Directory.CreateDirectory(configFolder); });

            if (Directory.Exists(configFolder) && !File.Exists(settingsPath))
                await WriteSettings();

            if (Directory.Exists(configFolder) && !File.Exists(splitsPath))
            {
                await Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://raw.githubusercontent.com/Loomeh/BRCAutosplitter/main/splits.txt", splitsPath);
                    }
                });
            }

            if (File.Exists(settingsPath))
                SetSettings();

            if (initStartup)
            {
                lsCon.ConnectToLiveSplit();
                initStartup = false;
            }

            setupEnded = true;
        }

        private async Task WriteSettings()
        {
            await Task.Run(() => {
                File.WriteAllText(settingsPath,
                    "[Settings]\n" +
                    "\n" +
                    $"Display FPS {split_key} {setting_displayFPS}\n" +
                    $"AutoMash Starts Enabled {split_key} {setting_automash}\n" +
                    $"Start FPS Capped {split_key} {setting_shouldCapFPS}\n" +
                    "\n" +
                    $"FPS Display Size {split_key} {setting_displayFPS_size}\n" +
                    $"FPS Display X Pos {split_key} {setting_displayFPS_x}\n" +
                    $"FPS Display Y Pos {split_key} {setting_displayFPS_y}\n" +
                    "\n" +
                    $"FPS Cap {split_key} {setting_FPScap}\n" +
                    "\n\n" +
                    "[Keys] (https://docs.unity3d.com/ScriptReference/KeyCode.html)\n" +
                    "\n" +
                    $"Lock FPS to 30 {split_key} {key_limitFPS}\n" +
                    $"Uncap FPS {split_key} {key_uncapFPS}\n" +
                    $"Toggle AutoMash {split_key} {key_automash}\n" +
                    $"Toggle Menu {split_key} {key_toggleMenu}"
                );
            });
        }

        private void SetSettings()
        {
            string[] lines = File.ReadAllLines(settingsPath);

            List<string> tempSettings = new List<string>();
            List<string> tempKeys = new List<string>();
            int lineType = 0;

            foreach (String line in lines)
            {
                if (line.Contains("[Settings]"))
                {
                    lineType = 1;
                }
                else if (line.Contains("[Keys]"))
                {
                    lineType = 2;
                }
                // Settings
                else if (lineType == 1 && !string.IsNullOrWhiteSpace(line) && line.Contains(split_key))
                {
                    string[] parts = line.Split(split_key);
                    if (parts.Length >= 2)
                    {
                        parts[1].Replace(" ", "");
                        tempSettings.Add(parts[1]);
                    }
                }
                // Keys
                else if (lineType == 2 && !string.IsNullOrWhiteSpace(line) && line.Contains(split_key))
                {
                    string[] parts = line.Split(split_key);
                    if (parts.Length >= 2)
                    {
                        parts[1].Replace(" ", "");
                        tempKeys.Add(parts[1]);
                    }
                }
            }

            // Set Settings
            if (tempSettings.Count >= 1 && bool.TryParse(tempSettings[0], out bool settings_displayFPS))
                setting_displayFPS = settings_displayFPS;

            if (tempSettings.Count >= 2 && bool.TryParse(tempSettings[1], out bool settings_automash))
                setting_automash = settings_automash;

            if (tempSettings.Count >= 3 && bool.TryParse(tempSettings[2], out bool settings_shouldCapFPS))
                setting_shouldCapFPS = settings_shouldCapFPS;

            if (tempSettings.Count >= 4 && int.TryParse(tempSettings[3], out int settings_displayFPS_size))
                setting_displayFPS_size = settings_displayFPS_size;

            if (tempSettings.Count >= 5 && int.TryParse(tempSettings[4], out int settings_displayFPS_x))
                setting_displayFPS_x = settings_displayFPS_x;

            if (tempSettings.Count >= 6 && int.TryParse(tempSettings[5], out int settings_displayFPS_y))
                setting_displayFPS_y = settings_displayFPS_y;

            if (tempSettings.Count >= 7 && int.TryParse(tempSettings[6], out int settings_FPScap))
                setting_FPScap = settings_FPScap;

            // Set Keys
            if (tempKeys.Count >= 1 && System.Enum.TryParse(tempKeys[0], out KeyCode keycode_limitFPS))
                key_limitFPS = keycode_limitFPS;

            if (tempKeys.Count >= 2 && System.Enum.TryParse(tempKeys[1], out KeyCode keycode_uncapFPS))
                key_uncapFPS = keycode_uncapFPS;

            if (tempKeys.Count >= 3 && System.Enum.TryParse(tempKeys[2], out KeyCode keycode_automash))
                key_automash = keycode_automash;

            if (tempKeys.Count >= 4 && System.Enum.TryParse(tempKeys[3], out KeyCode keycode_toggleMenu))
                key_toggleMenu = keycode_toggleMenu;

            fpsCap = setting_FPScap.ToString();
            lastValidFPS = fpsCap;

            keystring_limitFPS      = key_limitFPS.ToString();
            keystring_uncapFPS      = key_uncapFPS.ToString();
            keystring_automash      = key_automash.ToString();
            keystring_toggleMenu    = key_toggleMenu.ToString();

            if (!singleSetup)
            {
                fpsUncapped = !setting_shouldCapFPS;
                DoAutoMash.Instance.autoMash = setting_automash;
                autoMashState = DoAutoMash.Instance.autoMash;
                singleSetup = true;
            }
        }

        private async void SaveConfigSettings()
        {
            if (System.Enum.TryParse(keystring_limitFPS,    out KeyCode key1)) { key_limitFPS   = key1; };
            if (System.Enum.TryParse(keystring_uncapFPS,    out KeyCode key2)) { key_uncapFPS   = key2; };
            if (System.Enum.TryParse(keystring_automash,    out KeyCode key3)) { key_automash   = key3; };
            if (System.Enum.TryParse(keystring_toggleMenu,  out KeyCode key4)) { key_toggleMenu = key4; };

            setting_displayFPS      = temp_setting_displayFPS;
            setting_automash        = temp_setting_automash;
            setting_shouldCapFPS    = temp_setting_shouldCapFPS;

            if (int.TryParse(string_displayFPS_size,        out int i1)) { setting_displayFPS_size  = i1; }
            if (int.TryParse(string_displayFPS_x,           out int i2)) { setting_displayFPS_x     = i2; }
            if (int.TryParse(string_displayFPS_y,           out int i3)) { setting_displayFPS_y     = i3; }

            if (File.Exists(settingsPath))
            {
                setupEnded = false;
                await Task.Run(() => { File.Delete(settingsPath); });
                Start();
            }
        }

        private void Update()
        {
            if (setupEnded)
            {
                HandleInputs();

                if (limitingFPS || !fpsUncapped)
                {
                    if (QualitySettings.vSyncCount > 0)
                        QualitySettings.vSyncCount = 0;
                }

                if (limitingFPS)
                {
                    Application.targetFrameRate = 30;
                }
                else
                {
                    if (fpsUncapped)
                        Application.targetFrameRate = -1;
                    else
                        Application.targetFrameRate = setting_FPScap;
                }
            }

            if(open)
            {
                UnityEngine.Cursor.visible = true;
            }
            else if (lsCon.BaseModule.IsPlayingInStage && !open)
            {
                UnityEngine.Cursor.visible = false;
            }

        }

        private bool limitingFPS = false;
        private bool fpsUncapped = false;
        private void HandleInputs()
        {
            if (guiID != 2)
            {
                if (UnityEngine.Input.GetKeyDown(key_limitFPS))
                    limitingFPS = !limitingFPS;

                if (UnityEngine.Input.GetKeyDown(key_uncapFPS))
                    fpsUncapped = !fpsUncapped;

                if (UnityEngine.Input.GetKeyDown(key_automash))
                    DoAutoMash.Instance.autoMash = !DoAutoMash.Instance.autoMash;

                if (UnityEngine.Input.GetKeyDown(key_toggleMenu))
                    open = !open;
            }
        }

        private GUIStyle currentStyle;
        private bool open = false;
        private int guiID = 1;

        private const int guiWidth = 300;
        private const int guiHeight = 233;

        private String fpsCap = String.Empty;
        private String lastValidFPS = String.Empty;

        private KeyCode currentKey = KeyCode.None;

        private String keystring_limitFPS           = String.Empty;
        private String keystring_uncapFPS           = String.Empty;
        private String keystring_automash           = String.Empty;
        private String keystring_toggleMenu         = String.Empty;

        private bool temp_setting_displayFPS        = setting_displayFPS;
        private bool temp_setting_automash          = setting_automash;
        private bool temp_setting_shouldCapFPS      = setting_shouldCapFPS;

        private string string_displayFPS_size       = setting_displayFPS_size.ToString();
        private string string_displayFPS_x          = setting_displayFPS_x.ToString();
        private string string_displayFPS_y          = setting_displayFPS_y.ToString();

        private bool autoMashState = true;
        private const float showAutoMashLengthMax = 2f;
        private float showAutoMashLength = showAutoMashLengthMax;
        public void OnGUI()
        {
            // Init methods to reduce lag
            CreateText(0, 0, 0, 0, 0, new Color(0, 0, 0, 0), "");
            CreateBox(0, 0, 0, 0, new Color(0, 0, 0, 0));
            CreateButton(0, 0, 0, 0, "", new Color(0, 0, 0, 0), new Color(0, 0, 0, 0), new Color(0, 0, 0, 0));

            if (DoAutoMash.Instance.autoMash)
            {
                if (!autoMashState) { showAutoMashLength = 0f; }

                if (showAutoMashLength < showAutoMashLengthMax)
                {
                    GUI.color = new Color(0, 0, 0, 0);

                    if(lsCon.lang == "EN")
                        CreateText(20, Screen.height - 40, 600, 600, 30, new Color(0.2f, 1, 0.2f, 0.5f), "AutoMash Enabled");
                    else if(lsCon.lang == "JPN")
                        CreateText(20, Screen.height - 40, 600, 600, 30, new Color(0.2f, 1, 0.2f, 0.5f), "自動マッシング有効");

                    autoMashState = true;
                    showAutoMashLength += Core.dt;
                }
            }
            else if (!DoAutoMash.Instance.autoMash)
            {
                if (autoMashState) { showAutoMashLength = 0f; }

                if (showAutoMashLength < showAutoMashLengthMax)
                {
                    GUI.color = new Color(0, 0, 0, 0);


                    if (lsCon.lang == "EN")
                        CreateText(20, Screen.height - 40, 600, 600, 30, new Color(0.2f, 1, 0.2f, 0.5f), "AutoMash Disabled");
                    else if (lsCon.lang == "JPN")
                        CreateText(20, Screen.height - 40, 600, 600, 30, new Color(1, 0.2f, 0.2f, 0.5f), "自動マッシング無効");

                    autoMashState = false;
                    showAutoMashLength += Core.dt;
                }
            }

            if (setupEnded) {
                if (open)
                {
                    if (guiID == 1)
                    {
                        CreateBox(10, 7, guiWidth, guiHeight, new Color(0, 0, 0, 0.3f));
                        int curY = 12;

                        if(lsCon.lang == "EN")
                        {
                            CreateText(15, curY, 495, 300, 16, new Color(1, 0.8f, 0.3f, 1), "Keys:");
                            curY += 21;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_limitFPS}</color> = Lock FPS ({(limitingFPS ? "<color=#9ef7a3>Enabled</color>" : "<color=#f27e7e>Disabled</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_uncapFPS}</color> = Uncap FPS ({(fpsUncapped ? "<color=#9ef7a3>Enabled</color>" : "<color=#f27e7e>Disabled</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_automash}</color> = Toggle AutoMash ({(DoAutoMash.Instance.autoMash ? "<color=#9ef7a3>Enabled</color>" : "<color=#f27e7e>Disabled</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_toggleMenu}</color> = Toggle Menu");
                            curY += 27;
                            CreateText(15, curY, 495, 300, 16, new Color(1, 0.8f, 0.3f, 1), $"FPS Cap ({setting_FPScap}):");
                            curY += 21;
                            GUI.color = new Color(1, 0.8f, 0.3f, 1);
                            fpsCap = GUI.TextArea(new Rect(15, curY, guiWidth / 3, 21), fpsCap);
                            if (!int.TryParse(fpsCap, out int validFPS) && fpsCap != String.Empty) { fpsCap = lastValidFPS; } else { if (fpsCap != String.Empty) { lastValidFPS = validFPS.ToString(); } }
                            if (CreateButton((guiWidth / 3) + 20, curY, guiWidth - (guiWidth / 3) - 15, 21, "Set FPS Cap", new Color(0.2f, 0.2f, 0.2f, 1), new Color(0.4f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                if (int.TryParse(fpsCap, out int fps))
                                {
                                    if (fps < 30)
                                        fps = 30;
                                }
                                else { fps = 30; }

                                setting_FPScap = fps;
                                fpsCap = fps.ToString();
                                SaveConfigSettings();
                                fpsUncapped = false;
                            }
                            curY += 26;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 0.8f, 0.3f, 1), $"{(lsCon.IsConnectedToLivesplit ? "<color=green>Connected to LiveSplit</color>" : "<color=red>Not connected to LiveSplit</color>")}");
                            curY += 18;
                            if (CreateButton(15, curY, guiWidth - 10, 21, "Connect to LiveSplit", lsCon.IsConnectedToLivesplit ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), lsCon.IsConnectedToLivesplit ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                if (!lsCon.IsConnectedToLivesplit)
                                    lsCon.ConnectToLiveSplit();
                            }
                            curY += 26;
                            if (CreateButton(15, curY, guiWidth - 10, 21, "Settings...", new Color(0.2f, 0.2f, 0.2f, 1), new Color(0.4f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                guiID = 2;

                                temp_setting_displayFPS = setting_displayFPS;
                                temp_setting_automash = setting_automash;
                                temp_setting_shouldCapFPS = setting_shouldCapFPS;

                                string_displayFPS_size = setting_displayFPS_size.ToString();
                                string_displayFPS_x = setting_displayFPS_x.ToString();
                                string_displayFPS_y = setting_displayFPS_y.ToString();
                            }
                            curY += 26;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), "Developed by <color=purple>Loomeh</color> and <color=#F97CE4>Ninja Cookie</color>");
                        }
                        else if(lsCon.lang == "JPN")
                        {
                            CreateText(15, curY, 495, 300, 16, new Color(1, 0.8f, 0.3f, 1), "キー：");
                            curY += 21;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_limitFPS}</color> = FPSを制限する ({(limitingFPS ? "<color=#9ef7a3>有効。</color>" : "<color=#f27e7e>無効。</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_uncapFPS}</color> = FPSの制限を解除する ({(fpsUncapped ? "<color=#9ef7a3>有効。</color>" : "<color=#f27e7e>無効。</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_automash}</color> = 自動マッシングの切り替え ({(DoAutoMash.Instance.autoMash ? "<color=#9ef7a3>有効。</color>" : "<color=#f27e7e>無効。</color>")})");
                            curY += 12;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), $"<color=#64b1d9>{key_toggleMenu}</color> = メニューの切り替え");
                            curY += 27;
                            CreateText(15, curY, 495, 300, 16, new Color(1, 0.8f, 0.3f, 1), $"FPS制限 ({setting_FPScap}):");
                            curY += 21;
                            GUI.color = new Color(1, 0.8f, 0.3f, 1);
                            fpsCap = GUI.TextArea(new Rect(15, curY, guiWidth / 3, 21), fpsCap);
                            if (!int.TryParse(fpsCap, out int validFPS) && fpsCap != String.Empty) { fpsCap = lastValidFPS; } else { if (fpsCap != String.Empty) { lastValidFPS = validFPS.ToString(); } }
                            if (CreateButton((guiWidth / 3) + 20, curY, guiWidth - (guiWidth / 3) - 15, 21, "FPS制限設定", new Color(0.2f, 0.2f, 0.2f, 1), new Color(0.4f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                if (int.TryParse(fpsCap, out int fps))
                                {
                                    if (fps < 30)
                                        fps = 30;
                                }
                                else { fps = 30; }

                                setting_FPScap = fps;
                                fpsCap = fps.ToString();
                                SaveConfigSettings();
                                fpsUncapped = false;
                            }
                            curY += 26;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 0.8f, 0.3f, 1), $"{(lsCon.IsConnectedToLivesplit ? "<color=green>LiveSplitに接続済み</color>" : "<color=red>LiveSplitに未接続</color>")}");
                            curY += 18;
                            if (CreateButton(15, curY, guiWidth - 10, 21, "LiveSplitに接続", lsCon.IsConnectedToLivesplit ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), lsCon.IsConnectedToLivesplit ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                if (!lsCon.IsConnectedToLivesplit)
                                    lsCon.ConnectToLiveSplit();
                            }
                            curY += 26;
                            if (CreateButton(15, curY, guiWidth - 10, 21, "設定...", new Color(0.2f, 0.2f, 0.2f, 1), new Color(0.4f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                guiID = 2;

                                temp_setting_displayFPS = setting_displayFPS;
                                temp_setting_automash = setting_automash;
                                temp_setting_shouldCapFPS = setting_shouldCapFPS;

                                string_displayFPS_size = setting_displayFPS_size.ToString();
                                string_displayFPS_x = setting_displayFPS_x.ToString();
                                string_displayFPS_y = setting_displayFPS_y.ToString();
                            }
                            curY += 26;
                            CreateText(15, curY, 495, 300, 12, new Color(1, 1, 1, 1), "<color=purple>Loomeh</color>と<color=#F97CE4>Ninja Cookie</color>によって開発");
                        }
                    }
                    if(lsCon.lang == "EN")
                    {
                        if (guiID == 2)
                        {
                            CreateBox(10, 7, guiWidth, 290, new Color(0, 0, 0, 0.3f));
                            int curY = 12;

                            GUI.SetNextControlName("nofocus");
                            GUI.TextArea(new Rect(-5, -5, 0, 0), "");

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_limitFPS");
                            keystring_limitFPS = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_limitFPS);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Lock FPS");

                            if (GUI.GetNameOfFocusedControl() == "keystring_limitFPS")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_limitFPS, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_limitFPS = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_uncapFPS");
                            keystring_uncapFPS = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_uncapFPS);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Uncap FPS");

                            if (GUI.GetNameOfFocusedControl() == "keystring_uncapFPS")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_uncapFPS, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_uncapFPS = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_automash");
                            keystring_automash = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_automash);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Toggle AutoMash");

                            if (GUI.GetNameOfFocusedControl() == "keystring_automash")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_automash, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_automash = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_toggleMenu");
                            keystring_toggleMenu = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_toggleMenu);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Toggle Menu");

                            if (GUI.GetNameOfFocusedControl() == "keystring_toggleMenu")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_toggleMenu, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_toggleMenu = currentKey.ToString();
                            }

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_displayFPS}", temp_setting_displayFPS ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_displayFPS ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_displayFPS = !temp_setting_displayFPS;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Display FPS");

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_automash}", temp_setting_automash ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_automash ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_automash = !temp_setting_automash;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = AutoMash Starts Enabled");

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_shouldCapFPS}", temp_setting_shouldCapFPS ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_shouldCapFPS ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_shouldCapFPS = !temp_setting_shouldCapFPS;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Start FPS Capped");

                            curY += 26;
                            string_displayFPS_size = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_size);
                            if (!int.TryParse(string_displayFPS_size, out int FPSsize)) { if (string_displayFPS_size != String.Empty) { string_displayFPS_size = setting_displayFPS_size.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Display FPS Size");

                            curY += 26;
                            string_displayFPS_x = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_x);
                            if (!int.TryParse(string_displayFPS_x, out int FPSx)) { if (string_displayFPS_x != String.Empty) { string_displayFPS_x = setting_displayFPS_x.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Display FPS x");

                            curY += 26;
                            string_displayFPS_y = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_y);
                            if (!int.TryParse(string_displayFPS_y, out int FPSy)) { if (string_displayFPS_y != String.Empty) { string_displayFPS_y = setting_displayFPS_y.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = Display FPS y");

                            curY += 26;
                            if (CreateButton(15, curY, (guiWidth / 2) - 7, 21, "Save...", new Color(0.2f, 0.3f, 0.2f, 1), new Color(0.4f, 0.5f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                SaveConfigSettings();
                                guiID = 1;
                            }
                            if (CreateButton((guiWidth / 2) + 12, curY, (guiWidth / 2) - 7, 21, "Cancel", new Color(0.3f, 0.2f, 0.2f, 1), new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                guiID = 1;
                            }
                        }
                    }
                    else if(lsCon.lang == "JPN")
                    {
                        if (guiID == 2)
                        {
                            CreateBox(10, 7, guiWidth, 290, new Color(0, 0, 0, 0.3f));
                            int curY = 12;

                            GUI.SetNextControlName("nofocus");
                            GUI.TextArea(new Rect(-5, -5, 0, 0), "");

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_limitFPS");
                            keystring_limitFPS = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_limitFPS);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSを制限する");

                            if (GUI.GetNameOfFocusedControl() == "keystring_limitFPS")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_limitFPS, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_limitFPS = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_uncapFPS");
                            keystring_uncapFPS = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_uncapFPS);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSの制限を解除する");

                            if (GUI.GetNameOfFocusedControl() == "keystring_uncapFPS")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_uncapFPS, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_uncapFPS = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_automash");
                            keystring_automash = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_automash);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = 自動マッシングの切り替え");

                            if (GUI.GetNameOfFocusedControl() == "keystring_automash")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_automash, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_automash = currentKey.ToString();
                            }

                            curY += 26;

                            GUI.color = Color.cyan;
                            GUI.SetNextControlName("keystring_toggleMenu");
                            keystring_toggleMenu = GUI.TextArea(new Rect(15, curY, 80, 21), keystring_toggleMenu);
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = メニューの切り替え");

                            if (GUI.GetNameOfFocusedControl() == "keystring_toggleMenu")
                            {
                                if (Event.current.keyCode != KeyCode.None)
                                {
                                    currentKey = Event.current.keyCode;
                                    GUI.FocusControl("nofocus");
                                }
                                else
                                {
                                    System.Enum.TryParse(keystring_toggleMenu, out currentKey);
                                }

                                if (currentKey != KeyCode.None)
                                    keystring_toggleMenu = currentKey.ToString();
                            }

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_displayFPS}", temp_setting_displayFPS ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_displayFPS ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_displayFPS = !temp_setting_displayFPS;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSを表示");

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_automash}", temp_setting_automash ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_automash ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_automash = !temp_setting_automash;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = 自動マッシングは有効で開始します");

                            curY += 26;
                            if (CreateButton(15, curY, 80, 21, $"{temp_setting_shouldCapFPS}", temp_setting_shouldCapFPS ? new Color(0.2f, 0.3f, 0.2f, 1) : new Color(0.3f, 0.2f, 0.2f, 1), temp_setting_shouldCapFPS ? new Color(0.4f, 0.5f, 0.4f, 1) : new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                temp_setting_shouldCapFPS = !temp_setting_shouldCapFPS;
                            }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSは制限されて開始します");

                            curY += 26;
                            string_displayFPS_size = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_size);
                            if (!int.TryParse(string_displayFPS_size, out int FPSsize)) { if (string_displayFPS_size != String.Empty) { string_displayFPS_size = setting_displayFPS_size.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSサイズを表示");

                            curY += 26;
                            string_displayFPS_x = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_x);
                            if (!int.TryParse(string_displayFPS_x, out int FPSx)) { if (string_displayFPS_x != String.Empty) { string_displayFPS_x = setting_displayFPS_x.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSを x で表示");

                            curY += 26;
                            string_displayFPS_y = GUI.TextArea(new Rect(15, curY, 80, 21), string_displayFPS_y);
                            if (!int.TryParse(string_displayFPS_y, out int FPSy)) { if (string_displayFPS_y != String.Empty) { string_displayFPS_y = setting_displayFPS_y.ToString(); } }
                            CreateText(95, curY, 495, 300, 12, new Color(1, 1, 1, 1), " = FPSを y で表示");

                            curY += 26;
                            if (CreateButton(15, curY, (guiWidth / 2) - 7, 21, "保存...", new Color(0.2f, 0.3f, 0.2f, 1), new Color(0.4f, 0.5f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                SaveConfigSettings();
                                guiID = 1;
                            }
                            if (CreateButton((guiWidth / 2) + 12, curY, (guiWidth / 2) - 7, 21, "キャンセル", new Color(0.3f, 0.2f, 0.2f, 1), new Color(0.5f, 0.4f, 0.4f, 1), new Color(1, 1, 1, 1)))
                            {
                                guiID = 1;
                            }
                        }
                    }
                }
                else if (guiID != 1)
                {
                    guiID = 1;
                }
            }

            if (setting_displayFPS || (guiID == 2 && temp_setting_displayFPS))
            {
                GUI.color = new Color(0, 0, 0, 0);

                if (guiID != 2)
                {
                    int displayFPSx = setting_displayFPS_x;
                    if (open && setting_displayFPS_x < guiWidth && setting_displayFPS_y < guiHeight) { displayFPSx = guiWidth + 15; }
                    CreateText(displayFPSx, setting_displayFPS_y, 600, 600, setting_displayFPS_size, new Color(1, 1, 1, 0.5f), $"{1 / Time.deltaTime:F0}");
                }
                else if (guiID == 2 && temp_setting_displayFPS)
                {
                    int size = setting_displayFPS_size;
                    int x = setting_displayFPS_x;
                    int y = setting_displayFPS_y;
                    int.TryParse(string_displayFPS_size, out size);
                    int.TryParse(string_displayFPS_x, out x);
                    int.TryParse(string_displayFPS_y, out y);

                    CreateText(x, y, 600, 600, size, new Color(1, 1, 1, 0.5f), $"{1 / Time.deltaTime:F0}");
                }
            }
        }

        void CreateText(int x, int y, int width, int height, int size, Color color, String text, bool backdrop = true)
        {
            currentStyle = new GUIStyle(GUI.skin.label);
            currentStyle.fontSize = size;
            currentStyle.fontStyle = FontStyle.Bold;
            currentStyle.alignment = TextAnchor.UpperLeft;

            if (backdrop)
            {
                String backdropText = text;

                if (backdropText.Contains('<') && backdropText.Contains('>'))
                    backdropText = Regex.Replace(backdropText, @"<([^>]+)>", "").Replace("<", "").Replace(">", "");

                GUI.color = new Color(0, 0, 0, color.a);
                GUI.Label(new Rect(x + 1, y + 1, width, height), backdropText, currentStyle);
            }

            GUI.color = color;
            GUI.Label(new Rect(x, y, width, height), text, currentStyle);
        }

        void CreateBox(int x, int y, int width, int height, Color color)
        {
            currentStyle = new GUIStyle(GUI.skin.box);
            currentStyle.normal.background = new Texture2D(1, 1);
            currentStyle.normal.background.SetPixel(0, 0, color);
            currentStyle.normal.background.Apply();
            GUI.Box(new Rect(x, y, width, height), "", currentStyle);
        }

        bool CreateButton(int x, int y, int width, int height, String text, Color backgroundColor, Color hoverColor, Color textColor)
        {
            GUI.color = new Color(1, 1, 1, 1);

            currentStyle = new GUIStyle(GUI.skin.button);

            currentStyle.normal.background = new Texture2D(1, 1);
            currentStyle.normal.background.SetPixel(0, 0, backgroundColor);
            currentStyle.normal.background.Apply();

            currentStyle.hover.background = new Texture2D(1, 1);
            currentStyle.hover.background.SetPixel(0, 0, hoverColor);
            currentStyle.hover.background.Apply();

            currentStyle.normal.textColor = textColor;

            return GUI.Button(new Rect(x, y, width, height), text, currentStyle);
        }
    }
}
