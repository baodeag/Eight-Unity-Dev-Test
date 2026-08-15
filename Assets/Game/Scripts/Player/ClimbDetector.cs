using System.Collections;
using UnityEngine;

namespace baodeag.Game
{
    public class ClimbDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Animator animator;

        [Header("Detection")]
        [SerializeField] private LayerMask climbableLayers;
        [SerializeField] private float checkDistance = 0.9f;
        [SerializeField] private float checkHeight = 0.9f;
        [SerializeField] private float requiredInputDot = 0.6f;

        [Header("Motion")]
        [SerializeField] private float climbForward = 1.15f;
        [SerializeField] private float climbDuration = 0.95f;
        [SerializeField] private float topSurfacePadding = 0.08f;
        [SerializeField] private float landingClearancePadding = 0.08f;
        [SerializeField] private LayerMask blockingLayers = ~0;

        public bool IsClimbing { get; private set; }

        private static readonly int IsClimbingHash = Animator.StringToHash("isClimbing");
        private const string ClimbStateName = "Climb";

        public bool TryStartClimb(Vector3 moveDirection, Boundary boundary)
        {
            if (IsClimbing || moveDirection.sqrMagnitude < 0.1f)
            {
                return false;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (Vector3.Dot(forward.normalized, moveDirection.normalized) < requiredInputDot)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * checkHeight;
            if (!Physics.Raycast(origin, forward.normalized, out RaycastHit hit, checkDistance, climbableLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (!TryGetLandingPosition(hit, forward.normalized, out Vector3 target))
            {
                return false;
            }

            if (boundary != null)
            {
                target = boundary.ClampPosition(target);
            }

            StartCoroutine(ClimbRoutine(target));
            return true;
        }

        private IEnumerator ClimbRoutine(Vector3 target)
        {
            IsClimbing = true;
            animator.SetBool(IsClimbingHash, true);
            animator.CrossFade(ClimbStateName, 0.12f);
            Vector3 start = transform.position;

            for (float time = 0f; time < climbDuration; time += Time.deltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, time / climbDuration);
                Vector3 next = Vector3.Lerp(start, target, t);
                characterController.enabled = false;
                transform.position = next;
                characterController.enabled = true;
                yield return null;
            }

            characterController.enabled = false;
            transform.position = target;
            characterController.enabled = true;
            animator.SetBool(IsClimbingHash, false);
            IsClimbing = false;
        }

        private bool TryGetLandingPosition(RaycastHit hit, Vector3 forward, out Vector3 target)
        {
            Bounds bounds = hit.collider.bounds;
            Vector3 horizontalHit = hit.point;
            horizontalHit.y = 0f;

            target = horizontalHit + forward * climbForward;
            target.y = bounds.max.y + topSurfacePadding;

            return HasLandingClearance(target, hit.collider);
        }

        private bool HasLandingClearance(Vector3 target, Collider climbedCollider)
        {
            float radius = Mathf.Max(0.05f, characterController.radius - landingClearancePadding);
            float height = Mathf.Max(characterController.height, radius * 2f);
            Vector3 center = target + characterController.center;
            Vector3 bottom = center + Vector3.down * (height * 0.5f - radius);
            Vector3 top = center + Vector3.up * (height * 0.5f - radius);

            Collider[] overlaps = Physics.OverlapCapsule(bottom, top, radius, blockingLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < overlaps.Length; i++)
            {
                if (overlaps[i] == climbedCollider || overlaps[i] == characterController)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position + Vector3.up * checkHeight;
            Gizmos.DrawLine(origin, origin + transform.forward * checkDistance);
        }
    }
}
