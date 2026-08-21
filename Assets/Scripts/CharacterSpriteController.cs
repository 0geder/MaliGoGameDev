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

        [Header("Billboard")]
        public bool faceCameraOnStart = true;

        private SpriteRenderer spriteRenderer;
        private Sprite[] idleSet;
        private Sprite[] currentSet;
        private float frameTimer;
        private int frameIndex;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            idleSet = idleSprite != null ? new[] { idleSprite } : System.Array.Empty<Sprite>();
            currentSet = idleSet;

            if (idleSprite != null) spriteRenderer.sprite = idleSprite;
        }

        void Start()
        {
            if (faceCameraOnStart && Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;
        }

        public void UpdateAnimation(Vector2 moveInput, bool isMoving)
        {
            if (!isMoving)
            {
                spriteRenderer.flipX = false;
                spriteRenderer.sprite = idleSprite;
                frameTimer = 0f;
                frameIndex = 0;
                currentSet = idleSet;
                return;
            }

            bool vertical = Mathf.Abs(moveInput.y) >= Mathf.Abs(moveInput.x);
            Sprite[] set;

            if (vertical && moveInput.y >= 0f)
                set = walkUp != null && walkUp.Length > 0 ? walkUp : idleSet;
            else if (vertical)
                set = walkDown != null && walkDown.Length > 0 ? walkDown : idleSet;
            else
                set = walkSide != null && walkSide.Length > 0 ? walkSide : idleSet;

            spriteRenderer.flipX = !vertical && moveInput.x < 0f;

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

            if (set.Length > 0) spriteRenderer.sprite = set[frameIndex];
        }
    }
}