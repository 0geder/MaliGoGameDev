using System;
using UnityEngine;

namespace MaliGo.Characters
{
    /// <summary>
    /// Foundation for Mali dialogue. Future systems trigger lines via the public entry points.
    /// </summary>
    public class MaliDialogueController : MonoBehaviour
    {
        [SerializeField] MaliDialogueView dialogueView;
        [SerializeField] MaliNpcController maliController;

        public event Action<string> OnDialogueShown;
        public event Action OnDialogueHidden;

        public bool IsShowingDialogue { get; private set; }

        void Awake()
        {
            if (dialogueView == null)
            {
                dialogueView = GetComponent<MaliDialogueView>();
            }

            if (dialogueView == null)
            {
                dialogueView = gameObject.AddComponent<MaliDialogueView>();
            }

            if (maliController == null)
            {
                maliController = GetComponent<MaliNpcController>();
            }

            dialogueView.HideImmediate();
        }

        public void ShowLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            IsShowingDialogue = true;
            maliController?.SetBehaviourState(MaliBehaviourState.TALK);
            dialogueView.Show(line);
            OnDialogueShown?.Invoke(line);
        }

        public void ShowFormatted(string template, params object[] args)
        {
            ShowLine(string.Format(template, args));
        }

        public void ShowGreeting()
        {
            var player = PlayerIdentity.PlayerDataAccess.GetCurrentPlayer();
            var entry = Dialogue.MaliContextualDialogueSelector.SelectLine(player);
            string name = PlayerIdentity.PlayerDataAccess.GetCharacterName();

            if (entry == null)
            {
                ShowFormatted("Hey, {0}! Ready to see what today has in store?", name);
                return;
            }

            ShowFormatted(entry.line, name);

            if (entry.triggerType == Dialogue.MaliDialogueTriggerType.FirstMeeting)
            {
                MarkMetMali();
            }
        }

        static void MarkMetMali()
        {
            if (PlayerIdentity.PlayerDataManager.Instance == null)
            {
                return;
            }

            PlayerIdentity.PlayerDataManager.Instance.UpdatePlayerData(data => data.hasMetMali = true, saveImmediately: true);
        }

        public void Hide()
        {
            IsShowingDialogue = false;
            dialogueView.Hide();

            if (maliController != null && maliController.CurrentState == MaliBehaviourState.TALK)
            {
                MaliBehaviourState resumeState = maliController.FollowPlayerEnabled
                    ? MaliBehaviourState.FOLLOW
                    : MaliBehaviourState.IDLE;
                maliController.SetBehaviourState(resumeState);
            }

            OnDialogueHidden?.Invoke();
        }

        // --- Future trigger entry points (stub implementations) ---

        public void TriggerFromArea(string areaId)
        {
            Debug.Log($"[MaliDialogue] Area trigger reserved for future use: {areaId}");
        }

        public void TriggerFromInteraction(string interactionId)
        {
            Debug.Log($"[MaliDialogue] Interaction trigger reserved for future use: {interactionId}");
        }

        public void TriggerFromFinancialEvent(string eventId)
        {
            Debug.Log($"[MaliDialogue] Financial event trigger reserved for future use: {eventId}");
        }

        public void TriggerFromQuest(string questId)
        {
            Debug.Log($"[MaliDialogue] Quest trigger reserved for future use: {questId}");
        }

        public void TriggerFromLifeChapterTransition(string chapterId)
        {
            Debug.Log($"[MaliDialogue] Life chapter trigger reserved for future use: {chapterId}");
        }

        public void TriggerFromFinancialBehaviour(string behaviourId)
        {
            Debug.Log($"[MaliDialogue] Financial behaviour trigger reserved for future use: {behaviourId}");
        }
    }
}
