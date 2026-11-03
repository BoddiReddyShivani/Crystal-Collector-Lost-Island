using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver, LevelComplete, GameVictory }
    public GameState currentState { get; private set; } = GameState.Menu;

    public bool IsPlaying => currentState == GameState.Playing;

    [Header("Game Progression")]
    public int currentLevel = 1;
    public const int MaxLevels = 3;
    public int totalCrystals { get; private set; } = 5;
    public int collected { get; private set; } = 0;
    public int totalCrystalsGameWide { get; private set; } = 0;
    public float targetDistance { get; private set; } = 100f;

    [Header("References")]
    public PlayerController player;
    public ExitPortal portal;
    public LevelBuilder levelBuilder;

    [Header("Settings")]
    public bool autoRestartOnDeath = false;

    private string deathReason = "";
    private string hintMessage = "";
    private float hintTimer = 0f;
    private float deathTimer = 0f;

    public static T FindAny<T>() where T : Object
    {
        return Object.FindAnyObjectByType<T>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SoundManager.Ensure();

        if (levelBuilder == null)
        {
            levelBuilder = FindAny<LevelBuilder>();
            if (levelBuilder == null)
            {
                levelBuilder = gameObject.AddComponent<LevelBuilder>();
            }
        }

        if (player == null)
        {
            player = FindAny<PlayerController>();
        }

        Time.timeScale = 0f;
        currentState = GameState.Menu;
    }

    void Start()
    {
        EnsurePlayerAndCamera();
    }

    private void EnsurePlayerAndCamera()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindWithTag("Player");
            if (pObj != null)
                player = pObj.GetComponent<PlayerController>();
        }
    }

    public void StartGame(int startLevel = 1)
    {
        currentLevel = Mathf.Clamp(startLevel, 1, MaxLevels);
        LoadLevel(currentLevel);
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        SoundManager.Play("click");
    }

    public void LoadLevel(int level)
    {
        currentLevel = level;
        collected = 0;

        EnsurePlayerAndCamera();
        LevelBuilder.LevelConfig cfg = LevelBuilder.GetConfig(level);
        totalCrystals = cfg.crystalCount;
        targetDistance = cfg.totalDistance;

        if (levelBuilder != null && player != null)
        {
            portal = levelBuilder.BuildLevel(level, player.gameObject);
        }

        if (player != null)
        {
            player.SetDead(false);
            player.ResetPosition(new Vector3(0, 1.2f, -2f));
        }

        currentState = GameState.Playing;
        Time.timeScale = 1f;
        ShowHint("Level " + level + ": " + cfg.levelName + " - Collect " + totalCrystals + " Crystals!");
    }

    public void CollectCrystal()
    {
        if (currentState != GameState.Playing) return;

        collected++;
        totalCrystalsGameWide++;
        SoundManager.Play("collect");

        if (collected >= totalCrystals)
        {
            if (portal != null)
            {
                portal.Unlock();
            }
            SoundManager.Play("unlock");
            ShowHint("All Crystals Collected! Exit Portal is now UNLOCKED!");
        }
        else
        {
            int remaining = totalCrystals - collected;
            ShowHint("Crystal Collected! (" + collected + "/" + totalCrystals + ") - " + remaining + " remaining!");
        }
    }

    public void KillPlayer(string reason)
    {
        if (currentState != GameState.Playing) return;

        deathReason = reason;
        currentState = GameState.GameOver;
        deathTimer = 0f;

        SoundManager.Play("death");

        FollowCamera cam = FindAny<FollowCamera>();
        if (cam != null) cam.Shake(0.5f, 0.45f);

        if (player != null)
        {
            player.SetDead(true);
        }

        Time.timeScale = 0f;
    }

    public void CompleteLevel()
    {
        if (currentState != GameState.Playing) return;

        SoundManager.Play("win");
        Time.timeScale = 0f;

        if (currentLevel >= MaxLevels)
        {
            currentState = GameState.GameVictory;
        }
        else
        {
            currentState = GameState.LevelComplete;
        }
    }

    public void NextLevel()
    {
        SoundManager.Play("click");
        if (currentLevel < MaxLevels)
        {
            LoadLevel(currentLevel + 1);
        }
        else
        {
            currentState = GameState.GameVictory;
        }
    }

    public void RestartCurrentLevel()
    {
        SoundManager.Play("click");
        Time.timeScale = 1f;
        LoadLevel(currentLevel);
    }

    public void TogglePause()
    {
        SoundManager.Play("click");
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    public void ReturnToMenu()
    {
        SoundManager.Play("click");
        currentState = GameState.Menu;
        Time.timeScale = 0f;
    }

    public void EndGame()
    {
        SoundManager.Play("click");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ShowHint(string msg, float duration = 3.5f)
    {
        hintMessage = msg;
        hintTimer = duration;
    }

    void Update()
    {
        // Pause hotkey
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing || currentState == GameState.Paused)
            {
                TogglePause();
            }
        }

        // Restart hotkey when dead or won
        if ((currentState == GameState.GameOver || currentState == GameState.LevelComplete || currentState == GameState.GameVictory) && Input.GetKeyDown(KeyCode.R))
        {
            RestartCurrentLevel();
        }

        if (currentState == GameState.GameOver && autoRestartOnDeath)
        {
            deathTimer += Time.unscaledDeltaTime;
            if (deathTimer >= 2.0f)
            {
                RestartCurrentLevel();
            }
        }

        if (hintTimer > 0)
        {
            hintTimer -= Time.unscaledDeltaTime;
            if (hintTimer <= 0) hintMessage = "";
        }
    }

    void OnGUI()
    {
        // Custom UI Styles
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = Color.yellow;

        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };
        subtitleStyle.normal.textColor = Color.white;

        GUIStyle hudStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold
        };
        hudStyle.normal.textColor = Color.white;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        // ==================== HUD (During Active Gameplay) ====================
        if (currentState == GameState.Playing)
        {
            float playerDist = (player != null) ? player.distanceTraveled : 0f;

            // 1. Crystals Counter & Distance HUD Box
            GUI.Box(new Rect(20, 20, 310, 85), "", GUI.skin.box);
            GUI.Label(new Rect(30, 25, 290, 35), "💎 Crystals: " + collected + " / " + totalCrystals, hudStyle);
            GUI.Label(new Rect(30, 62, 290, 35), "🏃 Distance: " + playerDist.ToString("F0") + "m / " + targetDistance.ToString("F0") + "m", hudStyle);

            // 2. Level Badge (Top Center)
            GUI.Box(new Rect(Screen.width / 2 - 130, 20, 260, 40), "🏝️ Level " + currentLevel + " / " + MaxLevels + ": " + LevelBuilder.GetConfig(currentLevel).levelName, hudStyle);

            // 3. Control Hints (Bottom Left)
            GUI.Box(new Rect(20, Screen.height - 55, 480, 38), "WASD/Arrows: Move | SHIFT: Sprint | SPACE: Jump | M: Mute", hudStyle);

            // 4. Action Buttons (Top Right)
            // Pause Button
            if (GUI.Button(new Rect(Screen.width - 230, 20, 100, 42), "⏸️ Pause", buttonStyle))
            {
                TogglePause();
            }

            // Sound Toggle Button
            string muteText = SoundManager.IsMuted ? "🔇 Unmute" : "🔊 Sound";
            if (GUI.Button(new Rect(Screen.width - 120, 20, 100, 42), muteText, buttonStyle))
            {
                SoundManager.ToggleMute();
            }

            // Floating Hint Notification
            if (!string.IsNullOrEmpty(hintMessage) && hintTimer > 0)
            {
                GUI.Box(new Rect(Screen.width / 2 - 280, 75, 560, 45), hintMessage, subtitleStyle);
            }

            return;
        }

        // Dark dim backdrop for menus/overlays
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

        // ==================== 1. START GAME MENU ====================
        if (currentState == GameState.Menu)
        {
            float panelW = 520;
            float panelH = 510;
            Rect panel = new Rect(Screen.width / 2 - panelW / 2, Screen.height / 2 - panelH / 2, panelW, panelH);
            GUI.Box(panel, "");

            GUI.Label(new Rect(panel.x, panel.y + 20, panelW, 50), "CRYSTAL COLLECTOR", titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 68, panelW, 35), "🏝️ The Lost Island 3D Adventure", subtitleStyle);

            GUI.Label(new Rect(panel.x + 30, panel.y + 115, panelW - 60, 80),
                "• Explore the island, collect crystals, and reach the exit portal.\n" +
                "• ⚠️ WARNING: Touching ANY obstacle causes INSTANT DEATH!\n" +
                "• Controls: WASD/Arrows to Move, SHIFT to Sprint, SPACE to Jump.", subtitleStyle);

            // Start Game Button
            if (GUI.Button(new Rect(panel.x + 70, panel.y + 215, panelW - 140, 52), "▶ START GAME (Level 1)", buttonStyle))
            {
                StartGame(1);
            }

            // Level Select Buttons
            GUI.Label(new Rect(panel.x, panel.y + 280, panelW, 25), "— SELECT STARTING LEVEL —", subtitleStyle);
            if (GUI.Button(new Rect(panel.x + 40, panel.y + 315, 135, 45), "Level 1 (100m)", buttonStyle))
            {
                StartGame(1);
            }
            if (GUI.Button(new Rect(panel.x + 190, panel.y + 315, 140, 45), "Level 2 (180m)", buttonStyle))
            {
                StartGame(2);
            }
            if (GUI.Button(new Rect(panel.x + 345, panel.y + 315, 135, 45), "Level 3 (260m)", buttonStyle))
            {
                StartGame(3);
            }

            // End Game / Quit Button
            if (GUI.Button(new Rect(panel.x + 120, panel.y + 380, panelW - 240, 45), "🛑 END GAME (Quit)", buttonStyle))
            {
                EndGame();
            }

            // Sound Toggle in Menu
            string soundStatus = SoundManager.IsMuted ? "Sound: MUTED (Click to Enable)" : "Sound: ON (Click to Mute)";
            if (GUI.Button(new Rect(panel.x + 100, panel.y + 440, panelW - 200, 38), soundStatus))
            {
                SoundManager.ToggleMute();
            }
        }

        // ==================== 2. PAUSE MENU ====================
        else if (currentState == GameState.Paused)
        {
            float panelW = 440;
            float panelH = 380;
            Rect panel = new Rect(Screen.width / 2 - panelW / 2, Screen.height / 2 - panelH / 2, panelW, panelH);
            GUI.Box(panel, "");

            GUI.Label(new Rect(panel.x, panel.y + 25, panelW, 45), "⏸️ GAME PAUSED", titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 75, panelW, 30), "Current Progress: " + collected + " / " + totalCrystals + " Crystals", subtitleStyle);

            if (GUI.Button(new Rect(panel.x + 60, panel.y + 120, panelW - 120, 48), "▶ RESUME GAME", buttonStyle))
            {
                TogglePause();
            }

            if (GUI.Button(new Rect(panel.x + 60, panel.y + 180, panelW - 120, 48), "🔄 RESTART LEVEL", buttonStyle))
            {
                RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(panel.x + 60, panel.y + 240, panelW - 120, 48), "🏠 MAIN MENU", buttonStyle))
            {
                ReturnToMenu();
            }

            if (GUI.Button(new Rect(panel.x + 60, panel.y + 300, panelW - 120, 48), "🛑 END GAME (Quit)", buttonStyle))
            {
                EndGame();
            }
        }

        // ==================== 3. GAME OVER (INSTANT DEATH) ====================
        else if (currentState == GameState.GameOver)
        {
            float panelW = 480;
            float panelH = 390;
            Rect panel = new Rect(Screen.width / 2 - panelW / 2, Screen.height / 2 - panelH / 2, panelW, panelH);
            GUI.Box(panel, "");

            titleStyle.normal.textColor = new Color(1f, 0.25f, 0.25f);
            GUI.Label(new Rect(panel.x, panel.y + 20, panelW, 50), "💀 YOU DIED!", titleStyle);

            GUI.Label(new Rect(panel.x + 20, panel.y + 75, panelW - 40, 35), "Cause of Death: " + deathReason, subtitleStyle);
            float dist = (player != null) ? player.distanceTraveled : 0f;
            GUI.Label(new Rect(panel.x + 20, panel.y + 115, panelW - 40, 35), "Distance Traveled: " + dist.ToString("F0") + "m  |  Crystals: " + collected + " / " + totalCrystals, subtitleStyle);
            GUI.Label(new Rect(panel.x + 20, panel.y + 155, panelW - 40, 30), "Remember: Any obstacle contact is lethal!", subtitleStyle);

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 195, panelW - 140, 52), "🔄 TRY AGAIN (Restart)", buttonStyle))
            {
                RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 260, panelW - 140, 46), "🏠 MAIN MENU", buttonStyle))
            {
                ReturnToMenu();
            }

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 320, panelW - 140, 46), "🛑 END GAME (Quit)", buttonStyle))
            {
                EndGame();
            }
        }

        // ==================== 4. LEVEL COMPLETE ====================
        else if (currentState == GameState.LevelComplete)
        {
            float panelW = 480;
            float panelH = 380;
            Rect panel = new Rect(Screen.width / 2 - panelW / 2, Screen.height / 2 - panelH / 2, panelW, panelH);
            GUI.Box(panel, "");

            titleStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);
            GUI.Label(new Rect(panel.x, panel.y + 20, panelW, 50), "🎉 LEVEL " + currentLevel + " COMPLETED!", titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 75, panelW, 35), "All " + totalCrystals + " Crystals Collected!", subtitleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 115, panelW, 35), "Island Escaped Through The Portal!", subtitleStyle);

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 165, panelW - 140, 52), "▶ NEXT LEVEL >>", buttonStyle))
            {
                NextLevel();
            }

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 230, panelW - 140, 46), "🔄 REPLAY THIS LEVEL", buttonStyle))
            {
                RestartCurrentLevel();
            }

            if (GUI.Button(new Rect(panel.x + 70, panel.y + 290, panelW - 140, 46), "🏠 MAIN MENU", buttonStyle))
            {
                ReturnToMenu();
            }
        }

        // ==================== 5. GRAND GAME VICTORY ====================
        else if (currentState == GameState.GameVictory)
        {
            float panelW = 540;
            float panelH = 420;
            Rect panel = new Rect(Screen.width / 2 - panelW / 2, Screen.height / 2 - panelH / 2, panelW, panelH);
            GUI.Box(panel, "");

            titleStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(panel.x, panel.y + 20, panelW, 50), "🏆 CONGRATULATIONS! 🏆", titleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 75, panelW, 35), "You Conquered All 3 Island Levels!", subtitleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 115, panelW, 35), "Total Crystals Found: " + totalCrystalsGameWide, subtitleStyle);
            GUI.Label(new Rect(panel.x, panel.y + 150, panelW, 35), "Total Distance Conquered: 540m+", subtitleStyle);

            if (GUI.Button(new Rect(panel.x + 90, panel.y + 205, panelW - 180, 52), "🔄 PLAY AGAIN (Level 1)", buttonStyle))
            {
                StartGame(1);
            }

            if (GUI.Button(new Rect(panel.x + 90, panel.y + 270, panelW - 180, 48), "🏠 MAIN MENU", buttonStyle))
            {
                ReturnToMenu();
            }

            if (GUI.Button(new Rect(panel.x + 90, panel.y + 330, panelW - 180, 48), "🛑 END GAME (Quit)", buttonStyle))
            {
                EndGame();
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void AutoBootstrap()
    {
        if (FindAny<GameManager>() == null)
        {
            // Ensure directional light
            if (FindAny<Light>() == null)
            {
                GameObject sun = new GameObject("Directional Light");
                Light light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.3f;
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                RenderSettings.ambientLight = new Color(0.65f, 0.72f, 0.80f);
            }

            // Ensure player
            GameObject playerObj = GameObject.FindWithTag("Player");
            PlayerController pc = null;
            if (playerObj == null)
            {
                playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerObj.name = "Player";
                playerObj.tag = "Player";
                playerObj.transform.position = new Vector3(0f, 1.2f, -2f);
                Destroy(playerObj.GetComponent<CapsuleCollider>());
                CharacterController cc = playerObj.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.radius = 0.45f;
                cc.center = new Vector3(0f, 1f, 0f);
                pc = playerObj.AddComponent<PlayerController>();

                Shader sh = Shader.Find("Standard");
                if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
                if (sh == null) sh = Shader.Find("Diffuse");
                Material pMat = new Material(sh);
                pMat.color = new Color(0.15f, 0.48f, 0.95f);
                playerObj.GetComponent<Renderer>().sharedMaterial = pMat;
            }
            else
            {
                pc = playerObj.GetComponent<PlayerController>();
            }

            // Ensure camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                FollowCamera follow = camObj.AddComponent<FollowCamera>();
                follow.target = playerObj.transform;
                if (pc != null) pc.cameraTransform = camObj.transform;
            }
            else
            {
                FollowCamera follow = mainCam.GetComponent<FollowCamera>();
                if (follow == null) follow = mainCam.gameObject.AddComponent<FollowCamera>();
                follow.target = playerObj.transform;
                if (pc != null) pc.cameraTransform = mainCam.transform;
            }

            SoundManager.Ensure();

            // Ensure GameManager & LevelBuilder
            GameObject gmObj = new GameObject("GameManager");
            LevelBuilder lb = gmObj.AddComponent<LevelBuilder>();
            GameManager gm = gmObj.AddComponent<GameManager>();
            gm.player = pc;
            gm.levelBuilder = lb;

            gm.portal = lb.BuildLevel(1, playerObj);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
