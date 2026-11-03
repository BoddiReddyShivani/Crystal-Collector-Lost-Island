using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class IslandSceneBuilder
{
    [MenuItem("Crystal Collector/Build Demo Scene")]
    public static void BuildDemoScene()
    {
        BuildLevelScene(1);
    }

    [MenuItem("Crystal Collector/Build Level 1 Scene")]
    public static void BuildLevel1()
    {
        BuildLevelScene(1);
    }

    [MenuItem("Crystal Collector/Build Level 2 Scene")]
    public static void BuildLevel2()
    {
        BuildLevelScene(2);
    }

    [MenuItem("Crystal Collector/Build Level 3 Scene")]
    public static void BuildLevel3()
    {
        BuildLevelScene(3);
    }

    public static void BuildLevelScene(int levelNumber)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Material playerMat = Mat("Player Blue", new Color(0.15f, 0.48f, 0.95f));

        // 1. Lighting & Environment
        GameObject sun = new GameObject("Directional Light");
        Light light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.ambientLight = new Color(0.65f, 0.72f, 0.80f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.65f, 0.80f, 0.92f);
        RenderSettings.fogDensity = 0.005f;

        // 2. Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.2f, -2f);
        player.transform.localScale = Vector3.one;
        player.GetComponent<Renderer>().sharedMaterial = playerMat;

        // Remove standard CapsuleCollider and replace with CharacterController
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.45f;
        cc.center = new Vector3(0f, 1f, 0f);

        PlayerController pc = player.AddComponent<PlayerController>();

        // 3. Camera Setup
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 60f;
        cam.clearFlags = CameraClearFlags.Skybox;
        camObj.AddComponent<AudioListener>();

        FollowCamera follow = camObj.AddComponent<FollowCamera>();
        follow.target = player.transform;
        pc.cameraTransform = camObj.transform;

        camObj.transform.position = player.transform.position + follow.offset;
        camObj.transform.LookAt(player.transform.position + Vector3.up * 1.2f);

        // 4. Managers
        SoundManager.Ensure();

        GameObject manager = new GameObject("GameManager");
        LevelBuilder lb = manager.AddComponent<LevelBuilder>();
        GameManager gm = manager.AddComponent<GameManager>();
        gm.player = pc;
        gm.levelBuilder = lb;

        // 5. Build Initial Island Geometry
        ExitPortal exitPortal = lb.BuildLevel(levelNumber, player);
        gm.portal = exitPortal;

        // 6. Save Scene
        Directory.CreateDirectory("Assets/Scenes");
        string scenePath = "Assets/Scenes/LostIsland.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        Selection.activeGameObject = player;
        Debug.Log("Crystal Collector Lost Island - Scene generated successfully at " + scenePath + "!");
    }

    static Material Mat(string name, Color color)
    {
        Shader s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Diffuse");
        Material m = new Material(s);
        m.name = name;
        m.color = color;
        return m;
    }
}
