using BepInEx;
using Reptile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
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
        public bool IsLoading;
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
        public bool IsLoadingNoExtend;
        public bool prevIsLoadingNoExtend;

        public bool IsConnectedToLivesplit = false;
        public bool newGame;

        private bool[] SplitArray;

        private string IpAddress = "127.0.0.1";
        private int Port = 16834;


        private bool HasSentPauseCommand = false;

        private TcpClient Client = null;
        private NetworkStream Stream = null;

        public void Awake()
        {
            if(!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);
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
                    if (BaseModule.StageManager != null)
                    {
                        prevIsLoading = IsLoading;
                        IsLoading = BaseModule.IsLoading || BaseModule.StageManager.IsExtendingLoadingScreen;
                    }
                    else
                    {
                        IsLoading = BaseModule.IsLoading;
                    }

                    prevIsLoadingNoExtend = IsLoadingNoExtend;
                    IsLoadingNoExtend = BaseModule.IsLoading;

                    prevStage = currentStage;
                    currentStage = BaseModule.CurrentStage;

                    prevObjective = objective;
                    prevFinalBossHit = finalBossHit;
                }

                if (prevIsLoadingNoExtend && !IsLoadingNoExtend && Core.Instance.SaveManager != null && Core.Instance.SaveManager.CurrentSaveSlot != null && !Core.Instance.SaveManager.CurrentSaveSlot.fortuneAppLocked)
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

                if (finalBossHit && currentStage != Stage.osaka) { finalBossHit = false; }

                if (finalBossGO == null && currentStage == Stage.osaka && (objective == Story.ObjectiveID.BeatOsaka || objective == Story.ObjectiveID.FinalBoss)) { finalBossGO = GameObject.FindGameObjectWithTag("SnakebossHead"); }
                if (finalBossGO != null) { finalBossHit = finalBossGO.transform.GetComponent<SnakeBossChestImpactReceiver>().WasHit; }


                objective = Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective;
            }
        }

        public void UpdateAutosplitter()
        {
            if (IsConnectedToLivesplit || debug)
            {
                UpdateFields();

                if (currentStage == Stage.Prelude && newGame && !IsLoading)
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
                    (currentStage == Stage.hideout && prevStage == Stage.Prelude && SplitArray[0])
                    ||
                    (currentStage == Stage.square && prevStage == Stage.osaka && (objective == Story.ObjectiveID.EscapePoliceStation || objective == Story.ObjectiveID.JoinTheCrew) && SplitArray[1])
                    ||
                    (currentStage == Stage.downhill && (prevStage == Stage.hideout || prevStage == Stage.square) && (objective == Story.ObjectiveID.EscapePoliceStation || objective == Story.ObjectiveID.JoinTheCrew || objective == Story.ObjectiveID.BeatFranks) && SplitArray[2])
                    ||
                    (objective == Story.ObjectiveID.DJChallenge1 && (prevObjective == Story.ObjectiveID.BeatFranks || prevObjective == Story.ObjectiveID.EscapePoliceStation) && SplitArray[3])
                    ||
                    (currentStage == Stage.hideout && prevStage == Stage.downhill && objective == Story.ObjectiveID.GoToSquare && SplitArray[4])
                    ||
                    (currentStage == Stage.tower && prevStage == Stage.square && objective == Story.ObjectiveID.BeatEclipse && SplitArray[5])
                    ||
                    (currentStage == Stage.tower && objective == Story.ObjectiveID.DJChallenge2 && prevObjective == Story.ObjectiveID.BeatEclipse && SplitArray[6])
                    ||
                    (currentStage == Stage.hideout && prevStage == Stage.tower && objective == Story.ObjectiveID.BeatDotExe && SplitArray[7])
                    ||
                    (currentStage == Stage.Mall && prevStage == Stage.square && objective == Story.ObjectiveID.BeatDotExe && SplitArray[8])
                    ||
                    (currentStage == Stage.Mall && objective == Story.ObjectiveID.DJChallenge3 && prevObjective == Story.ObjectiveID.BeatDotExe && SplitArray[9])
                    ||
                    (currentStage == Stage.hideout && prevStage == Stage.Mall && objective == Story.ObjectiveID.SearchForPrince && SplitArray[10])
                    ||
                    (currentStage == Stage.downhill && objective == Story.ObjectiveID.SearchForPrince2 && prevObjective == Story.ObjectiveID.SearchForPrince && SplitArray[11])
                    ||
                    (currentStage == Stage.square && objective == Story.ObjectiveID.SearchForPrince3 && prevObjective == Story.ObjectiveID.SearchForPrince2 && SplitArray[12])
                    ||
                    (currentStage == Stage.tower && objective == Story.ObjectiveID.SearchForPrince4 && prevObjective == Story.ObjectiveID.SearchForPrince3 && SplitArray[13])
                    ||
                    (currentStage == Stage.hideout && prevStage == Stage.osaka && objective == Story.ObjectiveID.BeatSamurai && SplitArray[14])
                    ||
                    (currentStage == Stage.pyramid && prevStage == Stage.square && (objective == Story.ObjectiveID.SearchForPrince || objective == Story.ObjectiveID.BeatSamurai) && SplitArray[14])
                    ||
                    (currentStage == Stage.pyramid && objective == Story.ObjectiveID.DJChallenge4 && prevObjective == Story.ObjectiveID.BeatSamurai && SplitArray[15])
                    ||
                    (currentStage == Stage.pyramid && objective == Story.ObjectiveID.DJChallenge4 && prevObjective == Story.ObjectiveID.SearchForPrince && SplitArray[15])
                    ||
                    (currentStage == Stage.hideout && prevStage == Stage.pyramid && objective == Story.ObjectiveID.BeatOsaka && SplitArray[16])
                    ||
                    (currentStage == Stage.osaka && (objective == Story.ObjectiveID.BeatOsaka || objective == Story.ObjectiveID.FinalBoss) && finalBossHit && !prevFinalBossHit && SplitArray[17])
                    )
                {
                        // Stupid hack because it splits on main menu for some reason.
                        if(currentStage != Stage.NONE)
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

            if(File.Exists(SplitsPath) && SplitArray == null)
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
