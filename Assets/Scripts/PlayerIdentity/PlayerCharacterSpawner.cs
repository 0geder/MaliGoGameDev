using MaliGo.Characters;
using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Separates Mali from the human player and spawns the PlayerCharacter from PlayerData.
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class PlayerCharacterSpawner : MonoBehaviour
    {
        const string MaliLegacyObjectName = "Player_Sam";
        const string MaliObjectName = "Mali";
        const string SpawnPointName = "PlayerSpawnPoint";

        /// <summary>
        /// characterMedium.fbx imports at ~3.96 world-unit height, but MaliGoWorld's Kenney
        /// City Kit environment uses a compact scale (measured: Player_House ~0.83 units,
        /// a garden fence ~0.27 units - both consistent with ~0.27 units per real metre).
        /// At that ratio a ~1.75m person should measure ~0.47 units, giving this factor
        /// (0.47 / 3.96). See Assets/Editor/MaliGoScaleAudit.cs for the measurement tool.
        /// </summary>
        const float CharacterModelScale = 0.12f;
        const float ControllerHeight = 0.47f;
        const float ControllerRadius = 0.09f;

        [SerializeField] PlayerCharacterCatalog catalog;
        [SerializeField] GameObject playerCharacterPrefab;
        [SerializeField] Vector3 maliOffsetFromPlayer = new Vector3(0.32f, 0f, -0.22f);

        public PlayerCharacterCatalog Catalog => catalog;

        void Awake()
        {
            if (catalog == null)
            {
                catalog = KenneyRuntimeCatalogFactory.LoadCatalog();
            }

            if (playerCharacterPrefab == null)
            {
                playerCharacterPrefab = Resources.Load<GameObject>("PlayerCharacter");
            }
        }

        void Start()
        {
            InitializeWorldCharacters();
        }

        public void InitializeWorldCharacters()
        {
            GameObject mali = SeparateMaliFromPlayer();
            GameObject player = EnsurePlayerCharacter();
            RetargetCamera(player != null ? player.transform : null);

            if (mali != null && player != null)
            {
                PositionMaliNearPlayer(mali, player.transform);
            }
        }

        public static GameObject SeparateMaliFromPlayer()
        {
            GameObject maliObject = GameObject.Find(MaliObjectName) ?? GameObject.Find(MaliLegacyObjectName);
            if (maliObject == null)
            {
                return null;
            }

            if (maliObject.name != MaliObjectName)
            {
                maliObject.name = MaliObjectName;
            }

            maliObject.tag = "Untagged";

            MaliGoPlayerController playerController = maliObject.GetComponent<MaliGoPlayerController>();
            if (playerController != null)
            {
                Destroy(playerController);
            }

            PlayerIdentityBridge identityBridge = maliObject.GetComponent<PlayerIdentityBridge>();
            if (identityBridge != null)
            {
                Destroy(identityBridge);
            }

            if (maliObject.GetComponent<MaliNpcController>() == null)
            {
                maliObject.AddComponent<MaliNpcController>();
            }

            EnsureMaliCompanionComponents(maliObject);

            CharacterController controller = maliObject.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = maliObject.AddComponent<CharacterController>();
                controller.height = 1.2f;
                controller.radius = 0.3f;
                controller.center = new Vector3(0f, 0.6f, 0f);
            }

            return maliObject;
        }

        GameObject EnsurePlayerCharacter()
        {
            GameObject existingPlayer = GameObject.FindWithTag("Player");
            if (existingPlayer != null)
            {
                WireExistingPlayer(existingPlayer);
                return existingPlayer;
            }

            Vector3 spawnPosition = ResolveSpawnPosition();
            GameObject playerRoot = CreatePlayerCharacter(spawnPosition);
            if (playerRoot == null)
            {
                Debug.LogWarning("[PlayerCharacterSpawner] Could not create PlayerCharacter. Run MaliGo/Setup Player Character System in the Editor.");
                return null;
            }

            playerRoot.tag = "Player";
            WireExistingPlayer(playerRoot);
            return playerRoot;
        }

        Vector3 ResolveSpawnPosition()
        {
            GameObject spawnPoint = GameObject.Find(SpawnPointName);
            if (spawnPoint != null)
            {
                return spawnPoint.transform.position;
            }

            GameObject legacyMali = GameObject.Find(MaliObjectName) ?? GameObject.Find(MaliLegacyObjectName);
            if (legacyMali != null)
            {
                return legacyMali.transform.position;
            }

            return new Vector3(2f, 0.05f, -1.3f);
        }

        GameObject CreatePlayerCharacter(Vector3 spawnPosition)
        {
            GameObject playerRoot;

            if (playerCharacterPrefab != null)
            {
                playerRoot = Instantiate(playerCharacterPrefab, spawnPosition, Quaternion.identity);
                playerRoot.name = "PlayerCharacter";
            }
            else if (catalog != null && catalog.characterModelPrefab != null)
            {
                playerRoot = BuildPlayerFromCatalog(spawnPosition);
            }
            else
            {
                catalog = KenneyRuntimeCatalogFactory.LoadCatalog();
                if (catalog != null && catalog.characterModelPrefab != null)
                {
                    playerRoot = BuildPlayerFromCatalog(spawnPosition);
                }
                else
                {
                    return null;
                }
            }

            return playerRoot;
        }

        GameObject BuildPlayerFromCatalog(Vector3 spawnPosition)
        {
            GameObject playerRoot = new GameObject("PlayerCharacter");
            playerRoot.transform.position = spawnPosition;

            CharacterController controller = playerRoot.AddComponent<CharacterController>();
            controller.height = ControllerHeight;
            controller.radius = ControllerRadius;
            controller.center = new Vector3(0f, ControllerHeight * 0.5f, 0f);
            controller.slopeLimit = 45f;
            controller.stepOffset = ControllerHeight * 0.14f;

            Rigidbody rigidbody = playerRoot.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            GameObject visualRoot = new GameObject("HumanVisual");
            visualRoot.transform.SetParent(playerRoot.transform, false);

            GameObject modelInstance = Instantiate(catalog.characterModelPrefab, visualRoot.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one * CharacterModelScale;

            Animator animator = modelInstance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = modelInstance.AddComponent<Animator>();
            }

            if (catalog.animatorController != null)
            {
                animator.runtimeAnimatorController = catalog.animatorController;
            }

            if (animator.avatar == null)
            {
                Animator modelAnimator = catalog.characterModelPrefab.GetComponentInChildren<Animator>();
                if (modelAnimator != null && modelAnimator.avatar != null)
                {
                    animator.avatar = modelAnimator.avatar;
                }
            }

            PlayerCharacterVisualController visualController = visualRoot.GetComponent<PlayerCharacterVisualController>();
            if (visualController == null)
            {
                visualController = visualRoot.AddComponent<PlayerCharacterVisualController>();
            }

            MaliGoPlayerController movement = playerRoot.AddComponent<MaliGoPlayerController>();
            movement.visualController = visualController;

            PlayerIdentityBridge bridge = playerRoot.AddComponent<PlayerIdentityBridge>();
            bridge.Configure(visualController, catalog);

            return playerRoot;
        }

        static void WireExistingPlayer(GameObject playerRoot)
        {
            PlayerCharacterVisualController visualController = playerRoot.GetComponentInChildren<PlayerCharacterVisualController>();
            MaliGoPlayerController movement = playerRoot.GetComponent<MaliGoPlayerController>();
            if (movement == null)
            {
                movement = playerRoot.AddComponent<MaliGoPlayerController>();
            }

            movement.visualController = visualController;

            PlayerIdentityBridge bridge = playerRoot.GetComponent<PlayerIdentityBridge>();
            if (bridge == null)
            {
                bridge = playerRoot.AddComponent<PlayerIdentityBridge>();
            }

            PlayerCharacterCatalog activeCatalog = Object.FindFirstObjectByType<PlayerCharacterSpawner>()?.Catalog;
            if (activeCatalog == null)
            {
                activeCatalog = KenneyRuntimeCatalogFactory.LoadCatalog();
            }

            if (visualController != null)
            {
                bridge.Configure(visualController, activeCatalog);
            }
        }

        static void RetargetCamera(Transform playerTransform)
        {
            if (playerTransform == null)
            {
                return;
            }

            MaliGoCameraController cameraController = Object.FindFirstObjectByType<MaliGoCameraController>();
            if (cameraController != null)
            {
                cameraController.target = playerTransform;
            }
        }

        void PositionMaliNearPlayer(GameObject mali, Transform playerTransform)
        {
            Vector3 target = playerTransform.position + maliOffsetFromPlayer;
            target.y = playerTransform.position.y;
            mali.transform.position = target;

            MaliNpcController maliNpc = mali.GetComponent<MaliNpcController>();
            maliNpc?.RefreshPlayerReference();
            maliNpc?.SetFollowPlayer(true);
        }

        static void EnsureMaliCompanionComponents(GameObject maliObject)
        {
            if (maliObject.GetComponent<MaliDialogueController>() == null)
            {
                maliObject.AddComponent<MaliDialogueController>();
            }

            if (maliObject.GetComponent<MaliCompanionInteraction>() == null)
            {
                maliObject.AddComponent<MaliCompanionInteraction>();
            }
        }
    }
}
