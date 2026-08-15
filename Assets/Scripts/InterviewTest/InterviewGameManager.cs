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
        [SerializeField] private ParticleSystem winParticlePrefab;

        [Header("Win VFX")]
        [SerializeField] private int winParticleBurstCount = 5;
        [SerializeField] private float winParticleSpacing = 1.6f;
        [SerializeField] private Vector3 winParticleOffset = new Vector3(0f, 1.6f, 0f);

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

            if (InterviewScoreManager.instance != null && InterviewScoreManager.instance.CurrentScore >= InterviewScoreManager.instance.TargetScore)
            {
                HandleTargetScoreReached();
            }
        }

        private void OnDisable()
        {
            if (InterviewScoreManager.instance != null)
            {
                InterviewScoreManager.instance.OnTargetScoreReached -= HandleTargetScoreReached;
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

        private void HandleTargetScoreReached()
        {
            CurrentState = InterviewGameState.Win;
            SetGameplayEnabled(false);
            uiManager.ShowWinState();

            if (winParticle != null)
            {
                winParticle.gameObject.SetActive(true);
                PlayParticleSystemTree(winParticle);
            }

            SpawnExtraWinParticles();
        }

        private void SpawnExtraWinParticles()
        {
            ParticleSystem prefab = winParticlePrefab != null ? winParticlePrefab : winParticle;
            if (prefab == null)
            {
                return;
            }

            Vector3 center = GetWinParticleCenter();
            for (int i = 0; i < winParticleBurstCount; i++)
            {
                float angle = winParticleBurstCount <= 1 ? 0f : (360f / winParticleBurstCount) * i;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * winParticleSpacing;
                ParticleSystem particle = Instantiate(prefab, center + offset, Quaternion.Euler(0f, angle, 0f));
                particle.name = $"Win Confetti Burst {i + 1}";
                particle.gameObject.SetActive(true);
                PlayParticleSystemTree(particle);
                Destroy(particle.gameObject, 6f);
            }
        }

        private Vector3 GetWinParticleCenter()
        {
            if (playerController != null)
            {
                return playerController.transform.position + winParticleOffset;
            }

            if (cameraController != null)
            {
                return cameraController.transform.position + cameraController.transform.forward * 4f;
            }

            return Vector3.up * 2f;
        }

        private static void PlayParticleSystemTree(ParticleSystem particle)
        {
            ParticleSystem[] systems = particle.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].gameObject.SetActive(true);
                systems[i].Clear(true);
                systems[i].Play(true);
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

            InterviewScoreManager.instance.OnTargetScoreReached += HandleTargetScoreReached;
            subscribedToScore = true;
        }
    }
}
