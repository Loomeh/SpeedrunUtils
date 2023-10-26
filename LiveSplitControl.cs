using BepInEx;
using Reptile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
        private bool prevIsLoading;
        private bool inCutscene;
        private Story.ObjectiveID objective;
        private Story.ObjectiveID prevObjective;
        private Stage prevStage;
        public SequenceHandler sequenceHandler;
        public SaveSlotData saveSlotData;
        public WorldHandler worldHandler;

        public bool IsConnectedToLivesplit = false;

        private int StageID = 255;
        private bool[] SplitArray;

        private string IpAddress = "127.0.0.1";
        private int Port = 16834;

        private float timer = 0.0f;
        private float prevTimer = 0.0f;
        private float updateInterval = 0.002f;
        private float prevInterval = 0.022f;

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
                List<bool> tempList = new List<bool>();

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line) && line.Contains(','))
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            tempList.Add(bool.Parse(parts[1]));
                        }
                    }
                }

                SplitArray = tempList.ToArray();
            }
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
                //inCutscene = sequenceHandler.IsInSequence();

                if(BaseModule.CurrentStage == Stage.Prelude && prevIsLoading && !IsLoading)
                {
                    Stream.Write(Encoding.UTF8.GetBytes("starttimer\r\n"), 0, Encoding.UTF8.GetBytes("starttimer\r\n").Length);
                }


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

                if((BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.Prelude && SplitArray[0])
                //||
                //(BaseModule.CurrentStage == Stage.hideout && !inCutscene && IsLoading && !prevIsLoading && objective == Story.ObjectiveID.EscapePoliceStation && SplitArray[0]) - Having issues with referencing the sequence handler
                ||
                (BaseModule.CurrentStage == Stage.downhill && (prevStage == Stage.hideout || prevStage == Stage.square) && (objective == Story.ObjectiveID.EscapePoliceStation || objective == Story.ObjectiveID.JoinTheCrew || objective == Story.ObjectiveID.BeatFranks) && SplitArray[2])
                ||
                (BaseModule.CurrentStage == Stage.downhill && objective == Story.ObjectiveID.DJChallenge1 && prevObjective == Story.ObjectiveID.BeatFranks && SplitArray[3])) //Not working - no split
                    Stream.Write(Encoding.UTF8.GetBytes("split\r\n"), 0, Encoding.UTF8.GetBytes("split\r\n").Length);

                //if (sequenceHandler == null) { sequenceHandler = FindObjectOfType<SequenceHandler>(); }
            }
        }

        public void Update()
        {
            timer += Time.deltaTime;
            prevTimer += Time.deltaTime;

            if(timer >= updateInterval)
            {
                UpdateAutosplitter();
                timer = 0.0f;
            }

            if(prevTimer >= prevInterval)
            {
                prevIsLoading = IsLoading;
                prevStage = BaseModule.CurrentStage;
                prevObjective = objective;
            }

            Debug.Log(Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective);
        }

        public void OnApplicationQuit()
        {
            if(IsConnectedToLivesplit)
                Stream.Write(Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
        }
    }
}
