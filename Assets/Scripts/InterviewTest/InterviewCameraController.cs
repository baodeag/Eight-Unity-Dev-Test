using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace baodeag.InterviewTest
{
    public class InterviewCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform target;

        [Header("Follow")]
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 4.4f, -9f);
        [SerializeField] private float followSmoothTime = 0.08f;
        [SerializeField] private float rotationSensitivity = 0.18f;
        [SerializeField] private float minPitch = 8f;
        [SerializeField] private float maxPitch = 40f;

        [Header("Collision")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float collisionRadius = 0.2f;
        [SerializeField] private float collisionPadding = 0.15f;

        public Quaternion PlanarRotation => Quaternion.Euler(0f, yaw, 0f);

        private Vector3 followVelocity;
        private float yaw;
        private float pitch = 18f;
        private bool controlEnabled;

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
            if (target == null)
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

        public void SnapBehindTarget()
        {
            if (target == null)
            {
                return;
            }

            yaw = target.eulerAngles.y;
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

            if (pointerDelta.sqrMagnitude <= 0.01f)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId))
            {
                return;
            }

            yaw += pointerDelta.x * rotationSensitivity;
            pitch = Mathf.Clamp(pitch - pointerDelta.y * rotationSensitivity * 0.35f, minPitch, maxPitch);
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
                yaw += touch.deltaPosition.x * rotationSensitivity;
                pitch = Mathf.Clamp(pitch - touch.deltaPosition.y * rotationSensitivity * 0.35f, minPitch, maxPitch);
            }
#endif
        }

        private void FollowTarget(bool snap = false)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 pivot = target.position + Vector3.up * 1.2f;
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

            if (distance <= 0.01f || !Physics.SphereCast(pivot, collisionRadius, toCamera.normalized, out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                return desiredPosition;
            }

            return pivot + toCamera.normalized * Mathf.Max(0.5f, hit.distance - collisionPadding);
        }
    }
}
