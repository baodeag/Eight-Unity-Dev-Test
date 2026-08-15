using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace baodeag.Game
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;

        [Header("Follow")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 4.4f, -9f);
        [SerializeField] private float followSmoothTime = 0.08f;
        [SerializeField] private float rotationSensitivity = 0.18f;
        [SerializeField] private float verticalSensitivity = 0.18f;
        [SerializeField] private float minPitch = -10f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float pointerDeltaSqrThreshold = 0.01f;
        [SerializeField] private float pivotHeight = 1.2f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private float collisionPadding = 0.15f;
        [SerializeField] private float minCollisionDistance = 0.5f;
        [SerializeField] private float collisionCheckMinDistance = 0.01f;

        public Quaternion PlanarRotation => Quaternion.Euler(0f, yaw, 0f);

        private Vector3 followVelocity;
        private float yaw;
        private float pitch = 18f;
        private bool controlEnabled;
        private bool followEnabled = true;

        private void OnValidate()
        {
            followSmoothTime = Mathf.Max(0f, followSmoothTime);
            pointerDeltaSqrThreshold = Mathf.Max(0f, pointerDeltaSqrThreshold);
            pivotHeight = Mathf.Max(0f, pivotHeight);
            collisionRadius = Mathf.Max(0f, collisionRadius);
            collisionPadding = Mathf.Max(0f, collisionPadding);
            minCollisionDistance = Mathf.Max(0f, minCollisionDistance);
            collisionCheckMinDistance = Mathf.Max(0f, collisionCheckMinDistance);

            if (maxPitch < minPitch)
            {
                maxPitch = minPitch;
            }
        }

        private void Start()
        {
            if (target != null)
            {
                Vector3 flatForward = target.forward;
                flatForward.y = 0f;
                yaw = Quaternion.LookRotation(flatForward.sqrMagnitude > 0.01f ? flatForward : Vector3.forward).eulerAngles.y;
            }
        }

        private void LateUpdate()
        {
            if (target == null || !followEnabled)
            {
                return;
            }

            HandleSwipeRotation();
            FollowTarget();
        }

        public void SetCameraControlEnabled(bool enabled)
        {
            controlEnabled = enabled;
        }

        public void SetFollowEnabled(bool enabled)
        {
            followEnabled = enabled;
            if (!enabled)
            {
                followVelocity = Vector3.zero;
            }
        }

        public void SnapBehindTarget()
        {
            if (target == null)
            {
                return;
            }

            yaw = target.eulerAngles.y;
            followVelocity = Vector3.zero;
            FollowTarget(true);
        }

        private void HandleSwipeRotation()
        {
#if ENABLE_INPUT_SYSTEM
            if (!controlEnabled)
            {
                return;
            }

            Vector2 pointerDelta = Vector2.zero;
            int pointerId = -1;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                pointerDelta = Touchscreen.current.primaryTouch.delta.ReadValue();
                pointerId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                pointerDelta = Mouse.current.delta.ReadValue();
            }

            if (pointerDelta.sqrMagnitude <= pointerDeltaSqrThreshold)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
            {
                return;
            }

            ApplyOrbitDelta(pointerDelta);
#else
            if (!controlEnabled || Input.touchCount == 0)
            {
                return;
            }

            Touch touch = Input.GetTouch(0);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                ApplyOrbitDelta(touch.deltaPosition);
            }
#endif
        }

        private void ApplyOrbitDelta(Vector2 pointerDelta)
        {
            yaw += pointerDelta.x * rotationSensitivity;
            pitch = Mathf.Clamp(pitch - pointerDelta.y * verticalSensitivity, minPitch, maxPitch);
        }

        private void FollowTarget(bool snap = false)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Vector3 desiredPosition = pivot + rotation * followOffset;
            Vector3 correctedPosition = ResolveCollision(pivot, desiredPosition);

            transform.position = snap
                ? correctedPosition
                : Vector3.SmoothDamp(transform.position, correctedPosition, ref followVelocity, followSmoothTime);
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        }

        private Vector3 ResolveCollision(Vector3 pivot, Vector3 desiredPosition)
        {
            Vector3 toCamera = desiredPosition - pivot;
            float distance = toCamera.magnitude;

            if (distance <= collisionCheckMinDistance || !Physics.SphereCast(pivot, collisionRadius, toCamera.normalized, out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                return desiredPosition;
            }

            return pivot + toCamera.normalized * Mathf.Max(minCollisionDistance, hit.distance - collisionPadding);
        }
    }
}
