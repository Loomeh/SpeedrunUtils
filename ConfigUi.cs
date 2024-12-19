using BepInEx;
using Reptile;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements.Collections;

namespace SpeedrunUtils
{
    public class ConfigUi : MonoBehaviour
    {
        private LiveSplitControl lsCon;

        public bool open = false;

        private Rect winRect = new(20, 20, 300, 245);
        private Rect debugRect = new(20, 40, 275, 175);

        public string fpsCapStr = "";
        public int fpsCapInt = -1;
        public bool limiting = false;
        public bool uncapped = false;
        public KeyCode limitKey = KeyCode.O;
        public KeyCode uncapKey = KeyCode.P;
        public KeyCode autoMashKey = KeyCode.T;
        public KeyCode openKey = KeyCode.Insert;

        private GUIStyle currentStyle;
        private TextManager textManager;

        public static string configFolder = Paths.ConfigPath + @"\SpeedrunUtils\";
        private string filePath = Path.Combine(configFolder, "FPS.txt");
        private string keyConfigPath = Path.Combine(configFolder, "Keys.txt");
        private readonly string SplitsPath = Path.Combine(configFolder, "splits.txt");
        private readonly string fpsDisplayPath = Path.Combine(configFolder, "Settings.txt");

        private bool shouldFPSDisplay = true;
        private int fpsSize = 30;
        private int fpsXPos = 10;
        private int fpsYPos = 5;

        private bool uncappedFPSinLoading;
        private bool uncapKeyCheck = false;

        private float accum = 0.0f;
        private int frames = 0;
        private float timeleft;
        private float updateInterval = 0.5f;
        private int fps;

        private bool autoMashState = true;
        private const float showAutoMashLengthMax = 2f;
        private float showAutoMashLength = showAutoMashLengthMax;

        private Rect splitsWinRect = new Rect(320, 20, 200, 450);
        private bool splitsOpen = false;

        private Rect creditsWinRect = new Rect(320, 20, 400, 220);
        private bool creditsOpen = false;

        private bool[] tempSplitsArray;

        public void configureKeys()
        {
            SettingsManager.CheckAndAddSetting(keyConfigPath, "Open Menu", "Insert");
            SettingsManager.CheckAndAddSetting(keyConfigPath, "Limit framerate", "O");
            SettingsManager.CheckAndAddSetting(keyConfigPath, "Uncap framerate", "P");
            SettingsManager.CheckAndAddSetting(keyConfigPath, "AutoMash Toggle", "T");

            openKey = (KeyCode)Enum.Parse(typeof(KeyCode), SettingsManager.GetSetting(keyConfigPath, "Open Menu", "Insert"), true);
            limitKey = (KeyCode)Enum.Parse(typeof(KeyCode), SettingsManager.GetSetting(keyConfigPath, "Limit framerate", "O"), true);
            uncapKey = (KeyCode)Enum.Parse(typeof(KeyCode), SettingsManager.GetSetting(keyConfigPath, "Uncap framerate", "P"), true);
            autoMashKey = (KeyCode)Enum.Parse(typeof(KeyCode), SettingsManager.GetSetting(keyConfigPath, "AutoMash Toggle", "T"), true);
        }

        public void configFPSDisplay() 
        {
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "Display FPS", "true");
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "FPS Size", "30");
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "FPS X Pos", "10");
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "FPS Y Pos", "5");
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "AutoMash Starts Enabled", "true");
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "Unlock FPS in loading screens", "false");

            // Read settings
            shouldFPSDisplay = bool.Parse(SettingsManager.GetSetting(fpsDisplayPath, "Display FPS", "true"));
            fpsSize = int.Parse(SettingsManager.GetSetting(fpsDisplayPath, "FPS Size", "30"));
            fpsXPos = int.Parse(SettingsManager.GetSetting(fpsDisplayPath, "FPS X Pos", "10"));
            fpsYPos = int.Parse(SettingsManager.GetSetting(fpsDisplayPath, "FPS Y Pos", "5"));
            DoAutoMash.Instance.autoMash = bool.Parse(SettingsManager.GetSetting(fpsDisplayPath, "AutoMash Starts Enabled", "true"));
            autoMashState = DoAutoMash.Instance.autoMash;
            uncappedFPSinLoading = bool.Parse(SettingsManager.GetSetting(fpsDisplayPath, "Unlock FPS in loading screens", "false"));
        }

        public void Start()
        {
            currentStyle = new GUIStyle();

            if (!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }

            SettingsManager.CheckAndAddSetting(filePath, "FPSCap", Screen.currentResolution.refreshRate.ToString());
            ReadFPSFile();

            if (!File.Exists(SplitsPath))
            {
                string splitsText = "-- Any%\n";
                File.WriteAllText(SplitsPath, splitsText);

                SettingsManager.CheckAndAddSetting(SplitsPath, "Prologue End", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Early Mataan (Splits when you enter Millenium Square)", "false");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Versum Hill Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Dream Sequence 1 Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Chapter 1 End", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Brink Terminal Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Dream Sequence 2 Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Chapter 2 End", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Millenium Mall Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Dream Sequence 3 Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Chapter 3 End", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Flesh Prince Versum End", "false");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Flesh Prince Millenium End", "false");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Flesh Prince Brink End", "false");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Pyramid Island Start", "false");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Dream Sequence 4 Start", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Chapter 4 End", "true");
                SettingsManager.CheckAndAddSetting(SplitsPath, "Final Boss Defeated", "true");
            }


            // Write mouse menu fix on game launch as the harmony patch writes the setting too late which could cause confusion.
            SettingsManager.CheckAndAddSetting(fpsDisplayPath, "Mouse Menu Fix", "true");

            configureKeys();
            configFPSDisplay();

            lsCon = FindObjectOfType<LiveSplitControl>();
            textManager = FindObjectOfType<TextManager>();

            if (lsCon != null)
            {
                lsCon.ConnectToLiveSplit();
            }
            else
            {
                Debug.LogError("LiveSplitControl component not found.");
            }

            if (textManager == null)
            {
                Debug.LogError("TextManager component not found.");
            }
        }

        private void ReadFPSFile()
        {
            fpsCapStr = SettingsManager.GetSetting(filePath, "FPSCap", "300");
            if (int.TryParse(fpsCapStr, out int i) && i >= 30)
            {
                fpsCapInt = i;
            }
            else
            {
                fpsCapInt = 300;
                fpsCapStr = "300";
                SettingsManager.CheckAndAddSetting(filePath, "FPSCap", "300");
            }
        }

        private void handleFPSCounter()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if(timeleft <= 0.0f)
            {
                fps = Mathf.RoundToInt(accum / frames);
                timeleft = updateInterval;
                accum = 0.0f;
                frames = 0;
            }
        }

        public void OnGUI()
        {
            if (open)
            {
                GUI.color = new Color(0, 0, 0, 1);
                winRect = GUI.Window(0, winRect, WinProc, $"{PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION}");
            }

            if (splitsOpen && open)
            {
                GUI.color = new Color(0, 0, 0, 1);
                splitsWinRect = GUI.Window(1, splitsWinRect, SplitsWinProc, textManager.guiText.Get("Splits"));
            }

            if (creditsOpen && open)
            {
                GUI.color = new Color(0, 0, 0, 1);
                creditsWinRect = GUI.Window(2, creditsWinRect, CreditsWinProc, textManager.guiText.Get("Credits"));
            }

            if (shouldFPSDisplay)
            {
                GUI.color = new Color(0, 0, 0, 0);
                CreateText(fpsXPos, fpsYPos, 600, 600, new Color(1, 1, 1, 0.5f), $"{fps}");
            }

            if (DoAutoMash.Instance.autoMash)
            {
                if (!autoMashState) { showAutoMashLength = 0f; }

                if (showAutoMashLength < showAutoMashLengthMax)
                {
                    GUI.color = new Color(0, 0, 0, 0);
                    CreateText(20, Screen.height - 40, 600, 600, new Color(0.2f, 1, 0.2f, 0.5f), textManager.guiText.Get("AutoMash Enabled"));

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
                    CreateText(20, Screen.height - 40, 600, 600, new Color(1, 0.2f, 0.2f, 0.5f), textManager.guiText.Get("AutoMash Disabled"));

                    autoMashState = false;
                    showAutoMashLength += Core.dt;
                }
            }

            if (lsCon.debug)
            {
                GUI.color = new Color(0, 0, 0, 1);
                debugRect = GUI.Window(1, debugRect, DebugWinProc, "SpeedrunUtils Debug Mode");
            }
        }

        private void WinProc(int id)
        {
            var ox = 15f;
            var oy = 18f;
            
            var mx = winRect.width - 30;

            GUI.Label(new(ox, oy, mx, 20), $"FPS: {fps}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"{textManager.guiText.Get("FPS Limit")} ({limitKey}): {(limiting ? $"<color=green>{textManager.guiText.Get("On")}</color>" : $"<color=red>{textManager.guiText.Get("Off")}</color>")}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"{textManager.guiText.Get("Uncapped")} ({uncapKey}): {(uncapped ? $"<color=green>{textManager.guiText.Get("On")}</color>" : $"<color=red>{textManager.guiText.Get("Off")}</color>")}");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), $"{(lsCon.IsConnectedToLivesplit ? $"<color=green>{textManager.guiText.Get("Connected to LiveSplit")}</color>" : $"<color=red>{textManager.guiText.Get("Not connected to LiveSplit")}</color>")}");
            oy += 10 + 10;


            fpsCapStr = GUI.TextField(new Rect(ox, oy, mx, 20), fpsCapStr, 4);
            
            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Set FPS")))
            {
                if (Int32.TryParse(fpsCapStr, out int i) && Int32.Parse(fpsCapStr) >= 30)
                {
                    fpsCapInt = i;
                }
                else
                {
                    ReadFPSFile();
                }

                if (File.Exists(filePath))
                    File.Delete(filePath);

                using (FileStream fs = File.Create(filePath))
                {
                    byte[] buf = new UTF8Encoding(true).GetBytes(fpsCapStr);
                    fs.Write(buf, 0, fpsCapStr.Length);
                }
            }

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Connect to LiveSplit")))
            {
                lsCon.ConnectToLiveSplit();
            }

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Splits")))
            {
                tempSplitsArray = lsCon.SplitArray;
                splitsOpen = !splitsOpen;
            }

            oy += 10 + 15;

            GUI.Label(new(ox, oy, mx, 20), textManager.guiText.Get("Report any issues in #technical-help"));
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), $"{textManager.guiText.Get("Developed by")} <color=purple>Loomeh</color>");
            oy += 10 + 15;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Credits")))
            {
                creditsOpen = !creditsOpen;
            }

            oy += 10 + 5;

            GUI.DragWindow();
        }

        void CreateText(int x, int y, int width, int height, Color color, String text, bool backdrop = true)
        {
            currentStyle = new GUIStyle(GUI.skin.label);
            currentStyle.fontSize = fpsSize;
            currentStyle.fontStyle = FontStyle.Bold;
            currentStyle.alignment = TextAnchor.UpperLeft;

            if (backdrop)
            {
                GUI.color = new Color(0, 0, 0, color.a);
                GUI.Label(new Rect(x + 2, y + 2, width, height), text, currentStyle);
            }

            GUI.color = color;
            GUI.Label(new Rect(x, y, width, height), text, currentStyle);
        }

        private void SplitsWinProc(int id)
        {
            var ox = 15f;
            var oy = 0f;

            var mx = splitsWinRect.width - 30;

            tempSplitsArray[0] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[0], "Prologue");
            oy += 20;
            tempSplitsArray[1] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[1], "Early Mataan");
            oy += 20;
            tempSplitsArray[2] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[2], "Versum Hill Start");
            oy += 20;
            tempSplitsArray[3] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[3], "Dream Sequence 1 Start");
            oy += 20;
            tempSplitsArray[4] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[4], "Chapter 1 End");
            oy += 20;
            tempSplitsArray[5] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[5], "Brink Terminal Start");
            oy += 20;
            tempSplitsArray[6] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[6], "Dream Sequence 2 Start");
            oy += 20;
            tempSplitsArray[7] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[7], "Chapter 2 End");
            oy += 20;
            tempSplitsArray[8] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[8], "Millenium Mall Start");
            oy += 20;
            tempSplitsArray[9] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[9], "Dream Sequence 3 Start");
            oy += 20;
            tempSplitsArray[10] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[10], "Chapter 3 End");
            oy += 20;
            tempSplitsArray[11] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[11], "Flesh Prince Versum End");
            oy += 20;
            tempSplitsArray[12] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[12], "Flesh Prince Millenium End");
            oy += 20;
            tempSplitsArray[13] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[13], "Flesh Prince Brink End");
            oy += 20;
            tempSplitsArray[14] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[14], "Pyramid Island Start");
            oy += 20;
            tempSplitsArray[15] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[15], "Dream Sequence 4 Start");
            oy += 20;
            tempSplitsArray[16] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[16], "Chapter 4 End");
            oy += 20;
            tempSplitsArray[17] = GUI.Toggle(new Rect(15, oy + 26, 200, 21), tempSplitsArray[17], "Final Boss Defeated");

            oy += 10 + 40;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Save Splits")))
            {
                lsCon.ReplaceBoolArrayInFile(SplitsPath, tempSplitsArray);
            }

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), textManager.guiText.Get("Close")))
            {
                splitsOpen = false;
            }

            GUI.DragWindow();
        }

        private void CreditsWinProc(int id)
        {
            var ox = 15f;
            var oy = 15f;

            // Calculate maximum width, ensuring fit for the longest text entry
            var longestText = "Ninja Cookie - Research, Code Contributions, Automash";
            var mx = Mathf.Max(splitsWinRect.width - 30, GUI.skin.textArea.CalcSize(new GUIContent(longestText)).x + 30);

            GUI.Label(new(ox, oy, mx, 20), longestText);
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Jomoko - Research, Code Contributions");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Erisrine - Italian translation");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "chermont - Portuguese translation");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Judah Caruso - JudahsSpeedUtils");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Storied - Beta testing");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "ItzBytez - Beta testing");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "ness - Bug reports");
            oy += 10 + 10;

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Close"))
            {
                creditsOpen = false;
            }

            GUI.DragWindow();
        }


        private void DebugWinProc(int id)
        {
            var dOX = 15f;
            var dOY = 30f;
            var dMX = debugRect.width - 30;

            // Debug labels
            GUI.Label(new(dOX, dOY, dMX, 20), $"Current stage: {lsCon.currentStage}");
            dOY += 10 + 5;

            GUI.Label(new(dOX, dOY, dMX, 20), $"Previous stage: {lsCon.prevStage}");
            dOY += 10 + 5;

            GUI.Label(new(dOX, dOY, dMX, 20), $"Current objective: {lsCon.objective}");
            dOY += 10 + 5;

            GUI.Label(new(dOX, dOY, dMX, 20), $"Previous objective: {lsCon.prevObjective}");
            dOY += 10 + 5;

            GUI.Label(new(dOX, dOY, dMX, 20), $"Cutscene ID: {lsCon.sequenceName}");
            dOY += 10 + 5;

            GUI.Label(new(dOX, dOY, dMX, 20), $"Loading: {lsCon.IsLoading}");
            dOY += 10 + 5;

            GUI.DragWindow();
        }

        
        public void Update()
        {
            handleFPSCounter();

            if (uncappedFPSinLoading)
            {
                if (lsCon.IsLoading)
                    uncapped = true;
                else if (!lsCon.IsLoading && !uncapKeyCheck)
                    uncapped = false;
            }

            if (Input.GetKeyDown(openKey))
                open = !open;

            if (Input.GetKeyDown(limitKey))
            {
                limiting = !limiting;
                uncapKeyCheck = false;

                if (limiting)
                    uncapped = false; // If limiting is activated, set uncapped to false
            }

            if (Input.GetKeyDown(uncapKey))
            {
                uncapKeyCheck = !uncapKeyCheck;
                uncapped = !uncapped;

                if (uncapped)
                    limiting = false; // If uncapped is activated, set limiting to false
            }

            if (Input.GetKeyDown(autoMashKey))
            {
                DoAutoMash.Instance.autoMash = !DoAutoMash.Instance.autoMash;
            }

            if (QualitySettings.vSyncCount > 0)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (limiting)
            {
                Application.targetFrameRate = 30;
            }
            else
            {
                Application.targetFrameRate = uncapped ? -1 : fpsCapInt;
            }

        }
    }
}
