using BepInEx;
using Reptile;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunUtils
{
    public class LiveSplitControl : MonoBehaviour
    {
        private static readonly string ConfigPath = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        private readonly string SplitsPath = Path.Combine(ConfigPath, "splits.txt");

        public BaseModule BaseModule;
        private bool IsLoading;

        public bool IsConnectedToLivesplit = false;

        private int StageID = 255;
        private bool[] SplitArray;

        private string IpAddress = "127.0.0.1";
        private int Port = 16834;

        private float timer = 0.0f;
        private float updateInterval = 0.002f;

        private bool HasSentPauseCommand = false;

        private TcpClient Client = null;
        private NetworkStream Stream = null;

        public void Start()
        {
            if (!File.Exists(SplitsPath))
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile("https://raw.githubusercontent.com/Loomeh/BRCAutosplitter/main/splits.txt", SplitsPath);
                }
            }
            else
            {
                string[] lines = File.ReadAllLines(SplitsPath);
                SplitArray = new bool[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(',');
                    SplitArray[i] = bool.Parse(parts[1]);
                }
            }

            ConnectToLiveSplit();
        }

        public void ConnectToLiveSplit()
        {
            if (!IsConnectedToLivesplit)
            {
                Byte[] data = new Byte[256];
                String responseData = String.Empty;

                Client = new TcpClient(IpAddress, Port);
                Stream = Client.GetStream();

                Stream.Write(Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n"), 0, Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n").Length);

                int bytes = Stream.Read(data, 0, data.Length);
                responseData = Encoding.ASCII.GetString(data, 0, bytes);

                if (responseData != String.Empty)
                {
                    Stream.Write(Encoding.UTF8.GetBytes("initgametime\r\n"), 0, Encoding.UTF8.GetBytes("initgametime\r\n").Length);
                    IsConnectedToLivesplit = true;
                }
            }
        }

        public void UpdateAutosplitter()
        {
            if (IsConnectedToLivesplit)
            {
                if (BaseModule == null)
                    BaseModule = Core.Instance.BaseModule;

                IsLoading = BaseModule.IsLoading;

                if (IsLoading || SceneManager.GetActiveScene().name == "intro" || SceneManager.GetActiveScene().name == "Bootstrap" || SceneManager.GetActiveScene().name == "Core")
                {
                    if (!HasSentPauseCommand)
                    {
                        Debug.Log("Pausing game time!");
                        Stream.Write(Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
                        HasSentPauseCommand = true;
                    }
                }
                else if (HasSentPauseCommand)
                {
                    if (!IsLoading && SceneManager.GetActiveScene().name != "intro" && SceneManager.GetActiveScene().name != "Bootstrap" && SceneManager.GetActiveScene().name != "Core")
                    {
                        Debug.Log("Unpausing game time!");
                        Stream.Write(Encoding.UTF8.GetBytes("unpausegametime\r\n"), 0, Encoding.UTF8.GetBytes("unpausegametime\r\n").Length);
                        HasSentPauseCommand = false;
                    }
                }
            }
        }

        public void Update()
        {
            timer += Time.deltaTime;

            if(timer >= updateInterval)
            {
                UpdateAutosplitter();
                timer = 0.0f;
            }
        }
    }
}
