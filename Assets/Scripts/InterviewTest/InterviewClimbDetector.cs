using System.Collections;
using UnityEngine;

namespace baodeag.InterviewTest
{
    public class InterviewClimbDetector : MonoBehaviour
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
        [SerializeField] private float climbHeight = 1.8f;
        [SerializeField] private float climbForward = 0.9f;
        [SerializeField] private float climbDuration = 0.85f;

        public bool IsClimbing { get; private set; }

        private static readonly int IsClimbingHash = Animator.StringToHash("isClimbing");

        public bool TryStartClimb(Vector3 moveDirection, InterviewBoundary boundary)
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

            Vector3 target = transform.position + Vector3.up * climbHeight + forward.normalized * climbForward;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position + Vector3.up * checkHeight;
            Gizmos.DrawLine(origin, origin + transform.forward * checkDistance);
        }
    }
}
