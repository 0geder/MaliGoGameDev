using UnityEngine;

namespace MaliGo.Characters
{
    /// <summary>
    /// Mali the meerkat guide NPC. Independent from the human player.
    /// </summary>
    public class MaliNpcController : MonoBehaviour
    {
        [Header("Behaviour State")]
        [SerializeField] MaliBehaviourState behaviourState = MaliBehaviourState.IDLE;

        [Header("Follow Settings")]
        [SerializeField] bool followPlayer;
        [SerializeField] float followDistance = 2.5f;
        [SerializeField] float stopDistance = 1.35f;
        [SerializeField] float followSpeed = 3.2f;
        [SerializeField] Vector3 followOffset = new Vector3(1.2f, 0f, -0.8f);

        [Header("Companion Limits")]
        [SerializeField] float lookAtRange = 8f;
        [SerializeField] float maxCompanionDistance = 12f;

        [Header("Visual")]
        [SerializeField] CharacterSpriteController spriteController;

        Transform playerTransform;
        CharacterController characterController;
        Vector3 waitPosition;
        bool hasWaitPosition;
        Vector3 lastPlayerPosition;
        bool hasLastPlayerPosition;

        public MaliBehaviourState CurrentState => behaviourState;
        public bool FollowPlayerEnabled => followPlayer;
        public Transform PlayerTransform => playerTransform;

        void Awake()
        {
            if (spriteController == null)
            {
                spriteController = GetComponentInChildren<CharacterSpriteController>();
            }

            characterController = GetComponent<CharacterController>();
        }

        void Start()
        {
            RefreshPlayerReference();
            if (playerTransform != null)
            {
                lastPlayerPosition = playerTransform.position;
                hasLastPlayerPosition = true;
            }
        }

        void Update()
        {
            RefreshPlayerReference();

            switch (behaviourState)
            {
                case MaliBehaviourState.FOLLOW:
                    UpdateFollowBehaviour();
                    break;
                case MaliBehaviourState.TALK:
                    UpdateTalkBehaviour();
                    break;
                case MaliBehaviourState.WAIT:
                    UpdateWaitBehaviour();
                    break;
                default:
                    UpdateIdleBehaviour();
                    break;
            }
        }

        public void RefreshPlayerReference()
        {
            GameObject player = GameObject.FindWithTag("Player");
            playerTransform = player != null ? player.transform : null;
        }

        public void SetBehaviourState(MaliBehaviourState newState)
        {
            behaviourState = newState;

            if (newState == MaliBehaviourState.WAIT)
            {
                waitPosition = transform.position;
                hasWaitPosition = true;
            }

            if (newState == MaliBehaviourState.IDLE || newState == MaliBehaviourState.WAIT || newState == MaliBehaviourState.TALK)
            {
                PlayIdleAnimation();
            }
        }

        public void SetFollowPlayer(bool enabled)
        {
            followPlayer = enabled;
            behaviourState = enabled ? MaliBehaviourState.FOLLOW : MaliBehaviourState.IDLE;
            RefreshPlayerReference();
        }

        public void SetWaitPosition(Vector3 worldPosition)
        {
            waitPosition = worldPosition;
            hasWaitPosition = true;
            SetBehaviourState(MaliBehaviourState.WAIT);
        }

        void UpdateIdleBehaviour()
        {
            if (ShouldLookAtPlayer())
            {
                LookTowardPlayer();
            }
            else
            {
                PlayIdleAnimation();
            }
        }

        void UpdateTalkBehaviour()
        {
            LookTowardPlayer();
        }

        void UpdateWaitBehaviour()
        {
            if (!hasWaitPosition)
            {
                waitPosition = transform.position;
                hasWaitPosition = true;
            }

            MoveTowardFlatTarget(waitPosition, followSpeed, stopDistance * 0.5f);

            if (ShouldLookAtPlayer())
            {
                LookTowardPlayer();
            }
        }

        void UpdateFollowBehaviour()
        {
            if (!followPlayer || playerTransform == null)
            {
                PlayIdleAnimation();
                return;
            }

            Vector3 flatPlayer = Flatten(playerTransform.position);
            float distanceToPlayer = FlatDistance(transform.position, flatPlayer);

            if (distanceToPlayer > maxCompanionDistance)
            {
                Vector3 offsetDirection = followOffset.sqrMagnitude > 0.01f
                    ? followOffset.normalized
                    : Vector3.back;
                Vector3 catchUpTarget = flatPlayer + offsetDirection * stopDistance;
                MoveTowardFlatTarget(catchUpTarget, followSpeed * 1.25f, stopDistance);
                return;
            }

            bool playerIsMoving = IsPlayerMoving();
            Vector3 idealPosition = flatPlayer + followOffset;
            float distanceToIdeal = FlatDistance(transform.position, idealPosition);

            if (!playerIsMoving && distanceToIdeal <= followDistance)
            {
                LookTowardPlayer();
                return;
            }

            if (distanceToIdeal <= stopDistance)
            {
                LookTowardPlayer();
                return;
            }

            if (distanceToIdeal > followDistance || playerIsMoving)
            {
                MoveTowardFlatTarget(idealPosition, followSpeed, stopDistance);
            }
            else
            {
                LookTowardPlayer();
            }
        }

        void MoveTowardFlatTarget(Vector3 flatTarget, float speed, float stopRadius)
        {
            Vector3 flatCurrent = Flatten(transform.position);
            float distance = FlatDistance(flatCurrent, flatTarget);

            if (distance <= stopRadius)
            {
                LookTowardPlayer();
                return;
            }

            Vector3 step = Vector3.MoveTowards(flatCurrent, flatTarget, speed * Time.deltaTime);
            Vector3 delta = step - flatCurrent;

            if (characterController != null)
            {
                characterController.Move(delta);
            }
            else
            {
                transform.position = step;
            }

            if (spriteController != null && delta.sqrMagnitude > 0.0001f)
            {
                Vector2 moveInput = new Vector2(delta.x, delta.z).normalized;
                spriteController.UpdateAnimation(moveInput, true);
            }
        }

        bool IsPlayerMoving()
        {
            if (playerTransform == null)
            {
                return false;
            }

            if (!hasLastPlayerPosition)
            {
                lastPlayerPosition = playerTransform.position;
                hasLastPlayerPosition = true;
                return false;
            }

            Vector3 current = Flatten(playerTransform.position);
            Vector3 last = Flatten(lastPlayerPosition);
            bool moving = (current - last).sqrMagnitude > 0.0004f;
            lastPlayerPosition = playerTransform.position;
            return moving;
        }

        bool ShouldLookAtPlayer()
        {
            if (playerTransform == null)
            {
                return false;
            }

            return FlatDistance(transform.position, playerTransform.position) <= lookAtRange;
        }

        public void LookTowardPlayer()
        {
            RefreshPlayerReference();
            PlayIdleAnimation();

            if (playerTransform == null || spriteController == null)
            {
                return;
            }

            Vector3 toPlayer = playerTransform.position - transform.position;
            Vector2 facing = new Vector2(toPlayer.x, toPlayer.z);
            if (facing.sqrMagnitude > 0.0001f)
            {
                spriteController.UpdateAnimation(facing.normalized, false);
            }
        }

        void PlayIdleAnimation()
        {
            if (spriteController != null)
            {
                spriteController.UpdateAnimation(Vector2.zero, false);
            }
        }

        static Vector3 Flatten(Vector3 value)
        {
            return new Vector3(value.x, 0f, value.z);
        }

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            Vector3 flatA = Flatten(a);
            Vector3 flatB = Flatten(b);
            return Vector3.Distance(flatA, flatB);
        }
    }
}
