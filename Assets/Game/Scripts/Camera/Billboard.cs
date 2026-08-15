using UnityEngine;

namespace baodeag.Game
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private void LateUpdate()
        {
            Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
            if (cameraToUse == null)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(transform.position - cameraToUse.transform.position, Vector3.up);
        }
    }
}
