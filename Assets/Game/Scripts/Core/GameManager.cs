using UnityEngine;
using UnityEngine.SceneManagement;

namespace baodeag.Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private IntroCameraSequence introCameraSequence;
        [SerializeField] private GemSpawner gemSpawner;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ParticleSystem winParticle;
        [SerializeField] private ParticleSystem winParticlePrefab;

        [Header("Win VFX")]
        [SerializeField] private int winParticleBurstCount = 5;
        [SerializeField] private float winParticleSpacing = 1.6f;
        [SerializeField] private Vector3 winParticleOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField] private float winParticleLifetime = 6f;
        [SerializeField] private float cameraFallbackDistance = 4f;
        [SerializeField] private float worldFallbackHeight = 2f;

        public GameState CurrentState { get; private set; } = GameState.WaitingToStart;
        public bool IsGameplayActive => CurrentState == GameState.Playing;
        private bool subscribedToScore;

        private void OnValidate()
        {
            winParticleBurstCount = Mathf.Max(0, winParticleBurstCount);
            winParticleSpacing = Mathf.Max(0f, winParticleSpacing);
            winParticleLifetime = Mathf.Max(0f, winParticleLifetime);
            cameraFallbackDistance = Mathf.Max(0f, cameraFallbackDistance);
            worldFallbackHeight = Mathf.Max(0f, worldFallbackHeight);
        }

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
            if (uiManager != null)
            {
                uiManager.ShowWaitingState();
            }

            SubscribeToScore();

            if (ScoreManager.instance != null && ScoreManager.instance.CurrentScore >= ScoreManager.instance.TargetScore)
            {
                HandleTargetScoreReached();
            }
        }

        private void OnDisable()
        {
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.OnTargetScoreReached -= HandleTargetScoreReached;
            }

            subscribedToScore = false;
        }

        public void StartGame()
        {
            if (CurrentState != GameState.WaitingToStart)
            {
                return;
            }

            CurrentState = GameState.Intro;
            if (uiManager != null)
            {
                uiManager.ShowIntroState();
            }

            SetGameplayEnabled(false);
            if (introCameraSequence != null)
            {
                introCameraSequence.PlayIntro(BeginPlaying);
            }
            else
            {
                BeginPlaying();
            }
        }

        public void BeginPlaying()
        {
            if (CurrentState == GameState.Win)
            {
                return;
            }

            CurrentState = GameState.Playing;
            if (cameraController != null)
            {
                cameraController.SetCameraControlEnabled(true);
            }

            SetGameplayEnabled(true);
            if (uiManager != null)
            {
                uiManager.ShowPlayingState();
            }
        }

        public void ResetGame()
        {
            SaveManager.ClearScore();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleTargetScoreReached()
        {
            CurrentState = GameState.Win;
            SetGameplayEnabled(false);
            if (uiManager != null)
            {
                uiManager.ShowWinState();
            }

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
                Destroy(particle.gameObject, winParticleLifetime);
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
                return cameraController.transform.position + cameraController.transform.forward * cameraFallbackDistance;
            }

            return Vector3.up * worldFallbackHeight;
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
            if (subscribedToScore || ScoreManager.instance == null)
            {
                return;
            }

            ScoreManager.instance.OnTargetScoreReached += HandleTargetScoreReached;
            subscribedToScore = true;
        }
    }
}
