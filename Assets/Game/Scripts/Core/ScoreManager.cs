using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace baodeag.Game
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;

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

        private void OnValidate()
        {
            targetScore = Mathf.Max(1, targetScore);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            CurrentScore = SaveManager.LoadScore();
            CurrentGemCount = SaveManager.LoadGemCount();
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
            SaveManager.SaveProgress(CurrentScore, CurrentGemCount);
            OnScoreChanged?.Invoke(CurrentScore);
            OnGemCountChanged?.Invoke(CurrentGemCount);
            CheckWin();
        }

        public void ResetScore()
        {
            winRaised = false;
            CurrentScore = 0;
            CurrentGemCount = 0;
            SaveManager.ClearScore();
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
