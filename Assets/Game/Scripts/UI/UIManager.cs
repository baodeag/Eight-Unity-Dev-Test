using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace baodeag.Game
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        [Header("HUD")]
        [FormerlySerializedAs("scoreText")]
        [SerializeField] private Text scoreValueText;
        [SerializeField] private RectTransform gemIconTarget;
        [SerializeField] private Canvas canvas;

        [Header("Controls")]
        [SerializeField] private GameObject controlsRoot;
        [SerializeField] private Button startButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject winPanel;

        [Header("Collection Target")]
        [SerializeField] private float gemIconProjectionDistance = 6f;
        [SerializeField] private int fallbackTargetScore = 10;

        private Camera mainCamera;
        private bool subscribedToScore;
        private int currentScore;
        private int currentGemCount;

        private void OnValidate()
        {
            gemIconProjectionDistance = Mathf.Max(0f, gemIconProjectionDistance);
            fallbackTargetScore = Mathf.Max(1, fallbackTargetScore);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            SubscribeToScore();

            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStartClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(HandleResetClicked);
            }
        }

        private void Start()
        {
            if (ScoreManager.instance != null)
            {
                SubscribeToScore();
                UpdateScore(ScoreManager.instance.CurrentScore);
                UpdateGemCount(ScoreManager.instance.CurrentGemCount);
            }
        }

        private void OnDisable()
        {
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.OnScoreChanged -= UpdateScore;
                ScoreManager.instance.OnGemCountChanged -= UpdateGemCount;
            }

            subscribedToScore = false;

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStartClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(HandleResetClicked);
            }
        }

        public void ShowWaitingState()
        {
            SetControlsActive(false);
            SetStartActive(true);
            SetWinActive(false);
        }

        public void ShowIntroState()
        {
            SetControlsActive(false);
            SetStartActive(false);
            SetWinActive(false);
        }

        public void ShowPlayingState()
        {
            SetControlsActive(true);
            SetStartActive(false);
            SetWinActive(false);
        }

        public void ShowWinState()
        {
            SetControlsActive(false);
            SetStartActive(false);
            SetWinActive(true);
        }

        public Vector3 GetGemIconWorldPosition()
        {
            if (gemIconTarget == null)
            {
                return Vector3.zero;
            }

            Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(canvas != null ? canvas.worldCamera : null, gemIconTarget.position);
            Camera cameraToUse = mainCamera != null ? mainCamera : Camera.main;
            if (cameraToUse == null)
            {
                return Vector3.zero;
            }

            Ray ray = cameraToUse.ScreenPointToRay(screenPosition);
            return ray.GetPoint(gemIconProjectionDistance);
        }

        private void UpdateScore(int score)
        {
            currentScore = score;
            RefreshHudText();
        }

        private void UpdateGemCount(int gemCount)
        {
            currentGemCount = gemCount;
            RefreshHudText();
        }

        private void RefreshHudText()
        {
            if (scoreValueText != null)
            {
                int targetScore = ScoreManager.instance != null ? ScoreManager.instance.TargetScore : fallbackTargetScore;
                scoreValueText.text = $"{currentScore}/{targetScore}";
            }
        }

        private void SubscribeToScore()
        {
            if (subscribedToScore || ScoreManager.instance == null)
            {
                return;
            }

            ScoreManager.instance.OnScoreChanged += UpdateScore;
            ScoreManager.instance.OnGemCountChanged += UpdateGemCount;
            subscribedToScore = true;
        }

        private void HandleStartClicked()
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.StartGame();
            }
        }

        private void HandleResetClicked()
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.ResetGame();
            }
        }

        private void SetControlsActive(bool active)
        {
            if (controlsRoot != null)
            {
                controlsRoot.SetActive(active);
            }
        }

        private void SetStartActive(bool active)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(active);
            }
        }

        private void SetWinActive(bool active)
        {
            if (winPanel != null)
            {
                winPanel.SetActive(active);
            }
        }
    }
}
