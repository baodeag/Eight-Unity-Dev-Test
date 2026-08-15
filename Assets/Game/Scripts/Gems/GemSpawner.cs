using UnityEngine;

namespace baodeag.Game
{
    public class GemSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GemPool gemPool;
        [SerializeField] private GemFactory gemFactory;
        [SerializeField] private BoxCollider spawnArea;
        [SerializeField] private Transform player;

        [Header("Spawn")]
        [SerializeField] private float spawnInterval = 1.25f;
        [SerializeField] private int maxActiveGems = 12;
        [SerializeField] private float minDistanceFromPlayer = 2f;
        [SerializeField] private int maxSpawnAttempts = 20;
        [SerializeField] private float spawnRaycastHeight = 5f;
        [SerializeField] private float spawnRaycastDistance = 50f;
        [SerializeField] private float gemGroundOffset = 0.65f;
        [SerializeField] private LayerMask groundLayers = ~0;

        private float spawnTimer;
        private float minSqrDistanceFromPlayer;
        private bool spawningEnabled;

        private void Awake()
        {
            CacheDerivedValues();
        }

        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.05f, spawnInterval);
            maxActiveGems = Mathf.Max(1, maxActiveGems);
            maxSpawnAttempts = Mathf.Max(1, maxSpawnAttempts);
            spawnRaycastDistance = Mathf.Max(0.1f, spawnRaycastDistance);
            CacheDerivedValues();
        }

        private void Update()
        {
            if (!spawningEnabled || gemPool.ActiveCount >= maxActiveGems)
            {
                return;
            }

            spawnTimer += Time.deltaTime;
            if (spawnTimer < spawnInterval)
            {
                return;
            }

            spawnTimer = 0f;
            SpawnGem();
        }

        public void SetSpawningEnabled(bool enabled)
        {
            spawningEnabled = enabled;
            spawnTimer = enabled ? spawnInterval : 0f;
        }

        private void SpawnGem()
        {
            if (!TryGetSpawnPosition(out Vector3 position))
            {
                return;
            }

            Gem gem = gemPool.GetGem();
            if (gem == null)
            {
                return;
            }

            gem.Setup(gemFactory.GetRandomGemType(), position + Vector3.up * gemGroundOffset);
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            if (spawnArea == null)
            {
                position = Vector3.zero;
                return false;
            }

            Bounds bounds = spawnArea.bounds;
            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.max.y + spawnRaycastHeight,
                    Random.Range(bounds.min.z, bounds.max.z));

                if (player != null && GetPlanarSqrDistance(candidate, player.position) < minSqrDistanceFromPlayer)
                {
                    continue;
                }

                if (Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, spawnRaycastDistance, groundLayers, QueryTriggerInteraction.Ignore))
                {
                    position = hit.point;
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (spawnArea == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }

        private void CacheDerivedValues()
        {
            minSqrDistanceFromPlayer = minDistanceFromPlayer * minDistanceFromPlayer;
        }

        private static float GetPlanarSqrDistance(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x;
            float z = a.z - b.z;
            return x * x + z * z;
        }
    }
}
