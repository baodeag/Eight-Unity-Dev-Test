using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewGemSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InterviewGemPool gemPool;
        [SerializeField] private InterviewGemFactory gemFactory;
        [SerializeField] private BoxCollider spawnArea;
        [SerializeField] private Transform player;

        [Header("Spawn")]
        [SerializeField] private float spawnInterval = 1.25f;
        [SerializeField] private int maxActiveGems = 12;
        [SerializeField] private float minDistanceFromPlayer = 2f;
        [SerializeField] private LayerMask groundLayers = ~0;

        private float spawnTimer;
        private bool spawningEnabled;

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

            InterviewGem gem = gemPool.GetGem();
            if (gem == null)
            {
                return;
            }

            gem.Setup(gemFactory.GetRandomGemType(), position + Vector3.up * 0.65f);
        }

        private bool TryGetSpawnPosition(out Vector3 position)
        {
            Bounds bounds = spawnArea.bounds;
            for (int i = 0; i < 20; i++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    bounds.max.y + 5f,
                    Random.Range(bounds.min.z, bounds.max.z));

                if (player != null && Vector3.Distance(new Vector3(candidate.x, player.position.y, candidate.z), player.position) < minDistanceFromPlayer)
                {
                    continue;
                }

                if (Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, 50f, groundLayers, QueryTriggerInteraction.Ignore))
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
    }
}
