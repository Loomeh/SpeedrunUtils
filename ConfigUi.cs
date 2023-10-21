using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace SpeedrunUtils
{
    public class ConfigUi : MonoBehaviour
    {
        public bool open = false;

        private Rect winRect = new(20, 20, 275, 115);

        public string fpsCapStr = "";
        public int fpsCapInt = -1;
        public bool limiting = false;
        public bool uncapped = false;

        private string filePath = Path.Combine(Path.GetTempPath(), "SpeedrunUtils.txt");

        public void Start()
        {
            if(File.Exists(filePath))
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    fpsCapStr = sr.ReadToEnd();
                    fpsCapInt = Int32.Parse(fpsCapStr);
                }
            }
        }

        public void OnGUI()
        {
            if (open)
            {
                winRect = GUI.Window(0, winRect, WinProc, $"{PluginInfo.PLUGIN_NAME} ({PluginInfo.PLUGIN_VERSION})");
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

            fpsCapStr = GUI.TextField(new Rect(ox, oy, mx, 20), fpsCapStr, 4);

            oy += 10 + 10;

            if(GUI.Button(new Rect(ox, oy, mx, 20), "Set FPS"))
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

            GUI.DragWindow();
        }

        
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Quote)) open = !open;

            if (Input.GetKeyDown(KeyCode.P))
            {
                limiting = !limiting;

                if (uncapped)
                    uncapped = false;
            }

            if (Input.GetKeyDown(KeyCode.O))
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