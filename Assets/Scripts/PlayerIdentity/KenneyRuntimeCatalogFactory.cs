using UnityEngine;

namespace MaliGo.PlayerIdentity
{
    /// <summary>
    /// Loads Kenney player character assets at runtime, with Editor fallbacks before setup assets exist.
    /// </summary>
    public static class KenneyRuntimeCatalogFactory
    {
        const string KenneyRoot = "Assets/kenney_animated-characters-protagonists";
        const string ModelPath = KenneyRoot + "/Model/characterMedium.fbx";
        const string IdleClipPath = KenneyRoot + "/Animations/idle.fbx";
        const string RunClipPath = KenneyRoot + "/Animations/run.fbx";
        const string AnimatorPath = "Assets/MaliGo/Characters/PlayerCharacterAnimator.controller";
        const string MaterialPath = "Assets/MaliGo/Characters/PlayerSkinMaterial.mat";

        public static PlayerCharacterCatalog LoadCatalog()
        {
            PlayerCharacterCatalog catalog = Resources.Load<PlayerCharacterCatalog>("PlayerCharacterCatalog");
            if (catalog != null && catalog.characterModelPrefab != null)
            {
                return catalog;
            }

#if UNITY_EDITOR
            return CreateEditorFallbackCatalog();
#else
            return catalog;
#endif
        }

#if UNITY_EDITOR
        static PlayerCharacterCatalog CreateEditorFallbackCatalog()
        {
            var catalog = ScriptableObject.CreateInstance<PlayerCharacterCatalog>();
            catalog.characterModelPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            catalog.animatorController = LoadOrCreateAnimatorController();
            catalog.baseSkinMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);

            if (catalog.baseSkinMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                catalog.baseSkinMaterial = new Material(shader);
            }

            catalog.skinOptions = new[]
            {
                CreateSkinOption("skater_male", KenneyRoot + "/Skins/skaterMaleA.png"),
                CreateSkinOption("skater_female", KenneyRoot + "/Skins/skaterFemaleA.png"),
                CreateSkinOption("criminal_male", KenneyRoot + "/Skins/criminalMaleA.png"),
                CreateSkinOption("cyborg_female", KenneyRoot + "/Skins/cyborgFemaleA.png")
            };

            return catalog;
        }

        static PlayerCharacterCatalog.SkinOption CreateSkinOption(string id, string texturePath)
        {
            return new PlayerCharacterCatalog.SkinOption
            {
                optionId = id,
                skinTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath)
            };
        }

        /// <summary>
        /// The Kenney pack ships the rig (characterMedium.fbx) and its animations (idle.fbx, run.fbx)
        /// as separate files sharing one skeleton - Unity's standard multi-FBX Generic rig workflow.
        /// No PlayerCharacterAnimator.controller ships with the project, so this builds one the first
        /// time it's needed and reuses it afterwards.
        /// </summary>
        static RuntimeAnimatorController LoadOrCreateAnimatorController()
        {
            var existing = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath);
            if (existing != null)
            {
                return existing;
            }

            AnimationClip idleClip = LoadFirstAnimationClip(IdleClipPath);
            AnimationClip runClip = LoadFirstAnimationClip(RunClipPath);

            if (idleClip == null && runClip == null)
            {
                Debug.LogWarning("[KenneyRuntimeCatalogFactory] No idle/run animation clips found - PlayerCharacterAnimator.controller was not created.");
                return null;
            }

            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            UnityEditor.Animations.AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            UnityEditor.Animations.AnimatorState idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;

            UnityEditor.Animations.AnimatorState runState = stateMachine.AddState("Run");
            runState.motion = runClip != null ? runClip : idleClip;

            stateMachine.defaultState = idleState;

            UnityEditor.Animations.AnimatorStateTransition idleToRun = idleState.AddTransition(runState);
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.15f;
            idleToRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.15f, "Speed");

            UnityEditor.Animations.AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;
            runToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.15f, "Speed");

            UnityEditor.EditorUtility.SetDirty(controller);
            UnityEditor.AssetDatabase.SaveAssets();

            return controller;
        }

        static AnimationClip LoadFirstAnimationClip(string assetPath)
        {
            UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                {
                    return clip;
                }
            }

            return null;
        }
#endif
    }
}
