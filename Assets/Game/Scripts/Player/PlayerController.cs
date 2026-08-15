using UnityEngine;

namespace baodeag.Game
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private VirtualJoystick joystick;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private Boundary boundary;
        [SerializeField] private ClimbDetector climbDetector;
        [SerializeField] private Animator animator;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.8f;
        [SerializeField] private float rotationSpeed = 14f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float groundedStickForce = -2f;
        [SerializeField] private float moveInputSqrThreshold = 0.01f;
        [SerializeField] private float animatorDampTime = 0.1f;

        [Header("Attack")]
        [SerializeField] private float attackRadius = 2.1f;
        [SerializeField] private Vector3 attackOffset = new Vector3(0f, 0.75f, 0f);
        [SerializeField] private LayerMask gemLayers;
        [SerializeField] private int attackHitBufferSize = 12;

        private static readonly int MoveAmountHash = Animator.StringToHash("MoveAmount");
        private static readonly int VerticalHash = Animator.StringToHash("Vertical");
        private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
        private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private CharacterController characterController;
        private Collider[] attackHits;
        private Vector3 verticalVelocity;
        private bool inputEnabled;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            attackHits = new Collider[Mathf.Max(1, attackHitBufferSize)];
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            rotationSpeed = Mathf.Max(0f, rotationSpeed);
            moveInputSqrThreshold = Mathf.Max(0f, moveInputSqrThreshold);
            animatorDampTime = Mathf.Max(0f, animatorDampTime);
            attackRadius = Mathf.Max(0f, attackRadius);
            attackHitBufferSize = Mathf.Max(1, attackHitBufferSize);
        }

        private void Update()
        {
            if (climbDetector != null && climbDetector.IsClimbing)
            {
                UpdateAnimator(Vector2.zero, 0f);
                return;
            }

            HandleGravity();

            if (!inputEnabled)
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

            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }

            int hitCount = Physics.OverlapSphereNonAlloc(GetAttackCenter(), attackRadius, attackHits, gemLayers, QueryTriggerInteraction.Collide);
            Gem nearestGem = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Gem gem = attackHits[i].GetComponentInParent<Gem>();
                attackHits[i] = null;
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

            if (moveDirection.sqrMagnitude > moveInputSqrThreshold)
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
            if (characterController.enabled)
            {
                characterController.Move(motion * Time.deltaTime);
            }

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
            if (input.sqrMagnitude < moveInputSqrThreshold)
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
            if (animator == null)
            {
                return;
            }

            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = groundedStickForce;
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            animator.SetBool(IsGroundedHash, characterController.isGrounded);
        }

        private void UpdateAnimator(Vector2 input, float moveAmount)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetFloat(HorizontalHash, input.x, animatorDampTime, Time.deltaTime);
            animator.SetFloat(VerticalHash, input.y, animatorDampTime, Time.deltaTime);
            animator.SetFloat(MoveAmountHash, moveAmount, animatorDampTime, Time.deltaTime);
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
