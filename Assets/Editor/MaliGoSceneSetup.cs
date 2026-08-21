using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class MaliGoSceneSetup
{
    [MenuItem("Tools/MaliGo/Setup Initial Scene")]
    public static void SetupScene()
    {
        // 1. Ensure we have a clean scene
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.path == "" || currentScene.name == "Untitled")
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        // 2. MaliGo Color Palette
        Color mossGreen = new Color(0.031f, 0.478f, 0.094f);
        Color deepForest = new Color(0.059f, 0.369f, 0.180f);
        Color sage = new Color(0.788f, 0.937f, 0.706f);
        Color cream = new Color(0.976f, 1.000f, 0.965f);
        Color warmSand = new Color(1.000f, 0.957f, 0.914f);
        Color goldenAmber = new Color(0.875f, 0.643f, 0.392f);
        Color emberCoral = new Color(0.910f, 0.373f, 0.282f);
        Color charcoal = new Color(0.063f, 0.063f, 0.063f);

        Shader safeShader = Shader.Find("Universal Render Pipeline/Lit") ?? 
                            Shader.Find("HDRP/Lit") ?? 
                            Shader.Find("Standard") ?? 
                            Shader.Find("Diffuse");

        // 3. Main Camera (Isometric / Three-Quarter)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.name = "MaliGoMainCamera";
            mainCam.transform.position = new Vector3(-10f, 10f, -10f);
            mainCam.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 10f;
            mainCam.backgroundColor = deepForest;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            if (mainCam.GetComponent<MaliGoCameraController>() == null)
                mainCam.gameObject.AddComponent<MaliGoCameraController>();
        }
        else
        {
            GameObject camObj = new GameObject("MaliGoMainCamera");
            mainCam = camObj.AddComponent<Camera>();
            mainCam.transform.position = new Vector3(-10f, 10f, -10f);
            mainCam.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            mainCam.orthographic = true;
            mainCam.orthographicSize = 10f;
            mainCam.backgroundColor = deepForest;
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.tag = "MainCamera";
            mainCam.gameObject.AddComponent<MaliGoCameraController>();
        }

        // 4. Lighting (Fixed deprecation warning)
        Light dirLight = UnityEngine.Object.FindObjectOfType<Light>();
        if (dirLight == null || dirLight.type != LightType.Directional)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            dirLight.color = cream;
            dirLight.intensity = 1.2f;
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // 5. Environment: Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Environment";
        ground.transform.localScale = new Vector3(2f, 1f, 2f);
        Renderer groundRen = ground.GetComponent<Renderer>();
        groundRen.material = new Material(safeShader) { color = sage };

        // 6. Environment: Street/Path
        GameObject street = GameObject.CreatePrimitive(PrimitiveType.Plane);
        street.name = "Street_Path";
        street.transform.position = new Vector3(0f, 0.01f, 0f);
        street.transform.localScale = new Vector3(0.4f, 1f, 2f);
        Renderer streetRen = street.GetComponent<Renderer>();
        streetRen.material = new Material(safeShader) { color = charcoal };

        // 7. Stylized Home Placeholder
        GameObject home = GameObject.CreatePrimitive(PrimitiveType.Cube);
        home.name = "PlayerHome_Placeholder";
        home.transform.position = new Vector3(-4f, 1.5f, -4f);
        home.transform.localScale = new Vector3(4f, 3f, 4f);
        Renderer homeRen = home.GetComponent<Renderer>();
        homeRen.material = new Material(safeShader) { color = warmSand };

        // FIX: Changed from Cone to Cylinder to avoid PrimitiveType.Cone error
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "HomeRoof";
        roof.transform.position = new Vector3(-4f, 3.2f, -4f);
        roof.transform.localScale = new Vector3(4.5f, 0.8f, 4.5f); // Flat roof style
        Renderer roofRen = roof.GetComponent<Renderer>();
        roofRen.material = new Material(safeShader) { color = emberCoral };

        // 8. Player Placeholder (Sam)
        GameObject player = new GameObject("Player_Sam");
        player.transform.position = new Vector3(0f, 0f, 2f);
        player.tag = "Player";
        CharacterController playerController = player.AddComponent<CharacterController>();
        playerController.height = 2f;
        playerController.radius = 0.4f;
        playerController.center = new Vector3(0f, 1f, 0f);

        Rigidbody playerBody = player.AddComponent<Rigidbody>();
        playerBody.isKinematic = true;
        playerBody.useGravity = false;

        GameObject visual = new GameObject("SpriteVisual");
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = new Vector3(0f, 1f, 0f);
        visual.AddComponent<SpriteRenderer>();
        MaliGo.Characters.CharacterSpriteController spriteController = visual.AddComponent<MaliGo.Characters.CharacterSpriteController>();
        Sprite idle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Sam_Idle.png");
        if (idle != null)
        {
            spriteController.idleSprite = idle;
        }
        else
        {
            Debug.LogWarning("Sam_Idle sprite not found at Assets/Sprites/Sam_Idle.png. Assign it to SpriteVisual after importing the character asset.");
        }

        MaliGoPlayerController playerScript = player.AddComponent<MaliGoPlayerController>();
        playerScript.spriteController = spriteController;

        // 9. UI Canvas (Screen Space - Overlay)
        GameObject canvasObj = new GameObject("MaliGo_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- HUD PANEL (Top Left) ---
        GameObject hudPanel = new GameObject("HUD_Panel");
        hudPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(30, -30);
        hudRect.sizeDelta = new Vector2(320, 280);
        Image panelImg = hudPanel.AddComponent<Image>();
        panelImg.color = new Color(charcoal.r, charcoal.g, charcoal.b, 0.85f);

        // Helper to create UI Text
        System.Func<string, Vector2, float, GameObject> createText = (text, pos, fontSize) => {
            GameObject txtObj = new GameObject("Text_" + text.Replace(" ", "_"));
            txtObj.transform.SetParent(hudPanel.transform, false);
            RectTransform rect = txtObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(300, 30);
            
            Text txt = txtObj.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = (int)fontSize;
            txt.color = cream;
            txt.alignment = TextAnchor.MiddleLeft;
            return txtObj;
        };

        createText("SAM", new Vector2(15, -15), 26f);
        createText("LEVEL 1", new Vector2(15, -45), 18f);
        
        // XP Bar
        GameObject xpBg = new GameObject("XP_BG");
        xpBg.transform.SetParent(hudPanel.transform, false);
        RectTransform xpBgRect = xpBg.AddComponent<RectTransform>();
        xpBgRect.anchorMin = new Vector2(0, 1);
        xpBgRect.anchorMax = new Vector2(0, 1);
        xpBgRect.pivot = new Vector2(0, 1);
        xpBgRect.anchoredPosition = new Vector2(15, -75);
        xpBgRect.sizeDelta = new Vector2(200, 15);
        xpBg.AddComponent<Image>().color = charcoal;

        GameObject xpFill = new GameObject("XP_Fill");
        xpFill.transform.SetParent(xpBg.transform, false);
        RectTransform xpFillRect = xpFill.AddComponent<RectTransform>();
        xpFillRect.anchorMin = new Vector2(0, 0);
        xpFillRect.anchorMax = new Vector2(0, 1);
        xpFillRect.pivot = new Vector2(0, 0.5f);
        xpFillRect.anchoredPosition = new Vector2(0, 0);
        xpFillRect.sizeDelta = new Vector2(80, 15); // 40% XP
        xpFill.AddComponent<Image>().color = goldenAmber;

        createText("Cash: R5,000", new Vector2(15, -105), 18f);
        createText("Savings: R1,000", new Vector2(15, -135), 18f);
        createText("Stress: 18%", new Vector2(15, -165), 18f);

        // --- TODAY'S GOAL BOX (Top Right) ---
        GameObject goalBox = new GameObject("Goal_Box");
        goalBox.transform.SetParent(canvasObj.transform, false);
        RectTransform goalRect = goalBox.AddComponent<RectTransform>();
        goalRect.anchorMin = new Vector2(1, 1);
        goalRect.anchorMax = new Vector2(1, 1);
        goalRect.pivot = new Vector2(1, 1);
        goalRect.anchoredPosition = new Vector2(-30, -30);
        goalRect.sizeDelta = new Vector2(280, 130);
        Image goalImg = goalBox.AddComponent<Image>();
        goalImg.color = new Color(mossGreen.r, mossGreen.g, mossGreen.b, 0.9f);

        GameObject goalTitle = new GameObject("Goal_Title");
        goalTitle.transform.SetParent(goalBox.transform, false);
        RectTransform gtRect = goalTitle.AddComponent<RectTransform>();
        gtRect.anchorMin = new Vector2(0, 1);
        gtRect.anchorMax = new Vector2(0, 1);
        gtRect.pivot = new Vector2(0, 1);
        gtRect.anchoredPosition = new Vector2(20, -20);
        gtRect.sizeDelta = new Vector2(240, 30);
        Text gtText = goalTitle.AddComponent<Text>();
        gtText.text = "Today's Goal:";
        gtText.fontSize = 22;
        gtText.color = cream;
        gtText.alignment = TextAnchor.MiddleLeft;

        GameObject goalVal = new GameObject("Goal_Value");
        goalVal.transform.SetParent(goalBox.transform, false);
        RectTransform gvRect = goalVal.AddComponent<RectTransform>();
        gvRect.anchorMin = new Vector2(0, 1);
        gvRect.anchorMax = new Vector2(0, 1);
        gvRect.pivot = new Vector2(0, 1);
        gvRect.anchoredPosition = new Vector2(20, -55);
        gvRect.sizeDelta = new Vector2(240, 40);
        Text gvText = goalVal.AddComponent<Text>();
        gvText.text = "Save R200";
        gvText.fontSize = 28;
        gvText.color = goldenAmber;
        gvText.fontStyle = FontStyle.Bold;
        gvText.alignment = TextAnchor.MiddleLeft;

        // 10. Save Scene and Add to Build Settings
        string scenePath = "Assets/Scenes/MaliGoWorld.unity";
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        
        bool inBuild = false;
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.path == scenePath) { inBuild = true; break; }
        }
        
        if (!inBuild)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Debug.Log("✅ SUCCESS: MaliGo World Scene generated, saved, and added to Build Settings.");
        Debug.Log("▶️ Press PLAY to see the environment and HUD.");
    }
}