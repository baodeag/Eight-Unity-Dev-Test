using System.Collections;
using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewGem : MonoBehaviour
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

        [Header("Collect Motion")]
        [SerializeField] private float collectDuration = 0.65f;
        [SerializeField] private AnimationCurve collectCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public bool IsCollected { get; private set; }

        private InterviewGemPool pool;
        private InterviewGemType gemType;
        private Vector3 spawnPosition;
        private Material runtimeMaterial;
        private Coroutine collectRoutine;

        private void Awake()
        {
            ResolveReferences();
            ResetVisualTransform();
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

        public void InitializePool(InterviewGemPool ownerPool)
        {
            pool = ownerPool;
        }

        public void Setup(InterviewGemType type, Vector3 position)
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
            Vector3 targetPosition = InterviewUIManager.instance != null
                ? InterviewUIManager.instance.GetGemIconWorldPosition()
                : startPosition + Vector3.up * 2f;
            Vector3 controlPosition = (startPosition + targetPosition) * 0.5f + Vector3.up * 2.25f;

            for (float time = 0f; time < collectDuration; time += Time.deltaTime)
            {
                float t = collectCurve.Evaluate(time / collectDuration);
                Vector3 firstLeg = Vector3.Lerp(startPosition, controlPosition, t);
                Vector3 secondLeg = Vector3.Lerp(controlPosition, targetPosition, t);
                transform.position = Vector3.Lerp(firstLeg, secondLeg, t);
                transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.25f, t);
                yield return null;
            }

            int score = gemType != null ? gemType.scoreValue : 1;
            if (InterviewScoreManager.instance != null)
            {
                InterviewScoreManager.instance.AddCollectedGem(score);
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

        private void ApplyTypeVisual(InterviewGemType type)
        {
            if (type == null)
            {
                return;
            }

            if (gemRenderer != null && type.material != null)
            {
                if (runtimeMaterial != null)
                {
                    Destroy(runtimeMaterial);
                }

                runtimeMaterial = new Material(type.material);
                if (runtimeMaterial.HasProperty("_BaseColor"))
                {
                    runtimeMaterial.SetColor("_BaseColor", type.lightColor);
                }

                if (runtimeMaterial.HasProperty("_EmissionColor"))
                {
                    runtimeMaterial.EnableKeyword("_EMISSION");
                    runtimeMaterial.SetColor("_EmissionColor", type.lightColor * 1.8f);
                }

                gemRenderer.sharedMaterial = runtimeMaterial;
            }

            if (gemLight != null)
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
            visualRoot.localScale = Vector3.one * 0.75f;
        }
    }
}
