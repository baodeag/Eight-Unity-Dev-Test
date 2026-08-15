using UnityEngine;

namespace baodeag.InterviewTest
{
    [RequireComponent(typeof(CharacterController))]
    public class InterviewPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InterviewVirtualJoystick joystick;
        [SerializeField] private InterviewCameraController cameraController;
        [SerializeField] private InterviewBoundary boundary;
        [SerializeField] private InterviewClimbDetector climbDetector;
        [SerializeField] private Animator animator;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.8f;
        [SerializeField] private float rotationSpeed = 14f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float groundedStickForce = -2f;

        [Header("Attack")]
        [SerializeField] private float attackRadius = 2.1f;
        [SerializeField] private Vector3 attackOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private LayerMask gemLayers;

        private static readonly int MoveAmountHash = Animator.StringToHash("MoveAmount");
        private static readonly int VerticalHash = Animator.StringToHash("Vertical");
        private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private bool inputEnabled;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleGravity();

            if (!inputEnabled || (climbDetector != null && climbDetector.IsClimbing))
            {
                UpdateAnimator(Vector2.zero, 0f);
                return;
            }

            HandleMovement();
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled)
            {
                UpdateAnimator(Vector2.zero, 0f);
            }
        }

        public void AttemptAttack()
        {
            if (!inputEnabled || (climbDetector != null && climbDetector.IsClimbing))
            {
                return;
            }

            animator.SetTrigger(AttackHash);

            Collider[] hits = Physics.OverlapSphere(GetAttackCenter(), attackRadius, gemLayers, QueryTriggerInteraction.Collide);
            InterviewGem nearestGem = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                InterviewGem gem = hits[i].GetComponentInParent<InterviewGem>();
                if (gem == null || gem.IsCollected)
                {
                    continue;
                }

                float sqrDistance = (gem.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearestGem = gem;
                }
            }

            if (nearestGem != null)
            {
                nearestGem.Collect();
            }
        }

        private void HandleMovement()
        {
            Vector2 input = joystick != null ? joystick.Direction : Vector2.zero;
            Vector3 moveDirection = GetCameraRelativeMove(input);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                if (climbDetector != null)
                {
                    climbDetector.TryStartClimb(moveDirection, boundary);
                }
            }

            Vector3 motion = moveDirection * moveSpeed;
            motion += verticalVelocity;
            characterController.Move(motion * Time.deltaTime);

            if (boundary != null)
            {
                characterController.enabled = false;
                transform.position = boundary.ClampPosition(transform.position);
                characterController.enabled = true;
            }

            UpdateAnimator(input, moveDirection.magnitude);
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                return Vector3.zero;
            }

            Quaternion planarRotation = cameraController != null ? cameraController.PlanarRotation : Quaternion.identity;
            Vector3 forward = planarRotation * Vector3.forward;
            Vector3 right = planarRotation * Vector3.right;
            Vector3 moveDirection = forward * input.y + right * input.x;
            moveDirection.y = 0f;
            return Vector3.ClampMagnitude(moveDirection, 1f);
        }

        private void HandleGravity()
        {
            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = groundedStickForce;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            animator.SetBool(IsGroundedHash, characterController.isGrounded);
        }

        private void UpdateAnimator(Vector2 input, float moveAmount)
        {
            animator.SetFloat(HorizontalHash, input.x, 0.1f, Time.deltaTime);
            animator.SetFloat(VerticalHash, input.y, 0.1f, Time.deltaTime);
            animator.SetFloat(MoveAmountHash, moveAmount, 0.1f, Time.deltaTime);
        }

        private Vector3 GetAttackCenter()
        {
            return transform.position + transform.rotation * attackOffset;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(GetAttackCenter(), attackRadius);
        }
    }
}
