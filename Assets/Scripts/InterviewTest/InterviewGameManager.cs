using UnityEngine;
using UnityEngine.SceneManagement;

namespace baodeag.InterviewTest
{
    public class InterviewGameManager : MonoBehaviour
    {
        public static InterviewGameManager instance;

        [Header("References")]
        [SerializeField] private InterviewPlayerController playerController;
        [SerializeField] private InterviewCameraController cameraController;
        [SerializeField] private InterviewIntroCameraSequence introCameraSequence;
        [SerializeField] private InterviewGemSpawner gemSpawner;
        [SerializeField] private InterviewUIManager uiManager;
        [SerializeField] private ParticleSystem winParticle;

        public InterviewGameState CurrentState { get; private set; } = InterviewGameState.WaitingToStart;
        public bool IsGameplayActive => CurrentState == InterviewGameState.Playing;
        private bool subscribedToScore;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        private void OnEnable()
        {
            SubscribeToScore();
        }

        private void Start()
        {
            SetGameplayEnabled(false);
            uiManager.ShowWaitingState();
            SubscribeToScore();

            if (InterviewScoreManager.instance != null && InterviewScoreManager.instance.CurrentScore >= InterviewScoreManager.instance.WinScore)
            {
                HandleWinScoreReached();
            }
        }

        private void OnDisable()
        {
            if (InterviewScoreManager.instance != null)
            {
                InterviewScoreManager.instance.OnWinScoreReached -= HandleWinScoreReached;
            }

            subscribedToScore = false;
        }

        public void StartGame()
        {
            if (CurrentState != InterviewGameState.WaitingToStart)
            {
                return;
            }

            CurrentState = InterviewGameState.Intro;
            uiManager.ShowIntroState();
            SetGameplayEnabled(false);
            introCameraSequence.PlayIntro(BeginPlaying);
        }

        public void BeginPlaying()
        {
            if (CurrentState == InterviewGameState.Win)
            {
                return;
            }

            CurrentState = InterviewGameState.Playing;
            cameraController.SetCameraControlEnabled(true);
            SetGameplayEnabled(true);
            uiManager.ShowPlayingState();
        }

        public void ResetGame()
        {
            InterviewSaveManager.ClearScore();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleWinScoreReached()
        {
            CurrentState = InterviewGameState.Win;
            SetGameplayEnabled(false);
            uiManager.ShowWinState();

            if (winParticle != null)
            {
                winParticle.gameObject.SetActive(true);
                winParticle.Play(true);
            }
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.SetInputEnabled(enabled);
            }

            if (cameraController != null)
            {
                cameraController.SetCameraControlEnabled(enabled);
            }

            if (gemSpawner != null)
            {
                gemSpawner.SetSpawningEnabled(enabled);
            }
        }

        private void SubscribeToScore()
        {
            if (subscribedToScore || InterviewScoreManager.instance == null)
            {
                return;
            }

            InterviewScoreManager.instance.OnWinScoreReached += HandleWinScoreReached;
            subscribedToScore = true;
        }
    }
}
