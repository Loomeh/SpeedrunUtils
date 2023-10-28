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
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace SpeedrunUtils
{
    public class LiveSplitControl : MonoBehaviour
    {
        private static readonly string ConfigPath = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        private readonly string SplitsPath = Path.Combine(ConfigPath, "splits.txt");

        public bool debug = false;

        public BaseModule BaseModule;
        private bool IsLoading;
        private bool prevIsLoading;
        public Story.ObjectiveID objective;
        public Story.ObjectiveID prevObjective;
        public Stage currentStage;
        public Stage prevStage;
        public SequenceState sequenceState;
        public SequenceHandler sequenceHandler;
        public PlayableDirector sequence;
        public string sequenceName;
        public SaveSlotData saveSlotData;
        public WorldHandler worldHandler;
        public Player player;
        public GameObject finalBossGO;
        public bool finalBossHit;
        public bool prevFinalBossHit;
        public bool isboostlocked;
        public bool inCutscene;

        public bool IsConnectedToLivesplit = false;
        public bool newGame;

        private bool[] SplitArray;

        private string IpAddress = "127.0.0.1";
        private int Port = 16834;


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
            try
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
            catch (SocketException ex)
            {
                Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An unexpected error occurred: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
        }

        private void UpdateFields()
        {
            if (Core.Instance != null)
            {
                if (BaseModule == null) { BaseModule = Core.Instance.BaseModule; }
                if (worldHandler == null) { worldHandler = WorldHandler.instance; }

                if (BaseModule != null)
                {
                    prevIsLoading = IsLoading;
                    IsLoading = BaseModule.IsLoading;

                    prevStage = currentStage;
                    currentStage = BaseModule.CurrentStage;

                    prevObjective = objective;
                    prevFinalBossHit = finalBossHit;
                }

                if (prevIsLoading && !IsLoading && Core.Instance.SaveManager != null && Core.Instance.SaveManager.CurrentSaveSlot != null && !Core.Instance.SaveManager.CurrentSaveSlot.fortuneAppLocked)
                {
                    newGame = true;
                }

                if (player == null && worldHandler != null) { player = worldHandler.GetCurrentPlayer(); }
                if (player != null)
                {
                    inCutscene = player.IsBusyWithSequence();
                    sequenceState = (SequenceState)typeof(Player).GetField("sequenceState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player);
                }

                if (sequenceHandler == null && worldHandler != null) { sequenceHandler = (SequenceHandler)typeof(WorldHandler).GetField("sequenceHandler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldHandler); }
                if (sequenceHandler != null && inCutscene && sequenceName == "") { sequence = (PlayableDirector)typeof(SequenceHandler).GetField("sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequenceHandler); sequenceName = sequence.name; }
                else if (!inCutscene) { sequenceName = ""; }

                if (finalBossGO == null && BaseModule.CurrentStage == Stage.osaka && (objective == Story.ObjectiveID.BeatOsaka || objective == Story.ObjectiveID.FinalBoss)) { finalBossGO = GameObject.FindGameObjectWithTag("SnakebossHead"); }
                if (finalBossGO != null) { finalBossHit = finalBossGO.transform.GetComponent<SnakeBossChestImpactReceiver>().WasHit; }

                objective = Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective;
            }
        }

        public void UpdateAutosplitter()
        {
            if (IsConnectedToLivesplit || debug)
            {
                UpdateFields();

                if(currentStage == Stage.Prelude && newGame)
                {
                    try
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("reset\r\n"), 0, Encoding.UTF8.GetBytes("reset\r\n").Length);
                        Stream.Write(Encoding.UTF8.GetBytes("starttimer\r\n"), 0, Encoding.UTF8.GetBytes("starttimer\r\n").Length);
                        newGame = false;
                    }
                    catch (SocketException ex)
                    {
                        Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                        IsConnectedToLivesplit = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"An unexpected error occurred: {ex.Message}");
                        IsConnectedToLivesplit = false;
                    }
                }


                if (IsLoading || SceneManager.GetActiveScene().name == "intro" || SceneManager.GetActiveScene().name == "Bootstrap" || SceneManager.GetActiveScene().name == "Core")
                {
                    if (!HasSentPauseCommand)
                    {
                        try
                        {
                            Debug.Log("Pausing game time!");
                            Stream.Write(Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
                            HasSentPauseCommand = true;
                        }
                        catch (SocketException ex)
                        {
                            Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                            IsConnectedToLivesplit = false;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"An unexpected error occurred: {ex.Message}");
                            IsConnectedToLivesplit = false;
                        }
                    }
                }
                else if (HasSentPauseCommand)
                {
                    if (!IsLoading && SceneManager.GetActiveScene().name != "intro" && SceneManager.GetActiveScene().name != "Bootstrap" && SceneManager.GetActiveScene().name != "Core")
                    {
                        try
                        {
                            Debug.Log("Unpausing game time!");
                            Stream.Write(Encoding.UTF8.GetBytes("unpausegametime\r\n"), 0, Encoding.UTF8.GetBytes("unpausegametime\r\n").Length);
                            HasSentPauseCommand = false;
                        }
                        catch (SocketException ex)
                        {
                            Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                            IsConnectedToLivesplit = false;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"An unexpected error occurred: {ex.Message}");
                            IsConnectedToLivesplit = false;
                        }
                    }
                }

                if (
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.Prelude && SplitArray[0])
                    ||
                    (BaseModule.CurrentStage == Stage.downhill && (prevStage == Stage.hideout || prevStage == Stage.square) && (objective == Story.ObjectiveID.EscapePoliceStation || objective == Story.ObjectiveID.JoinTheCrew || objective == Story.ObjectiveID.BeatFranks) && SplitArray[1])
                    ||
                    (objective == Story.ObjectiveID.DJChallenge1 && (prevObjective == Story.ObjectiveID.BeatFranks || prevObjective == Story.ObjectiveID.EscapePoliceStation) && SplitArray[2])
                    ||
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.downhill && objective == Story.ObjectiveID.GoToSquare && SplitArray[3]) // Broken
                    ||
                    (BaseModule.CurrentStage == Stage.tower && prevStage == Stage.square && objective == Story.ObjectiveID.BeatEclipse && SplitArray[4]) // Broken
                    ||
                    (BaseModule.CurrentStage == Stage.tower && objective == Story.ObjectiveID.DJChallenge2 && prevObjective == Story.ObjectiveID.BeatEclipse && SplitArray[5])
                    ||
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.tower && objective == Story.ObjectiveID.BeatDotExe && SplitArray[6])
                    ||
                    (BaseModule.CurrentStage == Stage.Mall && prevStage == Stage.square && objective == Story.ObjectiveID.BeatDotExe && SplitArray[7])
                    ||
                    (BaseModule.CurrentStage == Stage.Mall && objective == Story.ObjectiveID.DJChallenge3 && prevObjective == Story.ObjectiveID.BeatDotExe && SplitArray[8])
                    ||
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.Mall && objective == Story.ObjectiveID.SearchForPrince && SplitArray[9])
                    ||
                    (BaseModule.CurrentStage == Stage.downhill && objective == Story.ObjectiveID.SearchForPrince2 && prevObjective == Story.ObjectiveID.SearchForPrince && SplitArray[10])
                    ||
                    (BaseModule.CurrentStage == Stage.square && objective == Story.ObjectiveID.SearchForPrince3 && prevObjective == Story.ObjectiveID.SearchForPrince2 && SplitArray[11])
                    ||
                    (BaseModule.CurrentStage == Stage.tower && objective == Story.ObjectiveID.SearchForPrince4 && prevObjective == Story.ObjectiveID.SearchForPrince3 && SplitArray[12])
                    ||
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.osaka && objective == Story.ObjectiveID.BeatSamurai && SplitArray[13])
                    ||
                    (BaseModule.CurrentStage == Stage.pyramid && prevStage == Stage.square && (objective == Story.ObjectiveID.SearchForPrince || objective == Story.ObjectiveID.BeatSamurai) && SplitArray[13])
                    ||
                    (BaseModule.CurrentStage == Stage.pyramid && objective == Story.ObjectiveID.DJChallenge4 && prevObjective == Story.ObjectiveID.BeatSamurai && SplitArray[14])
                    ||
                    (BaseModule.CurrentStage == Stage.pyramid && objective == Story.ObjectiveID.DJChallenge4 && prevObjective == Story.ObjectiveID.SearchForPrince && SplitArray[14])
                    ||
                    (BaseModule.CurrentStage == Stage.hideout && prevStage == Stage.pyramid && objective == Story.ObjectiveID.BeatOsaka && SplitArray[15])
                    ||
                    (BaseModule.CurrentStage == Stage.osaka && (objective == Story.ObjectiveID.BeatOsaka || objective == Story.ObjectiveID.FinalBoss) && finalBossHit && !prevFinalBossHit && SplitArray[16])
                    )
                {
                        // Stupid hack because it splits on main menu for some reason.
                        if(BaseModule.CurrentStage != Stage.NONE)
                        {
                            try
                            {
                                Stream.Write(Encoding.UTF8.GetBytes("split\r\n"), 0, Encoding.UTF8.GetBytes("split\r\n").Length);
                            }
                            catch (SocketException ex)
                            {
                                Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                                IsConnectedToLivesplit = false;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"An unexpected error occurred: {ex.Message}");
                                IsConnectedToLivesplit = false;
                            }
                    }
                            
                }
            }
        }

        public void Update()
        {
            if(IsConnectedToLivesplit || debug)
            {
                UpdateAutosplitter();
            }


            //Debug.Log($"Current Objective: {objective}. Previous Objective: {prevObjective}");
            //Debug.Log(string.Join("\n", SplitArray));
            //Debug.Log(BaseModule.CurrentStage);
        }

        public void OnApplicationQuit()
        {
            if(IsConnectedToLivesplit)
            {
                try
                {
                    Stream.Write(Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
                }
                catch (SocketException ex)
                {
                    Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                    IsConnectedToLivesplit = false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An unexpected error occurred: {ex.Message}");
                    IsConnectedToLivesplit = false;
                }
            }
        }
    }
}
