using BepInEx;
using Reptile;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunUtils
{
    public class ConfigUi : MonoBehaviour
    {
        private LiveSplitControl lsCon;

        public bool open = false;

        private Rect winRect = new(20, 20, 275, 245);
        private Rect debugRect = new(20, 40, 275, 175);

        public string fpsCapStr = "";
        public int fpsCapInt = -1;
        public bool limiting = false;
        public bool uncapped = false;
        public KeyCode limitKey = KeyCode.O;
        public KeyCode uncapKey = KeyCode.P;
        public KeyCode autoMashKey = KeyCode.T;

        private GUIStyle currentStyle;

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

        private Rect creditsWinRect = new Rect(320, 20, 450, 200);
        private bool creditsOpen = false;

        private bool[] tempSplitsArray;

        public void configureKeys()
        {
            string[] lines = File.ReadAllLines(keyConfigPath);

            List<string> tempList = new List<string>();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains(','))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        tempList.Add(parts[1]);
                    }
                }
            }

            if (System.Enum.TryParse(tempList[0], out KeyCode keycode_limit))
            {
                limitKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), tempList[0], true);
            }

            if (System.Enum.TryParse(tempList[1], out KeyCode keycode_uncap))
            {
                uncapKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), tempList[1], true);
            }

            if (System.Enum.TryParse(tempList[2], out KeyCode keycode_automash))
            {
                autoMashKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), tempList[2], true);
            }
        }

        public void configFPSDisplay() 
        {
            string[] lines = File.ReadAllLines(fpsDisplayPath);

            List<string> tempList = new List<string>();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && line.Contains(','))
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        tempList.Add(parts[1]);
                    }
                }
            }

            if (bool.TryParse(tempList[0], out bool fpsbool))
            {
                shouldFPSDisplay = fpsbool;
            }

            if (Int32.TryParse(tempList[1], out Int32 size))
            {
                fpsSize = size;
            }

            if (Int32.TryParse(tempList[2], out Int32 xpos))
            {
                fpsXPos = xpos;
            }

            if (Int32.TryParse(tempList[3], out Int32 ypos))
            {
                fpsYPos = ypos;
            }

            if (bool.TryParse(tempList[4], out bool autoMashEnabled))
            {
                DoAutoMash.Instance.autoMash = autoMashEnabled;
                autoMashState = autoMashEnabled;
            }

            if (bool.TryParse(tempList[5], out bool _uncapFPSinLoading))
            {
                uncappedFPSinLoading = _uncapFPSinLoading;
            }
        }

        public void Start()
        {
            currentStyle = new GUIStyle();

            if(!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            else
            {
                if (!File.Exists(filePath)) 
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        String fpsBytes = "300";
                        byte[] buf = new UTF8Encoding(true).GetBytes(fpsBytes);
                        fs.Write(buf, 0, fpsBytes.Length);
                    }
                }

                ReadFPSFile();

                if (!File.Exists(SplitsPath))
                {
                    string splitsText = "-- Any%\nPrologue End,true\nEarly Mataan (Splits when you enter Millenium Square),false\nVersum Hill Start,true\nDream Sequence 1 Start,true\nChapter 1 End,true\nBrink Terminal Start,true\nDream Sequence 2 Start,true\nChapter 2 End,true\nMillenium Mall Start,true\nDream Sequence 3 Start,true\nChapter 3 End,true\nFlesh Prince Versum End,false\nFlesh Prince Millenium End,false\nFlesh Prince Brink End,false\nPyramid Island Start,false\nDream Sequence 4 Start,true\nChapter 4 End,true\nFinal Boss Defeated,true";
                    File.WriteAllText(SplitsPath, splitsText);
                }

                if (File.Exists(keyConfigPath))
                {
                    configureKeys();
                }
                else
                {
                    File.WriteAllText(keyConfigPath, "Limit framerate,O\nUncap framerate,P\nAutoMash Toggle,T");
                    configureKeys();
                }

                if (File.Exists(fpsDisplayPath))
                {
                    configFPSDisplay();
                }
                else
                {
                    File.WriteAllText(fpsDisplayPath,
                        "Display FPS,true\n" +
                        "FPS Size,30\n" +
                        "FPS X Pos,10\n" +
                        "FPS Y Pos,5\n" +
                        "AutoMash Starts Enabled,true\n" +
                        "Unlock FPS in loading screens,false");

                    configFPSDisplay();
                }

                lsCon = FindObjectOfType<LiveSplitControl>();

                // This function seems to hang up the Start function so make sure it's always put last.
                lsCon.ConnectToLiveSplit();
            }
        }

        private void ReadFPSFile() 
        {
            if (File.Exists(filePath))
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    fpsCapStr = sr.ReadToEnd();

                    if (Int32.TryParse(fpsCapStr, out int i) && Int32.Parse(fpsCapStr) >= 30)
                    {
                        fpsCapInt = i;
                    }
                    else
                    {
                        fpsCapInt = 300;
                        fpsCapStr = "300";
                    }
                }
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
                splitsWinRect = GUI.Window(1, splitsWinRect, SplitsWinProc, "Splits");
            }

            if (creditsOpen && open)
            {
                GUI.color = new Color(0, 0, 0, 1);
                creditsWinRect = GUI.Window(2, creditsWinRect, CreditsWinProc, "Credits");
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
                    CreateText(20, Screen.height - 40, 600, 600, new Color(0.2f, 1, 0.2f, 0.5f), "AutoMash Enabled");

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
                    CreateText(20, Screen.height - 40, 600, 600, new Color(1, 0.2f, 0.2f, 0.5f), "AutoMash Disabled");

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

            GUI.Label(new(ox, oy, mx, 20), $"FPS Limit ({limitKey}): {(limiting ? "<color=green>On</color>" : "<color=red>Off</color>")}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"Uncapped ({uncapKey}): {(uncapped ? "<color=green>On</color>" : "<color=red>Off</color>")}");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), $"{(lsCon.IsConnectedToLivesplit ? "<color=green>Connected to LiveSplit</color>" : "<color=red>Not connected to LiveSplit</color>")}");
            oy += 10 + 10;


            fpsCapStr = GUI.TextField(new Rect(ox, oy, mx, 20), fpsCapStr, 4);
            
            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Set FPS"))
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

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Connect to LiveSplit"))
            {
                lsCon.ConnectToLiveSplit();
            }

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Splits"))
            {
                tempSplitsArray = lsCon.SplitArray;
                splitsOpen = !splitsOpen;
            }

            oy += 10 + 15;

            GUI.Label(new(ox, oy, mx, 20), "Report any issues in #technical-help");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Developed by <color=purple>Loomeh</color>");
            oy += 10 + 15;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Credits"))
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

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Save Splits"))
            {
                lsCon.ReplaceBoolArrayInFile(SplitsPath, tempSplitsArray);
            }

            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Close"))
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

            GUI.Label(new(ox, oy, mx, 20), "Judah - JudahsSpeedUtils");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Storied - Beta testing");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "ItzBytez - Beta testing");
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

            if (Input.GetKeyDown(KeyCode.Quote))
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
