using UnityEngine;

namespace MaliGo.Characters
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CharacterSpriteController : MonoBehaviour
    {
        [Header("Sprites")]
        public Sprite idleSprite;
        public Sprite[] walkDown;
        public Sprite[] walkUp;
        public Sprite[] walkSide;

        [Header("Animation")]
        public float frameRate = 8f;

        [Header("Juicy Walk Wobble")]
        public bool enableWalkBobbing = true;
        public float bobFrequency = 12f;
        public float bobHeight = 0.05f;
        public float wobbleTiltAngle = 4f;

        [Header("Billboard")]
        public bool faceCamera = true;

        private SpriteRenderer spriteRenderer;
        private Sprite[] idleSet;
        private Sprite[] currentSet;
        private float frameTimer;
        private int frameIndex;

        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;
        private float walkTime;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            initialLocalPos = transform.localPosition;
            initialLocalRot = transform.localRotation;

            idleSet = idleSprite != null ? new[] { idleSprite } : System.Array.Empty<Sprite>();
            currentSet = idleSet;

            if (idleSprite != null) spriteRenderer.sprite = idleSprite;
        }

        void LateUpdate()
        {
            if (faceCamera && Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }

        public void UpdateAnimation(Vector2 moveInput, bool isMoving)
        {
            if (spriteRenderer == null) return;

            if (!isMoving || moveInput.sqrMagnitude < 0.01f)
            {
                spriteRenderer.sprite = idleSprite != null ? idleSprite : spriteRenderer.sprite;
                frameTimer = 0f;
                frameIndex = 0;
                currentSet = idleSet;

                // Return smoothly to rest position
                if (enableWalkBobbing)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPos, Time.deltaTime * 10f);
                }
                return;
            }

            // Directional Facing
            // moveInput is screen-relative: X > 0 is Right, X < 0 is Left, Y > 0 is Up, Y < 0 is Down
            bool verticalDominant = Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x);
            Sprite[] set;

            if (verticalDominant && moveInput.y > 0f)
            {
                set = (walkUp != null && walkUp.Length > 0) ? walkUp : idleSet;
            }
            else if (verticalDominant)
            {
                set = (walkDown != null && walkDown.Length > 0) ? walkDown : idleSet;
            }
            else
            {
                set = (walkSide != null && walkSide.Length > 0) ? walkSide : idleSet;
            }

            // Flip horizontally when moving left
            if (Mathf.Abs(moveInput.x) > 0.05f)
            {
                spriteRenderer.flipX = moveInput.x < 0f;
            }

            // Frame animation
            if (set != currentSet)
            {
                currentSet = set;
                frameIndex = 0;
                frameTimer = 0f;
            }

            if (set.Length > 1)
            {
                frameTimer += Time.deltaTime * Mathf.Max(0.01f, frameRate);
                if (frameTimer >= 1f)
                {
                    frameTimer -= 1f;
                    frameIndex = (frameIndex + 1) % set.Length;
                }
            }

            if (set.Length > 0)
            {
                spriteRenderer.sprite = set[frameIndex];
            }

            // Procedural Walk Step Bobbing & Tilt for bouncy meerkat feel
            if (enableWalkBobbing)
            {
                walkTime += Time.deltaTime * bobFrequency;
                float bobY = Mathf.Abs(Mathf.Sin(walkTime)) * bobHeight;
                float tiltZ = Mathf.Sin(walkTime * 0.5f) * wobbleTiltAngle;

                transform.localPosition = initialLocalPos + new Vector3(0f, bobY, 0f);
            }
        }
    }
}