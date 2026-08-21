using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class MaliGoEnvironmentBuilder
{
    [MenuItem("Tools/MaliGo/Build Isometric World")]
    public static void BuildIsometricWorld()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        Color mossGreen = new Color(0.031f, 0.478f, 0.094f);
        Color deepForest = new Color(0.039f, 0.361f, 0.165f);
        Color sage = new Color(0.788f, 0.937f, 0.706f);
        Color cream = new Color(0.976f, 1f, 0.965f);
        Color warmSand = new Color(1f, 0.957f, 0.914f);
        Color goldenAmber = new Color(0.875f, 0.643f, 0.392f);
        Color emberCoral = new Color(0.910f, 0.373f, 0.282f);
        Color charcoal = new Color(0.063f, 0.063f, 0.063f);
        Color skyBlue1 = new Color(0.439f, 0.753f, 0.851f);
        
        Shader safeShader = Shader.Find("Universal Render Pipeline/Lit") ?? 
                            Shader.Find("Standard") ?? 
                            Shader.Find("Diffuse");
        
        SetupIsometricCamera();
        CreateSkyGradient(skyBlue1);
        CreateGround(sage, safeShader);
        CreateRoad(charcoal, safeShader);
        
        CreateLearningCenter(new Vector3(-12, 0, -8), mossGreen, deepForest, safeShader);
        CreateShop(new Vector3(-6, 0, -4), emberCoral, warmSand, goldenAmber, safeShader);
        CreateBank(new Vector3(6, 0, -6), goldenAmber, warmSand, safeShader);
        CreateArcade(new Vector3(12, 0, -8), new Color(0.753f, 0.627f, 0.878f), deepForest, safeShader);
        CreateHome(new Vector3(0, 0, 4), mossGreen, deepForest, warmSand, safeShader);
        
        CreateTree(new Vector3(-8, 0, 2), sage, deepForest);
        CreateTree(new Vector3(-5, 0, 3), sage, deepForest);
        CreateTree(new Vector3(-2, 0, 2), sage, deepForest);
        CreateTree(new Vector3(4, 0, 2), sage, deepForest);
        CreateTree(new Vector3(9, 0, 3), sage, deepForest);
        CreateTree(new Vector3(11, 0, 1), sage, deepForest);
        
        CreateStreetLamp(new Vector3(-3, 0, 0), charcoal);
        CreateStreetLamp(new Vector3(4, 0, 0), charcoal);
        
        CreateCharacter(new Vector3(0, 0, 2));
        CreateMailbox(new Vector3(3, 0, 3), mossGreen);
        
        CreateGardenPot(new Vector3(-2, 0, 3), emberCoral);
        CreateGardenPot(new Vector3(-1, 0, 3.5f), goldenAmber);
        CreateGardenPot(new Vector3(2, 0, 3), sage);
        
        CreateLighting(cream);
        
        string scenePath = "Assets/Scenes/MaliGoIsometricWorld.unity";
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        
        AddSceneToBuildSettings(scenePath);
        
        Debug.Log("✅ MaliGo Isometric World Built Successfully!");
        Debug.Log("▶️ Press Play to explore your world!");
    }
    
    static void SetupIsometricCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MaliGoMainCamera");
            mainCam = camObj.AddComponent<Camera>();
            mainCam.tag = "MainCamera";
        }
        
        mainCam.orthographic = true;
        mainCam.orthographicSize = 12f;
        mainCam.transform.position = new Vector3(0, 18, -16);
        mainCam.transform.rotation = Quaternion.Euler(35f, 45f, 0f);
        mainCam.backgroundColor = new Color(0.439f, 0.753f, 0.851f);
        mainCam.clearFlags = CameraClearFlags.SolidColor;
        if (mainCam.GetComponent<MaliGoCameraController>() == null)
            mainCam.gameObject.AddComponent<MaliGoCameraController>();
    }
    
    static void CreateSkyGradient(Color topColor)
    {
        if (Camera.main != null) Camera.main.backgroundColor = topColor;
    }
    
    static void CreateGround(Color color, Shader shader)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground_Grass";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.transform.position = new Vector3(0, -0.1f, 0);
        
        Renderer ren = ground.GetComponent<Renderer>();
        ren.material = new Material(shader) { color = color };
        
        Collider col = ground.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
    
    static void CreateRoad(Color color, Shader shader)
    {
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Plane);
        road.name = "Road_Main";
        road.transform.localScale = new Vector3(0.35f, 1f, 3f);
        road.transform.position = new Vector3(0, 0, 0);
        
        Renderer ren = road.GetComponent<Renderer>();
        ren.material = new Material(shader) { color = color };
        
        for (int i = 0; i < 8; i++)
        {
            GameObject marking = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marking.name = "Road_Marking_" + i;
            marking.transform.localScale = new Vector3(0.02f, 0.01f, 0.3f);
            marking.transform.position = new Vector3(0, 0.01f, -6f + i * 1.7f);
            
            Renderer markRen = marking.GetComponent<Renderer>();
            markRen.material = new Material(shader) { color = Color.white };
            
            Object.DestroyImmediate(marking.GetComponent<Collider>());
        }
        
        Collider col = road.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
    
    static void CreateLearningCenter(Vector3 pos, Color roofColor, Color wallColor, Shader shader)
    {
        GameObject building = new GameObject("Building_LearningCenter");
        building.transform.position = pos;
        
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainStructure";
        main.transform.SetParent(building.transform);
        main.transform.localPosition = new Vector3(0, 2f, 0);
        main.transform.localScale = new Vector3(3f, 4f, 3f);
        main.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Roof";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0, 4.5f, 0);
        roof.transform.localScale = new Vector3(3.5f, 2f, 3.5f);
        roof.GetComponent<Renderer>().material = new Material(shader) { color = roofColor };
        
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(building.transform);
        door.transform.localPosition = new Vector3(0, 1f, 1.51f);
        door.transform.localScale = new Vector3(1f, 2f, 0.2f);
        door.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.361f, 0.239f, 0.180f) };
        
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.SetParent(building.transform);
        window.transform.localPosition = new Vector3(-1f, 2.5f, 1.51f);
        window.transform.localScale = new Vector3(1f, 1f, 0.2f);
        window.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.682f, 0.839f, 0.945f, 0.7f) };
        
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "Sign";
        sign.transform.SetParent(building.transform);
        sign.transform.localPosition = new Vector3(0, 3.5f, 1.6f);
        sign.transform.localScale = new Vector3(2f, 0.5f, 0.3f);
        sign.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.102f, 0.353f, 0.784f) };
        
        building.AddComponent<MaliGo.Environment.BuildingInteriorController>();
        DestroyColliders(building);
    }

    static void CreateShop(Vector3 pos, Color roofColor, Color wallColor, Color signColor, Shader shader)
    {
        GameObject building = new GameObject("Building_Shop");
        building.transform.position = pos;
        
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainStructure";
        main.transform.SetParent(building.transform);
        main.transform.localPosition = new Vector3(0, 2.5f, 0);
        main.transform.localScale = new Vector3(3.5f, 5f, 3.5f);
        main.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0, 5f, -0.5f);
        roof.transform.localScale = new Vector3(3.7f, 0.5f, 3.7f);
        roof.transform.rotation = Quaternion.Euler(10f, 0, 0);
        roof.GetComponent<Renderer>().material = new Material(shader) { color = roofColor };
        
        GameObject awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awning.name = "Awning";
        awning.transform.SetParent(building.transform);
        awning.transform.localPosition = new Vector3(0, 3.5f, 2f);
        awning.transform.localScale = new Vector3(3.6f, 0.3f, 1f);
        awning.GetComponent<Renderer>().material = new Material(shader) { color = roofColor };
        
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(building.transform);
        door.transform.localPosition = new Vector3(0, 1.25f, 1.76f);
        door.transform.localScale = new Vector3(1.2f, 2.5f, 0.2f);
        door.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.361f, 0.239f, 0.180f) };
        
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.SetParent(building.transform);
        window.transform.localPosition = new Vector3(1.2f, 3f, 1.76f);
        window.transform.localScale = new Vector3(1f, 1.5f, 0.2f);
        window.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.682f, 0.839f, 0.945f, 0.7f) };
        
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "Sign";
        sign.transform.SetParent(building.transform);
        sign.transform.localPosition = new Vector3(0, 4.5f, 1.8f);
        sign.transform.localScale = new Vector3(2f, 0.6f, 0.3f);
        sign.GetComponent<Renderer>().material = new Material(shader) { color = signColor };
        
        building.AddComponent<MaliGo.Environment.BuildingInteriorController>();
        DestroyColliders(building);
    }

    static void CreateBank(Vector3 pos, Color accentColor, Color wallColor, Shader shader)
    {
        GameObject building = new GameObject("Building_Bank");
        building.transform.position = pos;
        
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainStructure";
        main.transform.SetParent(building.transform);
        main.transform.localPosition = new Vector3(0, 3f, 0);
        main.transform.localScale = new Vector3(5f, 6f, 4f);
        main.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof_Pediment";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0, 6.5f, 0);
        roof.transform.localScale = new Vector3(5.2f, 1.5f, 4.2f);
        roof.GetComponent<Renderer>().material = new Material(shader) { color = accentColor };
        
        for (int i = 0; i < 4; i++)
        {
            GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.name = "Column_" + i;
            column.transform.SetParent(building.transform);
            column.transform.localPosition = new Vector3(-2.25f + i * 1.5f, 3f, 2.1f);
            column.transform.localScale = new Vector3(0.3f, 6f, 0.3f);
            column.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        }
        
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(building.transform);
        door.transform.localPosition = new Vector3(0, 2f, 2.01f);
        door.transform.localScale = new Vector3(1.5f, 4f, 0.2f);
        door.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.361f, 0.239f, 0.180f) };
        
        GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
        window.name = "Window";
        window.transform.SetParent(building.transform);
        window.transform.localPosition = new Vector3(0, 4f, -2.01f);
        window.transform.localScale = new Vector3(3f, 2f, 0.2f);
        window.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.682f, 0.839f, 0.945f, 0.7f) };
        
        GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "Sign";
        sign.transform.SetParent(building.transform);
        sign.transform.localPosition = new Vector3(0, 5.5f, 2.1f);
        sign.transform.localScale = new Vector3(3f, 0.8f, 0.3f);
        sign.GetComponent<Renderer>().material = new Material(shader) { color = accentColor };
        
        building.AddComponent<MaliGo.Environment.BuildingInteriorController>();
        DestroyColliders(building);
    }

    static void CreateArcade(Vector3 pos, Color roofColor, Color wallColor, Shader shader)
    {
        GameObject building = new GameObject("Building_Arcade");
        building.transform.position = pos;
        
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainStructure";
        main.transform.SetParent(building.transform);
        main.transform.localPosition = new Vector3(0, 3f, 0);
        main.transform.localScale = new Vector3(3.5f, 6f, 3.5f);
        main.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Roof";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0, 6.5f, 0);
        roof.transform.localScale = new Vector3(3.7f, 1f, 3.7f);
        roof.transform.rotation = Quaternion.Euler(15f, 45f, 0);
        roof.GetComponent<Renderer>().material = new Material(shader) { color = roofColor };
        
        GameObject neonSign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        neonSign.name = "NeonSign";
        neonSign.transform.SetParent(building.transform);
        neonSign.transform.localPosition = new Vector3(0, 5f, 1.8f);
        neonSign.transform.localScale = new Vector3(2.5f, 0.8f, 0.3f);
        
        Material neonMat = new Material(shader);
        neonMat.color = new Color(0.910f, 0.475f, 0.969f);
        neonMat.EnableKeyword("_EMISSION");
        neonMat.SetColor("_EmissionColor", new Color(0.910f, 0.475f, 0.969f) * 2f);
        neonSign.GetComponent<Renderer>().material = neonMat;
        
        for (int i = 0; i < 2; i++)
        {
            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = "Window_" + i;
            window.transform.SetParent(building.transform);
            window.transform.localPosition = new Vector3(-1f + i * 2f, 3.5f, 1.76f);
            window.transform.localScale = new Vector3(1f, 1.5f, 0.2f);
            
            Material winMat = new Material(shader);
            winMat.color = new Color(1f, 0.910f, 0.475f, 0.88f);
            winMat.EnableKeyword("_EMISSION");
            winMat.SetColor("_EmissionColor", new Color(1f, 0.910f, 0.475f) * 1.5f);
            window.GetComponent<Renderer>().material = winMat;
        }
        
        building.AddComponent<MaliGo.Environment.BuildingInteriorController>();
        DestroyColliders(building);
    }

    static void CreateHome(Vector3 pos, Color roofColor, Color roofDark, Color wallColor, Shader shader)
    {
        GameObject building = new GameObject("Building_Home");
        building.transform.position = pos;
        
        GameObject main = GameObject.CreatePrimitive(PrimitiveType.Cube);
        main.name = "MainStructure";
        main.transform.SetParent(building.transform);
        main.transform.localPosition = new Vector3(0, 2.5f, 0);
        main.transform.localScale = new Vector3(5f, 5f, 5f);
        main.GetComponent<Renderer>().material = new Material(shader) { color = wallColor };
        
        GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        roof.name = "Roof_Pyramid";
        roof.transform.SetParent(building.transform);
        roof.transform.localPosition = new Vector3(0, 5.5f, 0);
        roof.transform.localScale = new Vector3(6f, 2.5f, 6f);
        roof.GetComponent<Renderer>().material = new Material(shader) { color = roofColor };
        
        GameObject chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.name = "Chimney";
        chimney.transform.SetParent(building.transform);
        chimney.transform.localPosition = new Vector3(-1.5f, 6.5f, 0);
        chimney.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
        chimney.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.784f, 0.659f, 0.486f) };
        
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "Door";
        door.transform.SetParent(building.transform);
        door.transform.localPosition = new Vector3(0, 1.5f, 2.51f);
        door.transform.localScale = new Vector3(1.2f, 3f, 0.2f);
        door.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.545f, 0.369f, 0.235f) };
        
        Vector3[] windowPositions = new Vector3[]
        {
            new Vector3(-1.5f, 3f, 2.51f),
            new Vector3(1.5f, 3f, 2.51f),
            new Vector3(-2.51f, 3f, 0),
            new Vector3(2.51f, 3f, 0)
        };
        
        foreach (Vector3 winPos in windowPositions)
        {
            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = "Window";
            window.transform.SetParent(building.transform);
            window.transform.localPosition = winPos;
            window.transform.localScale = new Vector3(1f, 1.5f, 0.2f);
            window.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.682f, 0.839f, 0.945f, 0.74f) };
        }
        
        GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);
        step.name = "Doorstep";
        step.transform.SetParent(building.transform);
        step.transform.localPosition = new Vector3(0, 0.2f, 3f);
        step.transform.localScale = new Vector3(2f, 0.4f, 1f);
        step.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.784f, 0.753f, 0.659f) };
        
        GameObject path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "Pathway";
        path.transform.SetParent(building.transform);
        path.transform.localPosition = new Vector3(0, 0.01f, 4.5f);
        path.transform.localScale = new Vector3(1.5f, 0.01f, 2f);
        path.GetComponent<Renderer>().material = new Material(shader) { color = new Color(0.733f, 0.698f, 0.596f, 0.62f) };
        
        building.AddComponent<MaliGo.Environment.BuildingInteriorController>();
        DestroyColliders(building);
    }

    static void CreateTree(Vector3 pos, Color leafColor, Color trunkColor)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.position = pos;
        
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0, 1f, 0);
        trunk.transform.localScale = new Vector3(0.4f, 2f, 0.4f);
        trunk.GetComponent<Renderer>().material.color = trunkColor;
        
        GameObject leaves1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves1.name = "Leaves_1";
        leaves1.transform.SetParent(tree.transform);
        leaves1.transform.localPosition = new Vector3(0.3f, 2.5f, 0);
        leaves1.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        leaves1.GetComponent<Renderer>().material.color = new Color(0.039f, 0.322f, 0.094f);
        
        GameObject leaves2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves2.name = "Leaves_2";
        leaves2.transform.SetParent(tree.transform);
        leaves2.transform.localPosition = new Vector3(-0.2f, 3.2f, 0);
        leaves2.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        leaves2.GetComponent<Renderer>().material.color = leafColor;
        
        GameObject leaves3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaves3.name = "Leaves_3";
        leaves3.transform.SetParent(tree.transform);
        leaves3.transform.localPosition = new Vector3(0, 3.8f, 0);
        leaves3.transform.localScale = new Vector3(1f, 1f, 1f);
        leaves3.GetComponent<Renderer>().material.color = new Color(0.788f, 0.937f, 0.706f, 0.48f);
        
        DestroyColliders(tree);
    }

    static void CreateStreetLamp(Vector3 pos, Color poleColor)
    {
        GameObject lamp = new GameObject("StreetLamp");
        lamp.transform.position = pos;
        
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(lamp.transform);
        pole.transform.localPosition = new Vector3(0, 2.5f, 0);
        pole.transform.localScale = new Vector3(0.1f, 5f, 0.1f);
        pole.GetComponent<Renderer>().material.color = new Color(0.616f, 0.639f, 0.686f);
        
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.name = "Arm";
        arm.transform.SetParent(lamp.transform);
        arm.transform.localPosition = new Vector3(0.8f, 4.5f, 0);
        arm.transform.localScale = new Vector3(1.5f, 0.1f, 0.1f);
        arm.transform.rotation = Quaternion.Euler(0, 0, -15f);
        arm.GetComponent<Renderer>().material.color = new Color(0.616f, 0.639f, 0.686f);
        
        GameObject fixture = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fixture.name = "LightFixture";
        fixture.transform.SetParent(lamp.transform);
        fixture.transform.localPosition = new Vector3(1.2f, 4f, 0);
        fixture.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
        fixture.GetComponent<Renderer>().material.color = new Color(0.420f, 0.451f, 0.502f);
        
        GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = "LightGlow";
        glow.transform.SetParent(lamp.transform);
        glow.transform.localPosition = new Vector3(1.2f, 3.9f, 0);
        glow.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        
        Shader safeShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material glowMat = new Material(safeShader);
        glowMat.color = new Color(1f, 0.992f, 0.910f, 0.95f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(1f, 0.992f, 0.910f) * 2f);
        glow.GetComponent<Renderer>().material = glowMat;
        
        Light lampLight = lamp.AddComponent<Light>();
        lampLight.type = LightType.Point;
        lampLight.color = new Color(1f, 0.992f, 0.910f);
        lampLight.intensity = 1f;
        lampLight.range = 8f;
        lampLight.transform.localPosition = new Vector3(1.2f, 3.9f, 0);
        
        DestroyColliders(lamp);
    }

    static void CreateCharacter(Vector3 pos)
    {
        GameObject character = new GameObject("Character_Sam");
        character.transform.position = pos;
        character.tag = "Player";
        
        CharacterController controller = character.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0f, 1f, 0f);

        Rigidbody rb = character.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GameObject visual = new GameObject("SpriteVisual");
        visual.transform.SetParent(character.transform);
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

        MaliGoPlayerController playerController = character.AddComponent<MaliGoPlayerController>();
        playerController.spriteController = spriteController;
    }

    static void CreateMailbox(Vector3 pos, Color color)
    {
        GameObject mailbox = new GameObject("Mailbox");
        mailbox.transform.position = pos;
        
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Post";
        post.transform.SetParent(mailbox.transform);
        post.transform.localPosition = new Vector3(0, 1f, 0);
        post.transform.localScale = new Vector3(0.1f, 2f, 0.1f);
        post.GetComponent<Renderer>().material.color = new Color(0.616f, 0.639f, 0.686f);
        
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = "Box";
        box.transform.SetParent(mailbox.transform);
        box.transform.localPosition = new Vector3(0, 2.2f, 0);
        box.transform.localScale = new Vector3(0.6f, 0.4f, 0.8f);
        box.GetComponent<Renderer>().material.color = color;
        
        GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lid.name = "Lid";
        lid.transform.SetParent(mailbox.transform);
        lid.transform.localPosition = new Vector3(0, 2.5f, 0);
        lid.transform.localScale = new Vector3(0.65f, 0.15f, 0.85f);
        lid.transform.rotation = Quaternion.Euler(15f, 0, 0);
        lid.GetComponent<Renderer>().material.color = new Color(0.059f, 0.369f, 0.180f);
        
        DestroyColliders(mailbox);
    }

    static void CreateGardenPot(Vector3 pos, Color plantColor)
    {
        GameObject pot = new GameObject("GardenPot");
        pot.transform.position = pos;
        
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "PotBase";
        baseObj.transform.SetParent(pot.transform);
        baseObj.transform.localPosition = new Vector3(0, 0.3f, 0);
        baseObj.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        baseObj.GetComponent<Renderer>().material.color = new Color(0.627f, 0.439f, 0.314f);
        
        GameObject plant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        plant.name = "Plant";
        plant.transform.SetParent(pot.transform);
        plant.transform.localPosition = new Vector3(0, 0.8f, 0);
        plant.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        plant.GetComponent<Renderer>().material.color = plantColor;
        
        GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flower.name = "Flower";
        flower.transform.SetParent(pot.transform);
        flower.transform.localPosition = new Vector3(0.1f, 1.1f, 0);
        flower.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        flower.GetComponent<Renderer>().material.color = new Color(0.910f, 0.373f, 0.282f);
        
        DestroyColliders(pot);
    }

    static void CreateLighting(Color lightColor)
    {
        GameObject sun = new GameObject("DirectionalLight_Sun");
        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.color = lightColor;
        sunLight.intensity = 1.2f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.4f, 0.5f, 0.6f);
        RenderSettings.fog = false;
    }

    static void DestroyColliders(GameObject parent)
    {
        Collider[] colliders = parent.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            Object.DestroyImmediate(col);
        }
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        bool sceneExists = false;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == scenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                sceneExists = true;
                break;
            }
        }
        
        if (!sceneExists)
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }
        
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}