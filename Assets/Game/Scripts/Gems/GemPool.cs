using System.Collections.Generic;
using UnityEngine;

namespace baodeag.Game
{
    public class GemPool : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private Gem gemPrefab;
        [SerializeField] private int initialSize = 16;

        private readonly Queue<Gem> availableGems = new Queue<Gem>();
        private readonly List<Gem> activeGems = new List<Gem>();
        private Gem runtimeGemPrefab;

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

        public Gem GetGem()
        {
            if (GetGemSource() == null)
            {
                Debug.LogError($"{nameof(GemPool)} cannot create a gem because no prefab or runtime template is available.", this);
                return null;
            }

            Gem gem = availableGems.Count > 0 ? availableGems.Dequeue() : CreateGem(false);
            activeGems.Add(gem);
            gem.gameObject.SetActive(true);
            return gem;
        }

        public void ReturnGem(Gem gem)
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

        private Gem CreateGem(bool addToAvailable = true)
        {
            Gem gem = Instantiate(GetGemSource(), transform);
            gem.InitializePool(this);
            gem.gameObject.SetActive(false);
            if (addToAvailable)
            {
                availableGems.Enqueue(gem);
            }
            return gem;
        }

        private Gem GetGemSource()
        {
            return gemPrefab != null ? gemPrefab : runtimeGemPrefab;
        }

        private Gem CreateRuntimeGemTemplate()
        {
            GameObject root = new GameObject("Runtime Gem Template");
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

            Gem gem = root.AddComponent<Gem>();
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
