using Reptile;
using System.Reflection;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace SpeedrunUtils
{
    internal class DoAutoMash : MonoBehaviour
    {
        public static DoAutoMash Instance;

        public DoAutoMash()
        {
            Instance = this;
        }

        SequenceHandler seqHandler;
        SequenceState sequenceState;
        UIManager uiManager;
        DialogueUI dialogueUI;
        WorldHandler worldHandler;
        Player player;
        bool disabledExit;
        AudioManager audioManager;
        MethodInfo PlaySfxUI;

        public bool autoMash = true;

        private void Update()
        {
            if (Core.Instance != null)
            {
                if (worldHandler == null) { worldHandler = WorldHandler.instance; }
                if (player == null && worldHandler != null) { player = WorldHandler.instance.GetCurrentPlayer(); }
                if (player != null) { sequenceState = (SequenceState)typeof(Player).GetField("sequenceState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); }
                if (seqHandler == null && worldHandler != null) { seqHandler = (SequenceHandler)typeof(WorldHandler).GetField("sequenceHandler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldHandler); }
                if (audioManager == null && seqHandler != null) { audioManager = (AudioManager)typeof(SequenceHandler).GetField("audioManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(seqHandler); }
                if (PlaySfxUI == null && audioManager != null) { PlaySfxUI = typeof(AudioManager).GetMethod("PlaySfxUI", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SfxCollectionID), typeof(AudioClipID), typeof(float) }, null); }
                if (uiManager == null) { uiManager = Core.Instance.UIManager; }
                if (dialogueUI == null && uiManager != null) { dialogueUI = (DialogueUI)typeof(UIManager).GetField("dialogueUI", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uiManager); }

                if (seqHandler != null && dialogueUI != null && audioManager != null && autoMash)
                {
                    if (sequenceState == SequenceState.IN_SEQUENCE)
                    {
                        FieldInfo skipTextActiveStateField = typeof(SequenceHandler).GetField("skipTextActiveState", BindingFlags.NonPublic | BindingFlags.Instance);
                        disabledExit = (bool)typeof(SequenceHandler).GetField("disabledExit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(seqHandler);
                        bool flag = seqHandler.IsEnabled && !disabledExit && skipTextActiveStateField.GetValue(seqHandler).ToString() == "NOT_SKIPPABLE";
                        if (flag && dialogueUI.CanBeSkipped && !dialogueUI.isYesNoPromptEnabled)
                        {
                            if (dialogueUI.ReadyToResume)
                            {
                                seqHandler.ResumeSequence();
                                if (dialogueUI.IsShowingDialogue() && PlaySfxUI != null)
                                {
                                    PlaySfxUI.Invoke(audioManager, new object[] { SfxCollectionID.MenuSfx, AudioClipID.dialogueconfirm, 0f });
                                }
                                dialogueUI.EndDialogue();
                            }
                            else
                            {
                                typeof(SequenceHandler).GetMethod("FastForwardTypewriter", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(seqHandler, new object[] { });
                            }
                        }
                    }
                }
            }
        }
    }
}