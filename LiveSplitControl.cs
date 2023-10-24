using BepInEx;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Reptile;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.IO;
using System.Net;
using System.Reflection;

namespace SpeedrunUtils
{
    public class LiveSplitControl : MonoBehaviour
    {
        public static string configPath = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        private string splitsPath = Path.Combine(configPath, "splits.txt");

        public BaseModule baseModule;
        public bool isLoading;

        public bool isConnectedToLivesplit = false;

        public int StageID = 255;
        public bool[] splitArray;

        public string ipAddress = "127.0.0.1";
        public Int32 port = 16834;


        public void Start()
        {
            if(!File.Exists(splitsPath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://raw.githubusercontent.com/Loomeh/BRCAutosplitter/main/splits.txt", splitsPath);
                }
            }
            else
            {
                string[] lines = File.ReadAllLines(splitsPath);
                splitArray = new bool[lines.Length];
                
                for(int i = 0; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(',');
                    splitArray[i] = bool.Parse(parts[1]);
                }
            }

        }


        private bool hasSentPauseCommand = false;
        public void Update()
        {
            bool prevLoading = false;
            if(baseModule == null)
                baseModule = Core.Instance.BaseModule;

            isLoading = baseModule.IsLoading;


            string currentSceneName = SceneManager.GetActiveScene().name;

            TcpClient client = new TcpClient(ipAddress, port);
            NetworkStream stream = client.GetStream();

            if(!isConnectedToLivesplit)
            {
                Byte[] data = new Byte[256];
                String responseData = String.Empty;

                stream.Write(System.Text.Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n"), 0, System.Text.Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n").Length);

                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                if (responseData != String.Empty)
                {
                    stream.Write(System.Text.Encoding.UTF8.GetBytes("initgametime\r\n"), 0, System.Text.Encoding.UTF8.GetBytes("initgametime\r\n").Length);
                    isConnectedToLivesplit = true;
                }
                    
            }


            if (isConnectedToLivesplit)
            {
                if (isLoading)
                {
                    if(!hasSentPauseCommand)
                    {
                        Debug.Log("Pausing game time!");
                        stream.Write(System.Text.Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, System.Text.Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
                        hasSentPauseCommand = true;
                    }
                }
                else if(hasSentPauseCommand)
                {
                    if(!isLoading)
                    {
                        Debug.Log("Unpausing game time!");
                        stream.Write(System.Text.Encoding.UTF8.GetBytes("unpausegametime\r\n"), 0, System.Text.Encoding.UTF8.GetBytes("unpausegametime\r\n").Length);
                        hasSentPauseCommand = false;
                    }
                }
            }
            prevLoading = isLoading;
        }
    }
}