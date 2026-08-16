using System.Collections;
using UnityEngine;

namespace baodeag.Game
{
    public class Gem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer gemRenderer;
        [SerializeField] private Light gemLight;
        [SerializeField] private Collider gemCollider;
        [SerializeField] private Transform visualRoot;

        [Header("Idle Motion")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float bobHeight = 0.18f;
        [SerializeField] private float bobSpeed = 2.2f;
        [SerializeField] private float visualScale = 0.75f;

        [Header("Collect Motion")]
        [SerializeField] private float collectDuration = 0.65f;
        [SerializeField] private float collectArcHeight = 2.25f;
        [SerializeField] private float collectFallbackHeight = 2f;
        [SerializeField] private float collectEndScale = 0.25f;
        [SerializeField] private AnimationCurve collectCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Visual")]
        [SerializeField] private float emissionIntensity = 1.8f;

        public bool IsCollected { get; private set; }

        private GemPool pool;
        private GemType gemType;
        private Vector3 spawnPosition;
        private MaterialPropertyBlock materialPropertyBlock;
        private Coroutine collectRoutine;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            ResolveReferences();
            materialPropertyBlock = new MaterialPropertyBlock();
            ResetVisualTransform();
        }

        private void OnValidate()
        {
            bobHeight = Mathf.Max(0f, bobHeight);
            bobSpeed = Mathf.Max(0f, bobSpeed);
            visualScale = Mathf.Max(0.01f, visualScale);
            collectDuration = Mathf.Max(0.01f, collectDuration);
            collectArcHeight = Mathf.Max(0f, collectArcHeight);
            collectFallbackHeight = Mathf.Max(0f, collectFallbackHeight);
            collectEndScale = Mathf.Max(0f, collectEndScale);
            emissionIntensity = Mathf.Max(0f, emissionIntensity);
        }

        private void Update()
        {
            if (IsCollected)
            {
                return;
            }

            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = spawnPosition + Vector3.up * bob;
        }

        public void InitializePool(GemPool ownerPool)
        {
            pool = ownerPool;
        }

        public void Setup(GemType type, Vector3 position)
        {
            gemType = type;
            IsCollected = false;
            spawnPosition = position;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            ResetVisualTransform();

            if (gemCollider != null)
            {
                gemCollider.enabled = true;
            }

            ApplyTypeVisual(type);
        }

        public void Collect()
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            if (gemCollider != null)
            {
                gemCollider.enabled = false;
            }

            if (collectRoutine != null)
            {
                StopCoroutine(collectRoutine);
            }

            collectRoutine = StartCoroutine(CollectRoutine());
        }

        private IEnumerator CollectRoutine()
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = UIManager.instance != null
                ? UIManager.instance.GetGemIconWorldPosition()
                : startPosition + Vector3.up * collectFallbackHeight;
            Vector3 controlPosition = (startPosition + targetPosition) * 0.5f + Vector3.up * collectArcHeight;

            for (float time = 0f; time < collectDuration; time += Time.deltaTime)
            {
                float t = collectCurve.Evaluate(time / collectDuration);
                Vector3 firstLeg = Vector3.Lerp(startPosition, controlPosition, t);
                Vector3 secondLeg = Vector3.Lerp(controlPosition, targetPosition, t);
                transform.position = Vector3.Lerp(firstLeg, secondLeg, t);
                transform.localScale = Vector3.one * Mathf.Lerp(1f, collectEndScale, t);
                yield return null;
            }

            int score = gemType != null ? gemType.scoreValue : 1;
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddCollectedGem(score);
            }

            if (pool != null)
            {
                pool.ReturnGem(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void ApplyTypeVisual(GemType type)
        {
            if (gemRenderer != null)
            {
                gemRenderer.GetPropertyBlock(materialPropertyBlock);
                materialPropertyBlock.Clear();

                if (type != null && type.material != null)
                {
                    gemRenderer.sharedMaterial = type.material;
                }

                Material material = type != null ? type.material : gemRenderer.sharedMaterial;
                if (material != null && type != null && material.HasProperty(BaseColorId))
                {
                    materialPropertyBlock.SetColor(BaseColorId, type.lightColor);
                }

                if (material != null && type != null && material.HasProperty(EmissionColorId))
                {
                    materialPropertyBlock.SetColor(EmissionColorId, type.lightColor * emissionIntensity);
                }

                gemRenderer.SetPropertyBlock(materialPropertyBlock);
            }

            if (gemLight != null && type != null)
            {
                gemLight.color = type.lightColor;
            }
        }

        private void ResolveReferences()
        {
            if (visualRoot == null && transform.childCount > 0)
            {
                visualRoot = transform.GetChild(0);
            }

            if (gemRenderer == null)
            {
                gemRenderer = GetComponentInChildren<Renderer>(true);
            }

            if (gemLight == null)
            {
                gemLight = GetComponentInChildren<Light>(true);
            }

            if (gemCollider == null)
            {
                gemCollider = GetComponent<Collider>();
            }
        }

        private void ResetVisualTransform()
        {
            if (visualRoot == null)
            {
                return;
            }

            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one * visualScale;
        }
    }
}
