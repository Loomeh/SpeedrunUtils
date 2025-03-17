using BepInEx;
using Reptile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace SpeedrunUtils
{
    public class LiveSplitControl : MonoBehaviour
    {
        private static readonly string ConfigPath = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        private readonly string SplitsPath = Path.Combine(ConfigPath, "splits.txt");
        private readonly string SettingsPath = Path.Combine(ConfigPath, "Settings.txt");

        public bool debug = false;

        private BaseModule BaseModule;
        public bool IsLoading;
        private bool prevIsLoading;
        public Story.ObjectiveID objective = Story.ObjectiveID.NONE;
        public Story.ObjectiveID prevObjective = Story.ObjectiveID.NONE;
        public Stage currentStage = Stage.NONE;
        public Stage prevStage = Stage.NONE;
        public SequenceState sequenceState = SequenceState.NONE;
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

        public bool[] SplitArray;

        private string IpAddress = "127.0.0.1";
        private int Port = 16834;

        private bool HasSentPauseCommand = false;
        private float cutsceneCacheTimeout = 0.5f;
        private bool cutsceneSkipInProgress = false;

        private TcpClient Client = null;
        private NetworkStream Stream = null;

        private string cutsceneNameCache;

        private const int BUFFER_SIZE = 1024;

        private Dictionary<string, CutsceneData> cutscenes = new Dictionary<string, CutsceneData>();
        private Dictionary<string, float> cutsceneTimeCache = new Dictionary<string, float>();

        private static MethodInfo cachedExitSequenceMethod;
        private static object sequenceHandlerInstance;

        // Struct to store cutscene data
        private struct CutsceneData
        {
            public string Name;
            public string Duration;
            public bool Skipped;

            public CutsceneData(string name, string duration)
            {
                Name = name;
                Duration = duration;
                Skipped = false;
            }
        }

        // Initialize cutscenes
        private void InitializeCutscenes()
        {
            cutscenes.Clear();

            cutscenes.Add("ch1s5b", new CutsceneData("ch1s5b", "1:35.751"));
            cutscenes.Add("ch1s10", new CutsceneData("ch1s10", "1:27.370"));
            //cutscenes.Add("ch1s12", new CutsceneData("ch1s12", "0:38.210"));
            cutscenes.Add("ch2s1", new CutsceneData("ch2s1", "1:42.287"));
            cutscenes.Add("ch3s1", new CutsceneData("ch3s1", "0:57.427"));
        }

        public void ResetCutscenes()
        {
            foreach (var key in cutscenes.Keys.ToList())
            {
                var cutscene = cutscenes[key];
                cutscene.Skipped = false;
                cutscenes[key] = cutscene;
            }
            cutsceneTimeCache.Clear();
            Debug.Log("All cutscenes and cache reset for new game.");
        }


        public void Awake()
        {
            InitializeCutscenes();


            if(!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);
        }

        public void Start()
        {
            if (File.Exists(SplitsPath) && SplitArray == null)
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
                if (finalBossGO != null)
                {
                    prevFinalBossHit = finalBossHit;
                    finalBossHit = finalBossGO.transform.GetComponent<SnakeBossChestImpactReceiver>().WasHit;
                }

                if (Core.Instance.SaveManager != null && Core.Instance.SaveManager.CurrentSaveSlot != null)
                {
                    prevObjective = objective;
                    objective = Core.Instance.SaveManager.CurrentSaveSlot.CurrentStoryObjective;
                }
            }
        }

        private void CacheExitSequenceMethod()
        {
            if (cachedExitSequenceMethod == null && sequenceHandler != null)
            {
                // Cache the method and instance
                cachedExitSequenceMethod = typeof(SequenceHandler).GetMethod("SetExitSequence", BindingFlags.Instance | BindingFlags.NonPublic);
                sequenceHandlerInstance = sequenceHandler;

                if (cachedExitSequenceMethod == null)
                {
                    Debug.LogError("Failed to cache 'SetExitSequence'. Method not found.");
                }
            }
        }

        public void ExitSequence()
        {
            if (sequenceHandler == null)
            {
                Debug.LogError("SequenceHandler is null. Cannot exit sequence.");
                return;
            }

            // Initialize the cached method if not already done
            CacheExitSequenceMethod();

            if (cachedExitSequenceMethod == null)
            {
                Debug.LogError("Cached 'SetExitSequence' method is null. Exiting.");
                return;
            }

            // Check if we're still loading or if the player is null
            if (IsLoading || player == null)
            {
                Debug.LogWarning("Game is loading or player not ready. Skipping sequence exit for safety.");
                return;
            }

            try
            {
                // Call the cached method on the cached instance
                cachedExitSequenceMethod.Invoke(sequenceHandlerInstance, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking 'SetExitSequence': {ex.Message}");
            }
        }



        public void UpdateAutosplitter()
        {
            if (IsConnectedToLivesplit || debug)
            {
                UpdateFields();

                if(currentStage == Stage.NONE && prevStage != Stage.NONE)
                {
                    ResetCutscenes();
                }

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

                // Manage cutscene skipping
                if (inCutscene && !cutsceneSkipInProgress && bool.Parse(SettingsManager.GetSetting(SettingsPath, "Enable cutscene skipping", "true")))
                {
                    cutsceneNameCache = sequenceName;

                    if (cutscenes.ContainsKey(cutsceneNameCache))
                    {
                        StartCoroutine(SkipCutscene(cutsceneNameCache));
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

        public IEnumerator SkipCutscene(string cutsceneName)
        {
            if (!cutscenes.ContainsKey(cutsceneName))
            {
                Debug.LogError($"Cutscene '{cutsceneName}' not found!");
                yield break;
            }

            if (!IsConnectedToLivesplit || cutsceneSkipInProgress) yield break;

            // Add safety check for loading state
            if (IsLoading || prevIsLoading)
            {
                Debug.Log("Game is loading or just finished loading, delaying cutscene skip attempt");
                yield return new WaitUntil(() => (!prevIsLoading && !IsLoading));

                // Check again if we're still in the cutscene
                if (sequenceName != cutsceneName)
                {
                    Debug.Log("No longer in target cutscene after loading delay");
                    yield break;
                }
            }

            var cutsceneData = cutscenes[cutsceneName];

            // Check if the cutscene has already been skipped
            if (cutsceneData.Skipped)
            {
                // Allow skipping again if the game is still in this cutscene
                if (sequenceName == cutsceneName)
                {
                    Debug.Log($"Cutscene '{cutsceneName}' is still active despite being marked as skipped. Continuing exit sequence.");
                    yield return StartCoroutine(cutsceneSkip(cutsceneName));
                }
                else
                {
                    Debug.Log($"Cutscene '{cutsceneName}' already skipped and is no longer active.");
                }
                yield break;
            }

            cutsceneSkipInProgress = true;

            Stopwatch responseTimer = new Stopwatch();
            responseTimer.Start();
            byte[] commandBytes = Encoding.UTF8.GetBytes("getcurrentgametime\r\n");
            Stream.Write(commandBytes, 0, commandBytes.Length);
            Stream.Flush();

            var responseBuffer = new byte[BUFFER_SIZE];
            var waitTask = Stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
            
            yield return new WaitUntil(() => waitTask.IsCompleted);

            if (waitTask.IsCompleted)
            {
                responseTimer.Stop();

                Debug.Log("LiveSplit Response Time: " + responseTimer.ElapsedMilliseconds);

                int bytesRead = waitTask.Result;
                string response = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead).Trim();
                Debug.Log($"Attempting to parse time: '{response}'");

                if (TimeSpan.TryParse(response, out TimeSpan currentGameTime))
                {
                    if (TimeSpan.TryParseExact(cutsceneData.Duration, @"m\:ss\.fff", null, out TimeSpan skipTime))
                    {
                        TimeSpan timeoutDelay = TimeSpan.FromMilliseconds(responseTimer.ElapsedMilliseconds);
                        var newTime = (currentGameTime + skipTime) - timeoutDelay;

                        string newTimeString = newTime.ToString(@"hh\:mm\:ss\.fff");
                        Debug.Log($"New time to set: {newTimeString}");

                        commandBytes = Encoding.UTF8.GetBytes($"setgametime {newTimeString}\r\n");
                        Stream.Write(commandBytes, 0, commandBytes.Length);
                        Stream.Flush();
                        Debug.Log("New time sent to server.");


                        yield return StartCoroutine(cutsceneSkip(cutsceneName));
                    }
                    else
                    {
                        Debug.LogError($"Invalid format for cutscene duration: {cutsceneData.Duration}");
                    }
                }
                else
                {
                    Debug.LogError($"Invalid format for received time: {response}");
                }
            }
            else
            {
                Debug.LogError("Timeout occurred while waiting for game time response.");
            }

            cutsceneSkipInProgress = false;


            IEnumerator cutsceneSkip(string cutsceneName)
            {
                int maxAttempts = 3;
                int attempts = 0;

                while (sequenceName == cutsceneName && attempts < maxAttempts)
                {
                    if (IsLoading || player == null)
                    {
                        Debug.Log("Game is loading or player not ready. Waiting before attempting to skip...");
                        yield return new WaitForSecondsRealtime(0.2f);
                        continue;
                    }

                    ExitSequence();
                    attempts++;
                    yield return new WaitForSecondsRealtime(0.1f);
                }

                // Mark the cutscene as skipped
                var updatedCutsceneData = cutscenes[cutsceneName];
                updatedCutsceneData.Skipped = true;
                cutscenes[cutsceneName] = updatedCutsceneData;

                // Update the cache with the current time
                cutsceneTimeCache[cutsceneName] = Time.time;

                Debug.Log($"Cutscene '{cutsceneName}' marked as skipped and cached after {attempts} attempts.");
            }
        }


        public void ReplaceBoolArrayInFile(string filePath, bool[] newBoolArray)
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                List<string> modifiedLines = new List<string>();

                for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the first line
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]) && lines[i].Contains(','))
                    {
                        string[] parts = lines[i].Split(',');
                        if (parts.Length >= 2)
                        {
                            // Replace the existing bool value with the new one
                            parts[1] = newBoolArray[i - 1].ToString();
                            modifiedLines.Add(string.Join(",", parts));
                        }
                    }
                }

                // Write the modified lines back to the file, including the first line
                File.WriteAllLines(filePath, new[] { lines[0] }.Concat(modifiedLines).ToArray());
            }
        }


        public void Update()
        {
            if(IsConnectedToLivesplit || debug)
            {
                UpdateAutosplitter();
            }
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
