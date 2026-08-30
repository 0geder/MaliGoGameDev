using System.IO;
using MaliGo.Characters;
using MaliGo.PlayerIdentity;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MaliGoPlayerCharacterSetup
{
    const string KenneyRoot = "Assets/kenney_animated-characters-protagonists";
    const string ModelPath = KenneyRoot + "/Model/characterMedium.fbx";
    const string IdleAnimPath = KenneyRoot + "/Animations/idle.fbx";
    const string RunAnimPath = KenneyRoot + "/Animations/run.fbx";
    const string CatalogPath = "Assets/Resources/PlayerCharacterCatalog.asset";
    const string PrefabPath = "Assets/MaliGo/Characters/PlayerCharacter.prefab";
    const string AnimatorPath = "Assets/MaliGo/Characters/PlayerCharacterAnimator.controller";
    const string MaterialPath = "Assets/MaliGo/Characters/PlayerSkinMaterial.mat";
    const string WorldScenePath = "Assets/Scenes/MaliGoWorld.unity";

    [MenuItem("MaliGo/Setup Player Character System")]
    public static void SetupPlayerCharacterSystem()
    {
        EnsureFolders();
        FixSkinImportSettings();
        Material skinMaterial = CreateSkinMaterial();
        RuntimeAnimatorController animatorController = CreateAnimatorController();
        PlayerCharacterCatalog catalog = CreateCatalog(skinMaterial, animatorController);
        GameObject playerPrefab = CreatePlayerPrefab(catalog, animatorController);
        WireMaliGoWorldScene(playerPrefab, catalog);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MaliGo] Player Character System setup complete.");
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/MaliGo"))
        {
            AssetDatabase.CreateFolder("Assets", "MaliGo");
        }

        if (!AssetDatabase.IsValidFolder("Assets/MaliGo/Characters"))
        {
            AssetDatabase.CreateFolder("Assets/MaliGo", "Characters");
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }
    }

    static void FixSkinImportSettings()
    {
        string[] skinPaths =
        {
            KenneyRoot + "/Skins/skaterMaleA.png",
            KenneyRoot + "/Skins/skaterFemaleA.png",
            KenneyRoot + "/Skins/criminalMaleA.png",
            KenneyRoot + "/Skins/cyborgFemaleA.png"
        };

        foreach (string path in skinPaths)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Default;
            importer.spriteImportMode = SpriteImportMode.None;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    static Material CreateSkinMaterial()
    {
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (existing != null)
        {
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader) { name = "PlayerSkinMaterial" };
        Texture2D defaultSkin = LoadTexture(KenneyRoot + "/Skins/skaterMaleA.png");
        if (defaultSkin != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", defaultSkin);
            }
            else if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", defaultSkin);
            }
        }

        AssetDatabase.CreateAsset(material, MaterialPath);
        return material;
    }

    static RuntimeAnimatorController CreateAnimatorController()
    {
        RuntimeAnimatorController existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);
        if (existing != null)
        {
            return existing;
        }

        AnimationClip idleClip = LoadAnimationClip(IdleAnimPath);
        AnimationClip runClip = LoadAnimationClip(RunAnimPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimatorStateMachine root = controller.layers[0].stateMachine;
        AnimatorState idleState = root.AddState("Idle", new Vector3(250f, 0f, 0f));
        idleState.motion = idleClip;
        AnimatorState runState = root.AddState("Run", new Vector3(500f, 0f, 0f));
        runState.motion = runClip;

        AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        return controller;
    }

    static AnimationClip LoadAnimationClip(string assetPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview"))
            {
                return clip;
            }
        }

        Debug.LogWarning($"[MaliGo] Animation clip not found in {assetPath}");
        return null;
    }

    static PlayerCharacterCatalog CreateCatalog(Material skinMaterial, RuntimeAnimatorController animatorController)
    {
        PlayerCharacterCatalog catalog = AssetDatabase.LoadAssetAtPath<PlayerCharacterCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<PlayerCharacterCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.characterModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        catalog.animatorController = animatorController;
        catalog.baseSkinMaterial = skinMaterial;
        catalog.skinOptions = new[]
        {
            new PlayerCharacterCatalog.SkinOption { optionId = "skater_male", skinTexture = LoadTexture(KenneyRoot + "/Skins/skaterMaleA.png") },
            new PlayerCharacterCatalog.SkinOption { optionId = "skater_female", skinTexture = LoadTexture(KenneyRoot + "/Skins/skaterFemaleA.png") },
            new PlayerCharacterCatalog.SkinOption { optionId = "criminal_male", skinTexture = LoadTexture(KenneyRoot + "/Skins/criminalMaleA.png") },
            new PlayerCharacterCatalog.SkinOption { optionId = "cyborg_female", skinTexture = LoadTexture(KenneyRoot + "/Skins/cyborgFemaleA.png") }
        };

        EditorUtility.SetDirty(catalog);
        return catalog;
    }

    static Texture2D LoadTexture(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static GameObject CreatePlayerPrefab(PlayerCharacterCatalog catalog, RuntimeAnimatorController animatorController)
    {
        GameObject modelPrefab = catalog.characterModelPrefab;
        if (modelPrefab == null)
        {
            Debug.LogError("[MaliGo] characterMedium.fbx not found.");
            return null;
        }

        GameObject playerRoot = new GameObject("PlayerCharacter");
        CharacterController controller = playerRoot.AddComponent<CharacterController>();
        controller.height = 1.75f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 0.875f, 0f);
        controller.slopeLimit = 45f;
        controller.stepOffset = 0.25f;

        Rigidbody rigidbody = playerRoot.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;

        GameObject visualRoot = new GameObject("HumanVisual");
        visualRoot.transform.SetParent(playerRoot.transform, false);

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, visualRoot.transform);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;

        Animator animator = modelInstance.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = modelInstance.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = animatorController;
        Animator sourceAnimator = modelPrefab.GetComponentInChildren<Animator>();
        if (sourceAnimator != null && sourceAnimator.avatar != null)
        {
            animator.avatar = sourceAnimator.avatar;
        }

        PlayerCharacterVisualController visualController = visualRoot.AddComponent<PlayerCharacterVisualController>();

        MaliGoPlayerController movement = playerRoot.AddComponent<MaliGoPlayerController>();
        movement.visualController = visualController;

        PlayerIdentityBridge bridge = playerRoot.AddComponent<PlayerIdentityBridge>();
        bridge.Configure(visualController, catalog);

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(playerRoot, PrefabPath);
        Object.DestroyImmediate(playerRoot);

        GameObject resourcesPrefabPath = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        string resourcesPrefabFullPath = "Assets/Resources/PlayerCharacter.prefab";
        if (!File.Exists(resourcesPrefabFullPath))
        {
            AssetDatabase.CopyAsset(PrefabPath, resourcesPrefabFullPath);
        }

        return prefabAsset ?? resourcesPrefabPath;
    }

    static void WireMaliGoWorldScene(GameObject playerPrefab, PlayerCharacterCatalog catalog)
    {
        Scene scene = EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);

        GameObject systems = GameObject.Find("MaliGo_Systems") ?? new GameObject("MaliGo_Systems");
        PlayerCharacterSpawner spawner = systems.GetComponent<PlayerCharacterSpawner>();
        if (spawner == null)
        {
            spawner = systems.AddComponent<PlayerCharacterSpawner>();
        }

        SerializedObject spawnerObject = new SerializedObject(spawner);
        spawnerObject.FindProperty("catalog").objectReferenceValue = catalog;
        spawnerObject.FindProperty("playerCharacterPrefab").objectReferenceValue = playerPrefab;
        spawnerObject.ApplyModifiedPropertiesWithoutUndo();

        ConvertLegacySamToMali();

        if (GameObject.FindWithTag("Player") == null && playerPrefab != null)
        {
            GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
            Vector3 spawnPosition = spawnPoint != null
                ? spawnPoint.transform.position
                : new Vector3(2f, 0.05f, -1.3f);

            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerInstance.transform.position = spawnPosition;
            playerInstance.tag = "Player";
            playerInstance.name = "PlayerCharacter";
        }

        RetargetCameraInScene();
        PositionMaliNearPlayerInScene();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void ConvertLegacySamToMali()
    {
        GameObject mali = GameObject.Find("Mali") ?? GameObject.Find("Player_Sam");
        if (mali == null)
        {
            return;
        }

        mali.name = "Mali";
        mali.tag = "Untagged";

        Object.DestroyImmediate(mali.GetComponent<MaliGoPlayerController>(), true);
        Object.DestroyImmediate(mali.GetComponent<PlayerIdentityBridge>(), true);

        if (mali.GetComponent<MaliNpcController>() == null)
        {
            mali.AddComponent<MaliNpcController>();
        }

        if (mali.GetComponent<MaliDialogueController>() == null)
        {
            mali.AddComponent<MaliDialogueController>();
        }

        if (mali.GetComponent<MaliCompanionInteraction>() == null)
        {
            mali.AddComponent<MaliCompanionInteraction>();
        }

        if (mali.GetComponent<CharacterController>() == null)
        {
            CharacterController controller = mali.AddComponent<CharacterController>();
            controller.height = 1.2f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.6f, 0f);
        }
    }

    static void RetargetCameraInScene()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            return;
        }

        MaliGoCameraController cameraController = Object.FindFirstObjectByType<MaliGoCameraController>();
        if (cameraController != null)
        {
            cameraController.target = player.transform;
            EditorUtility.SetDirty(cameraController);
        }
    }

    static void PositionMaliNearPlayerInScene()
    {
        GameObject mali = GameObject.Find("Mali");
        GameObject player = GameObject.FindWithTag("Player");
        if (mali == null || player == null)
        {
            return;
        }

        Vector3 target = player.transform.position + new Vector3(1.2f, 0f, -0.8f);
        target.y = player.transform.position.y;
        mali.transform.position = target;
    }
}
