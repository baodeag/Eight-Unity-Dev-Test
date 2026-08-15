using System;
using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewScoreManager : MonoBehaviour
    {
        public static InterviewScoreManager instance;

        [Header("Score")]
        [SerializeField] private int winScore = 10;

        public event Action<int> OnScoreChanged;
        public event Action OnWinScoreReached;

        public int CurrentScore { get; private set; }
        public int WinScore => winScore;

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
        }

        private void Start()
        {
            OnScoreChanged?.Invoke(CurrentScore);
            CheckWin();
        }

        public void AddScore(int amount)
        {
            if (amount <= 0 || winRaised)
            {
                return;
            }

            CurrentScore += amount;
            InterviewSaveManager.SaveScore(CurrentScore);
            OnScoreChanged?.Invoke(CurrentScore);
            CheckWin();
        }

        public void ResetScore()
        {
            winRaised = false;
            CurrentScore = 0;
            InterviewSaveManager.ClearScore();
            OnScoreChanged?.Invoke(CurrentScore);
        }

        private void CheckWin()
        {
            if (winRaised || CurrentScore < winScore)
            {
                return;
            }

            winRaised = true;
            OnWinScoreReached?.Invoke();
        }
    }
}
