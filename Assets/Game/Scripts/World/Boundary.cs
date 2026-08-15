using UnityEngine;

namespace baodeag.Game
{
    public class Boundary : MonoBehaviour
    {
        [Header("Bounds")]
        [SerializeField] private Vector2 xRange = new Vector2(-14f, 14f);
        [SerializeField] private Vector2 zRange = new Vector2(-14f, 14f);

        public Vector3 ClampPosition(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, xRange.x, xRange.y);
            position.z = Mathf.Clamp(position.z, zRange.x, zRange.y);
            return position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((xRange.x + xRange.y) * 0.5f, 0.1f, (zRange.x + zRange.y) * 0.5f);
            Vector3 size = new Vector3(xRange.y - xRange.x, 0.2f, zRange.y - zRange.x);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
