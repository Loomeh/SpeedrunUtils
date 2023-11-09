using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunUtils
{
    public class ConfigUi : MonoBehaviour
    {
        private LiveSplitControl lsCon;

        public bool open = false;

        private Rect winRect = new(20, 20, 275, 200);
        private Rect debugRect = new(20, 40, 275, 175);

        public string fpsCapStr = "";
        public int fpsCapInt = -1;
        public bool limiting = false;
        public bool uncapped = false;
        public KeyCode limitKey = KeyCode.O;
        public KeyCode uncapKey = KeyCode.P;

        private GUIStyle currentStyle;

        public static string configFolder = Paths.ConfigPath + @"\SpeedrunUtils\";
        private string filePath = Path.Combine(configFolder, "FPS.txt");
        private string keyConfigPath = Path.Combine(configFolder, "Keys.txt");
        private readonly string SplitsPath = Path.Combine(configFolder, "splits.txt");
        private readonly string fpsDisplayPath = Path.Combine(configFolder, "FPSDisplay.txt");

        static int screenHeight = Screen.height;
        static int screenWidth = Screen.width;

        private Rect screenBounds = new Rect(0, 0, screenWidth, screenHeight);

        private int guiX = (screenWidth / 2) - (600 / 2);
        private int guiY = (screenHeight / 2) - (((screenHeight) - (screenHeight / 20)) / 2);
        private int guiW = 600;
        private int guiH = (screenHeight) - (screenHeight / 20);

        private bool shouldFPSDisplay = true;
        private int fpsSize = 30;
        private int fpsXPos = 10;
        private int fpsYPos = 5;

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
        }

        public void Start()
        {
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
                        String fpsBytes = "30";
                        byte[] buf = new UTF8Encoding(true).GetBytes(fpsBytes);
                        fs.Write(buf, 0, fpsBytes.Length);
                    }
                }

                ReadFPSFile();

                if (!File.Exists(SplitsPath))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile("https://raw.githubusercontent.com/Loomeh/BRCAutosplitter/main/splits.txt", SplitsPath);
                    }
                }

                if (File.Exists(keyConfigPath))
                {
                    configureKeys();
                }
                else
                {
                    File.WriteAllText(keyConfigPath, "Limit framerate,O\nUncap framerate,P");
                    configureKeys();
                }

                if (File.Exists(fpsDisplayPath))
                {
                    configFPSDisplay();
                }
                else
                {
                    File.WriteAllText(fpsDisplayPath, "Display FPS,true\nSize,30\nX Pos,10\nY Pos,5");
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

        public void OnGUI()
        {
            if (open)
            {
                GUI.color = new Color(0, 0, 0, 1);
                winRect = GUI.Window(0, winRect, WinProc, $"{PluginInfo.PLUGIN_NAME} (1.3.6)");
            }
            else if (shouldFPSDisplay)
            {
                screenHeight = Screen.height;
                screenWidth = Screen.width;
                screenBounds = new Rect(0, 0, screenWidth, screenHeight);

                GUI.color = new Color(0, 0, 0, 0);
                screenBounds = GUI.Window(0, screenBounds, FPSDisplay, "");
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

            GUI.Label(new(ox, oy, mx, 20), $"FPS: {1 / Time.deltaTime:F0}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"FPS Limit ({limitKey.ToString()}): {(limiting ? "<color=green>On</color>" : "<color=red>Off</color>")}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"Uncapped ({uncapKey.ToString()}): {(uncapped ? "<color=green>On</color>" : "<color=red>Off</color>")}");
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

            oy += 10 + 15;

            GUI.Label(new(ox, oy, mx, 20), "Report any issues in #technical-help");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), "Developed by <color=purple>Loomeh</color> and <color=cyan>Ninja Cookie</color>");
            oy += 10 + 5;

            GUI.DragWindow();
        }

        private void FPSDisplay(int id)
        {
            CreateText(fpsXPos, fpsYPos, 600, 600, new Color(1, 1, 1, 0.5f), $"{1 / Time.deltaTime:F0}");
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
            if (Input.GetKeyDown(KeyCode.Quote)) open = !open;

            if (Input.GetKeyDown(limitKey))
            {
                limiting = !limiting;

                if (uncapped)
                    uncapped = false;
            }

            if (Input.GetKeyDown(uncapKey))
            {
                uncapped = !uncapped;
                
                if(limiting)
                    limiting = false;
            }


            if(QualitySettings.vSyncCount > 0)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (limiting)
            {
                Application.targetFrameRate = 30;
            }
            else
            {
                if (uncapped)
                    Application.targetFrameRate = -1;
                else
                    Application.targetFrameRate = fpsCapInt;
            }
        }
    }
}
