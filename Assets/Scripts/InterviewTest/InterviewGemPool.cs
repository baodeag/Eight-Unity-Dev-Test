using System.Collections.Generic;
using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewGemPool : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private InterviewGem gemPrefab;
        [SerializeField] private int initialSize = 16;

        private readonly Queue<InterviewGem> availableGems = new Queue<InterviewGem>();
        private readonly List<InterviewGem> activeGems = new List<InterviewGem>();
        private InterviewGem runtimeGemPrefab;

        public int ActiveCount => activeGems.Count;

        private void Awake()
        {
            if (gemPrefab == null)
            {
                runtimeGemPrefab = CreateRuntimeGemTemplate();
            }

            for (int i = 0; i < initialSize; i++)
            {
                CreateGem();
            }
        }

        public InterviewGem GetGem()
        {
            if (GetGemSource() == null)
            {
                Debug.LogError($"{nameof(InterviewGemPool)} cannot create a gem because no prefab or runtime template is available.", this);
                return null;
            }

            InterviewGem gem = availableGems.Count > 0 ? availableGems.Dequeue() : CreateGem(false);
            activeGems.Add(gem);
            gem.gameObject.SetActive(true);
            return gem;
        }

        public void ReturnGem(InterviewGem gem)
        {
            if (gem == null)
            {
                return;
            }

            activeGems.Remove(gem);
            gem.gameObject.SetActive(false);
            gem.transform.SetParent(transform);
            availableGems.Enqueue(gem);
        }

        public void ReturnAll()
        {
            for (int i = activeGems.Count - 1; i >= 0; i--)
            {
                ReturnGem(activeGems[i]);
            }
        }

        private InterviewGem CreateGem(bool addToAvailable = true)
        {
            InterviewGem gem = Instantiate(GetGemSource(), transform);
            gem.InitializePool(this);
            gem.gameObject.SetActive(false);
            if (addToAvailable)
            {
                availableGems.Enqueue(gem);
            }
            return gem;
        }

        private InterviewGem GetGemSource()
        {
            return gemPrefab != null ? gemPrefab : runtimeGemPrefab;
        }

        private InterviewGem CreateRuntimeGemTemplate()
        {
            GameObject root = new GameObject("Runtime Interview Gem Template");
            root.layer = LayerMask.NameToLayer("Gem");
            root.transform.SetParent(transform);
            root.SetActive(false);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.layer = root.layer;
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = Vector3.one * 0.65f;

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            SphereCollider gemCollider = root.AddComponent<SphereCollider>();
            gemCollider.isTrigger = true;
            gemCollider.radius = 0.7f;

            Light gemLight = root.AddComponent<Light>();
            gemLight.type = LightType.Point;
            gemLight.range = 4f;
            gemLight.intensity = 1.8f;
            gemLight.color = Color.cyan;

            InterviewGem gem = root.AddComponent<InterviewGem>();
            gem.InitializePool(this);
            AssignRuntimeReference(gem, "visualRoot", visual.transform);
            AssignRuntimeReference(gem, "gemRenderer", visual.GetComponent<Renderer>());
            AssignRuntimeReference(gem, "gemLight", gemLight);
            AssignRuntimeReference(gem, "gemCollider", gemCollider);
            return gem;
        }

        private static void AssignRuntimeReference(Object target, string fieldName, Object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }
    }
}
