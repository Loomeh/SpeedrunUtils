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
        private Rect debugRect = new(20, 40, 275, 150);

        public string fpsCapStr = "";
        public int fpsCapInt = -1;
        public bool limiting = false;
        public bool uncapped = false;
        public KeyCode uncapKey;
        public KeyCode limitKey;

        public static string configFolder = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        private string filePath = Path.Combine(configFolder, "FPS.txt");
        private string keyConfigPath = Path.Combine(configFolder, "Keys.txt");
        private readonly string SplitsPath = Path.Combine(configFolder, "splits.txt");

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

            limitKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), tempList[1], true);
            uncapKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), tempList[0], true);
        }


        public void Start()
        {
            if(!Directory.Exists(configFolder))
            {
                Directory.CreateDirectory(configFolder);
            }
            else
            {
                if (File.Exists(filePath))
                {
                    using (StreamReader sr = File.OpenText(filePath))
                    {
                        fpsCapStr = sr.ReadToEnd();
                        fpsCapInt = Int32.Parse(fpsCapStr);
                    }
                }

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

                lsCon = FindObjectOfType<LiveSplitControl>();

                // This function seems to hang up the Start function so make sure it's always put last.
                lsCon.ConnectToLiveSplit();
            }
        }

        public void OnGUI()
        {
            if (open)
            {
                winRect = GUI.Window(0, winRect, WinProc, $"{PluginInfo.PLUGIN_NAME} (1.3.3)");
            }

            if(lsCon.debug)
            {
                debugRect = GUI.Window(1, debugRect, DebugWinProc, "SpeedrunUtils Debug Mode");
            }
        }

        private void WinProc(int id)
        {
            var ox = 15f;
            var oy = 30f;
            
            var mx = winRect.width - 30;


            GUI.Label(new(ox, oy, mx, 20), $"FPS Limit: {(limiting ? "<color=green>On</color>" : "<color=red>Off</color>")}");
            oy += 10 + 5;

            GUI.Label(new(ox, oy, mx, 20), $"Uncapped: {(uncapped ? "<color=green>On</color>" : "<color=red>Off</color>")}");
            oy += 10 + 10;

            GUI.Label(new(ox, oy, mx, 20), $"{(lsCon.IsConnectedToLivesplit ? "<color=green>Connected to LiveSplit</color>" : "<color=red>Not connected to LiveSplit</color>")}");
            oy += 10 + 10;


            fpsCapStr = GUI.TextField(new Rect(ox, oy, mx, 20), fpsCapStr, 4);


            oy += 10 + 10;

            if (GUI.Button(new Rect(ox, oy, mx, 20), "Set FPS"))
            {
                fpsCapInt = Int32.Parse(fpsCapStr);

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