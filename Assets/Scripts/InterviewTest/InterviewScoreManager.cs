using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace baodeag.InterviewTest
{
    public class InterviewScoreManager : MonoBehaviour
    {
        public static InterviewScoreManager instance;

        [Header("Win Condition")]
        [FormerlySerializedAs("winScore")]
        [FormerlySerializedAs("targetGemCount")]
        [SerializeField] private int targetScore = 10;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnGemCountChanged;
        public event Action OnTargetScoreReached;

        public int CurrentScore { get; private set; }
        public int CurrentGemCount { get; private set; }
        public int TargetScore => targetScore;

        private bool winRaised;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CurrentScore = InterviewSaveManager.LoadScore();
            CurrentGemCount = InterviewSaveManager.LoadGemCount();
        }

        private void Start()
        {
            OnScoreChanged?.Invoke(CurrentScore);
            OnGemCountChanged?.Invoke(CurrentGemCount);
            CheckWin();
        }

        public void AddCollectedGem(int scoreValue)
        {
            if (scoreValue <= 0 || winRaised)
            {
                return;
            }

            CurrentScore += scoreValue;
            CurrentGemCount++;
            InterviewSaveManager.SaveProgress(CurrentScore, CurrentGemCount);
            OnScoreChanged?.Invoke(CurrentScore);
            OnGemCountChanged?.Invoke(CurrentGemCount);
            CheckWin();
        }

        public void ResetScore()
        {
            winRaised = false;
            CurrentScore = 0;
            CurrentGemCount = 0;
            InterviewSaveManager.ClearScore();
            OnScoreChanged?.Invoke(CurrentScore);
            OnGemCountChanged?.Invoke(CurrentGemCount);
        }

        private void CheckWin()
        {
            if (winRaised || CurrentScore < targetScore)
            {
                return;
            }

            winRaised = true;
            OnTargetScoreReached?.Invoke();
        }
    }
}
