using Reptile;
using System.Reflection;
using UnityEngine;
using System;
using TMPro;

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
        TextMeshProUGUI textLabel;
        DialogueBehaviour currentDialogue;
        bool fastForwardTypewriter;

        public bool autoMash = true;

        private void Update()
        {
            if (Core.Instance != null)
            {
                if (worldHandler == null) { worldHandler = WorldHandler.instance; }
                if (player == null && worldHandler != null) { player = WorldHandler.instance.GetCurrentPlayer(); }
                if (player != null) { sequenceState = (SequenceState)typeof(Player).GetField("sequenceState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(player); }
                if (seqHandler == null && worldHandler != null) { seqHandler = (SequenceHandler)typeof(WorldHandler).GetField("sequenceHandler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldHandler); }
                if (uiManager == null && seqHandler != null) { uiManager = (UIManager)typeof(SequenceHandler).GetField("uIManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(seqHandler); ; }
                if (dialogueUI == null && uiManager != null) { dialogueUI = (DialogueUI)typeof(UIManager).GetField("dialogueUI", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(uiManager); }
                if (dialogueUI != null) { textLabel = (TextMeshProUGUI)typeof(DialogueUI).GetField("textLabel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialogueUI); }
                if (dialogueUI != null) { currentDialogue = (DialogueBehaviour)typeof(DialogueUI).GetField("currentDialogue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialogueUI); }
                if (dialogueUI != null) { fastForwardTypewriter = (bool)typeof(DialogueUI).GetField("fastForwardTypewriter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialogueUI); }
                if (audioManager == null && seqHandler != null) { audioManager = (AudioManager)typeof(SequenceHandler).GetField("audioManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(seqHandler); }
                if (PlaySfxUI == null && audioManager != null) { PlaySfxUI = typeof(AudioManager).GetMethod("PlaySfxUI", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(SfxCollectionID), typeof(AudioClipID), typeof(float) }, null); }

                if (seqHandler != null && dialogueUI != null && audioManager != null && worldHandler != null && textLabel != null && autoMash)
                {
                    SceneObjectsRegister sceneObjectsRegister = (SceneObjectsRegister)typeof(WorldHandler).GetField("sceneObjectsRegister", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(worldHandler);

                    if (sequenceState == SequenceState.IN_SEQUENCE && sceneObjectsRegister != null)
                    {
                        FieldInfo skipTextActiveStateField = typeof(SequenceHandler).GetField("skipTextActiveState", BindingFlags.NonPublic | BindingFlags.Instance);
                        disabledExit = (bool)typeof(SequenceHandler).GetField("disabledExit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(seqHandler);
                        bool flag = seqHandler.IsEnabled && !disabledExit && skipTextActiveStateField.GetValue(seqHandler).ToString() == "NOT_SKIPPABLE";
                        if (flag && dialogueUI.CanBeSkipped && !dialogueUI.isYesNoPromptEnabled)
                        {
                            if (currentDialogue != null && dialogueUI.ReadyToResume)
                            {
                                seqHandler.ResumeSequence();
                                if (dialogueUI.IsShowingDialogue() && PlaySfxUI != null)
                                {
                                    PlaySfxUI.Invoke(audioManager, new object[] { SfxCollectionID.MenuSfx, AudioClipID.dialogueconfirm, 0f });
                                }
                                dialogueUI.EndDialogue();
                            }
                            else if (currentDialogue != null && textLabel.maxVisibleCharacters > 1 && textLabel.textInfo.characterCount != textLabel.maxVisibleCharacters && !fastForwardTypewriter)
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