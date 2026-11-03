using UnityEngine;

/// <summary>
/// Builds an authentic 3D Tropical Lost Island environment with golden beaches,
/// lush palm groves, clear turquoise ocean, tiki ruin columns, natural coral rock obstacles,
/// and realistic faceted gemstone crystal clusters.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder Instance { get; private set; }

    [System.Serializable]
    public class LevelConfig
    {
        public int levelNumber;
        public string levelName;
        public float totalDistance;
        public int crystalCount;
        public int obstacleCount;
        public int movingHazardCount;
        public Color sandColor;
        public Color grassColor;
        public Color rockColor;
        public Color waterColor;
    }

    private GameObject currentLevelRoot;

    // Distinct realistic gemstone colors for crystals
    public static readonly Color[] GemstoneColors = new Color[]
    {
        new Color(0.12f, 0.95f, 1.0f),  // 💎 Celestial Aquamarine
        new Color(0.15f, 1.0f, 0.45f),  // 💚 Island Emerald
        new Color(1.0f, 0.84f, 0.15f),  // 💛 Golden Citrine / Topaz
        new Color(0.85f, 0.28f, 1.0f),  // 💜 Mystic Amethyst
        new Color(1.0f, 0.22f, 0.38f)   // ❤️ Island Fire Ruby
    };

    void Awake()
    {
        Instance = this;
    }

    public static LevelConfig GetConfig(int level)
    {
        switch (level)
        {
            case 1:
                return new LevelConfig
                {
                    levelNumber = 1,
                    levelName = "Tropical Lagoon Island",
                    totalDistance = 100f,
                    crystalCount = 5,
                    obstacleCount = 8,
                    movingHazardCount = 0,
                    sandColor = new Color(0.88f, 0.77f, 0.54f), // Warm Island Beach Sand
                    grassColor = new Color(0.24f, 0.72f, 0.30f), // Lush Palm Grass
                    rockColor = new Color(0.48f, 0.50f, 0.52f),  // Weathered Coral Stone
                    waterColor = new Color(0.12f, 0.65f, 0.88f, 0.85f) // Tropical Turquoise Ocean
                };
            case 2:
                return new LevelConfig
                {
                    levelNumber = 2,
                    levelName = "Coral Sandbar & Twin Isles",
                    totalDistance = 180f,
                    crystalCount = 7,
                    obstacleCount = 12,
                    movingHazardCount = 4,
                    sandColor = new Color(0.84f, 0.72f, 0.48f), // Deep Sandbar
                    grassColor = new Color(0.22f, 0.66f, 0.28f), // Island Thicket
                    rockColor = new Color(0.42f, 0.44f, 0.46f),  // Dark Coral Reef Rock
                    waterColor = new Color(0.08f, 0.58f, 0.84f, 0.85f)
                };
            case 3:
            default:
                return new LevelConfig
                {
                    levelNumber = 3,
                    levelName = "Ancient Tiki Temple Peak",
                    totalDistance = 260f,
                    crystalCount = 10,
                    obstacleCount = 18,
                    movingHazardCount = 8,
                    sandColor = new Color(0.80f, 0.68f, 0.46f), // Mountain Island Trail
                    grassColor = new Color(0.18f, 0.58f, 0.24f), // Jungle Summit
                    rockColor = new Color(0.38f, 0.38f, 0.42f),  // Sacred Ancient Stone
                    waterColor = new Color(0.06f, 0.52f, 0.80f, 0.85f)
                };
        }
    }

    public ExitPortal BuildLevel(int levelNumber, GameObject player)
    {
        if (currentLevelRoot != null)
        {
            Destroy(currentLevelRoot);
        }

        LevelConfig cfg = GetConfig(levelNumber);
        currentLevelRoot = new GameObject("Island_Level_" + levelNumber + "_" + cfg.levelName);

        Material matSand = CreateMat("IslandSandMat", cfg.sandColor, 0.1f, 0.35f);
        Material matGrass = CreateMat("IslandGrassMat", cfg.grassColor, 0.05f, 0.2f);
        Material matRock = CreateMat("CoralRockMat", cfg.rockColor, 0.15f, 0.4f);
        Material matWater = CreateWaterMat("OceanWaterMat", cfg.waterColor);
        Material matWood = CreateMat("PalmBarkMat", new Color(0.36f, 0.20f, 0.10f), 0.0f, 0.2f);
        Material matLeaves = CreateMat("PalmFrondMat", new Color(0.12f, 0.55f, 0.18f), 0.05f, 0.45f);

        // 1. Vast Tropical Ocean Water Plane
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ocean.name = "Tropical Ocean Water";
        ocean.transform.SetParent(currentLevelRoot.transform);
        ocean.transform.position = new Vector3(0, -1.75f, cfg.totalDistance * 0.5f);
        ocean.transform.localScale = new Vector3(180f, 0.2f, cfg.totalDistance + 140f);
        ocean.GetComponent<Renderer>().sharedMaterial = matWater;
        Destroy(ocean.GetComponent<BoxCollider>()); // No collider - falling into ocean is lethal!

        // 2. Continuous Island Beach & Pathway Segments
        float segmentLength = 20f;
        int numSegments = Mathf.CeilToInt(cfg.totalDistance / segmentLength) + 1;
        float pathWidth = Mathf.Max(9f, 15f - levelNumber * 1.8f);

        for (int i = 0; i <= numSegments; i++)
        {
            float zCenter = i * segmentLength;

            // Sandy beach island foundation
            GameObject sandBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sandBase.name = "Island_Beach_Base_" + i;
            sandBase.transform.SetParent(currentLevelRoot.transform);
            sandBase.transform.position = new Vector3(0, -0.6f, zCenter);
            sandBase.transform.localScale = new Vector3(pathWidth + 6f, 1.4f, segmentLength + 0.4f);
            sandBase.GetComponent<Renderer>().sharedMaterial = matSand;

            // Lush tropical green grass surface
            GameObject grassTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grassTop.name = "Island_Grass_Top_" + i;
            grassTop.transform.SetParent(currentLevelRoot.transform);
            grassTop.transform.position = new Vector3(0, 0.08f, zCenter);
            grassTop.transform.localScale = new Vector3(pathWidth, 0.15f, segmentLength + 0.4f);
            grassTop.GetComponent<Renderer>().sharedMaterial = matGrass;

            // Sea-washed border stones along beach edges
            GameObject stoneLeft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stoneLeft.name = "Coast_Rock_L_" + i;
            stoneLeft.transform.SetParent(currentLevelRoot.transform);
            stoneLeft.transform.position = new Vector3(-(pathWidth * 0.5f + 1.2f), 0.35f, zCenter);
            stoneLeft.transform.localScale = new Vector3(1.3f, 0.85f, 1.3f);
            stoneLeft.GetComponent<Renderer>().sharedMaterial = matRock;

            GameObject stoneRight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stoneRight.name = "Coast_Rock_R_" + i;
            stoneRight.transform.SetParent(currentLevelRoot.transform);
            stoneRight.transform.position = new Vector3(pathWidth * 0.5f + 1.2f, 0.35f, zCenter);
            stoneRight.transform.localScale = new Vector3(1.3f, 0.85f, 1.3f);
            stoneRight.GetComponent<Renderer>().sharedMaterial = matRock;

            // Coconut Palm Trees & Island Ruins
            if (i % 2 == 1)
            {
                float side = (i % 4 == 1) ? -1f : 1f;
                Vector3 treePos = new Vector3(side * (pathWidth * 0.5f + 3.2f), 0.1f, zCenter);
                BuildCoconutPalmTree(treePos, matWood, matLeaves, currentLevelRoot.transform);
            }
            else if (i % 4 == 2 && i > 0)
            {
                // Ancient Island Tiki Ruin Pillar
                float side = ((i / 4) % 2 == 0) ? 1f : -1f;
                Vector3 ruinPos = new Vector3(side * (pathWidth * 0.5f + 2.8f), 0.1f, zCenter);
                BuildIslandRuinPillar(ruinPos, matRock, currentLevelRoot.transform);
            }
        }

        // 3. Spawn Realistic Faceted Gemstone Crystals
        float crystalStep = (cfg.totalDistance - 16f) / cfg.crystalCount;
        for (int c = 1; c <= cfg.crystalCount; c++)
        {
            float zPos = 8f + c * crystalStep;
            float xOffset = Mathf.Sin(c * 1.8f) * (pathWidth * 0.32f);
            Vector3 crystalPos = new Vector3(xOffset, 1.1f, zPos);

            Color gemColor = GemstoneColors[(c - 1) % GemstoneColors.Length];

            GameObject crystalObj = new GameObject("Realistic_Crystal_" + c);
            crystalObj.transform.SetParent(currentLevelRoot.transform);
            crystalObj.transform.position = crystalPos;

            CrystalCollectible crystalComp = crystalObj.AddComponent<CrystalCollectible>();
            crystalComp.crystalColor = gemColor;
        }

        // 4. Spawn Lethal Obstacles (Touching ANY obstacle = INSTANT DEATH!)
        float obstacleStep = (cfg.totalDistance - 22f) / (cfg.obstacleCount + 1);
        int movingCount = 0;

        for (int o = 1; o <= cfg.obstacleCount; o++)
        {
            float zPos = 12f + o * obstacleStep;
            bool isMoving = (movingCount < cfg.movingHazardCount) && (o % 2 == 0);

            if (isMoving)
            {
                movingCount++;
                // Dynamic Rolling Tidal Boulder Hazard
                GameObject movingHazard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                movingHazard.name = "Rolling_Tidal_Boulder_" + o;
                movingHazard.tag = "Obstacle";
                movingHazard.transform.SetParent(currentLevelRoot.transform);
                movingHazard.transform.position = new Vector3(0, 0.85f, zPos);
                movingHazard.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
                
                Material hazardMat = CreateMat("HazardMat", new Color(0.95f, 0.25f, 0.15f), 0.2f, 0.7f);
                hazardMat.EnableKeyword("_EMISSION");
                hazardMat.SetColor("_EmissionColor", new Color(0.8f, 0.15f, 0.05f) * 1.8f);
                movingHazard.GetComponent<Renderer>().sharedMaterial = hazardMat;

                Obstacle obs = movingHazard.AddComponent<Obstacle>();
                obs.hazardName = "Crushing Rolling Island Boulder";
                obs.isMoving = true;
                obs.moveDirection = Vector3.right;
                obs.moveDistance = pathWidth * 0.78f;
                obs.moveSpeed = 2.6f + levelNumber * 0.75f;
                obs.isRotating = true;
                obs.rotationSpeed = new Vector3(0, 160f, 0);
            }
            else
            {
                // Static Jagged Coral Rock / Fallen Palm Hazard
                float xOffset = ((o % 3) - 1) * (pathWidth * 0.28f);

                if (o % 3 == 0)
                {
                    // Fallen Palm Log Barrier
                    GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    log.name = "Fallen_Log_Obstacle_" + o;
                    log.tag = "Obstacle";
                    log.transform.SetParent(currentLevelRoot.transform);
                    log.transform.position = new Vector3(xOffset, 0.35f, zPos);
                    log.transform.localScale = new Vector3(0.55f, 2.2f, 0.55f);
                    log.transform.rotation = Quaternion.Euler(0, 0, 90f);
                    log.GetComponent<Renderer>().sharedMaterial = matWood;

                    Obstacle obs = log.AddComponent<Obstacle>();
                    obs.hazardName = "Fallen Island Log Barrier";
                }
                else
                {
                    // Craggy Coral Spire Rock
                    GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rock.name = "Jagged_Coral_Rock_" + o;
                    rock.tag = "Obstacle";
                    rock.transform.SetParent(currentLevelRoot.transform);
                    rock.transform.position = new Vector3(xOffset, 0.7f, zPos);
                    rock.transform.localScale = new Vector3(1.75f, 1.4f, 1.5f);
                    rock.transform.rotation = Quaternion.Euler(15f, (o * 53f) % 360f, 12f);
                    rock.GetComponent<Renderer>().sharedMaterial = matRock;

                    Obstacle obs = rock.AddComponent<Obstacle>();
                    obs.hazardName = "Lethal Jagged Coral Rock";
                }
            }
        }

        // 5. Ancient Island Exit Portal (At End of Island)
        Vector3 portalPos = new Vector3(0, 0, cfg.totalDistance);
        GameObject portalObj = new GameObject("Island_Exit_Portal");
        portalObj.transform.SetParent(currentLevelRoot.transform);
        portalObj.transform.position = portalPos;

        Material matPortalGlow = CreateMat("PortalSwirlMat", new Color(0.9f, 0.15f, 0.15f), 0.2f, 0.9f);
        matPortalGlow.EnableKeyword("_EMISSION");
        matPortalGlow.SetColor("_EmissionColor", new Color(0.9f, 0.15f, 0.15f) * 2.2f);

        // Center Swirling Portal Ring
        GameObject portalGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        portalGlow.name = "Portal_Swirl_Center";
        portalGlow.transform.SetParent(portalObj.transform);
        portalGlow.transform.position = portalPos + new Vector3(0, 1.9f, 0);
        portalGlow.transform.localScale = new Vector3(1.8f, 0.12f, 1.8f);
        portalGlow.transform.rotation = Quaternion.Euler(90f, 0, 0);
        portalGlow.GetComponent<Renderer>().sharedMaterial = matPortalGlow;

        // Ancient Tiki Arch Pillars
        GameObject archL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        archL.transform.SetParent(portalObj.transform);
        archL.transform.position = portalPos + new Vector3(-1.45f, 1.9f, 0);
        archL.transform.localScale = new Vector3(0.28f, 1.9f, 0.28f);
        archL.GetComponent<Renderer>().sharedMaterial = matRock;

        GameObject archR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        archR.transform.SetParent(portalObj.transform);
        archR.transform.position = portalPos + new Vector3(1.45f, 1.9f, 0);
        archR.transform.localScale = new Vector3(0.28f, 1.9f, 0.28f);
        archR.GetComponent<Renderer>().sharedMaterial = matRock;

        GameObject archTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        archTop.transform.SetParent(portalObj.transform);
        archTop.transform.position = portalPos + new Vector3(0, 3.8f, 0);
        archTop.transform.localScale = new Vector3(0.28f, 1.45f, 0.28f);
        archTop.transform.rotation = Quaternion.Euler(0, 0, 90f);
        archTop.GetComponent<Renderer>().sharedMaterial = matRock;

        BoxCollider portalCollider = portalObj.AddComponent<BoxCollider>();
        portalCollider.isTrigger = true;
        portalCollider.size = new Vector3(3.8f, 4.2f, 2.2f);
        portalCollider.center = new Vector3(0, 1.9f, 0);

        ExitPortal exitPortal = portalObj.AddComponent<ExitPortal>();
        exitPortal.portalRenderer = portalGlow.GetComponent<Renderer>();

        // 6. Reset Player Position to the Island Start
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ResetPosition(new Vector3(0, 1.2f, -2f));
            }
            else
            {
                player.transform.position = new Vector3(0, 1.2f, -2f);
            }
        }

        return exitPortal;
    }

    /// <summary>
    /// Builds a multi-frond coconut palm tree on the island beach.
    /// </summary>
    private void BuildCoconutPalmTree(Vector3 pos, Material matWood, Material matLeaves, Transform parent)
    {
        GameObject tree = new GameObject("Island_Palm_Tree");
        tree.transform.SetParent(parent);
        tree.transform.position = pos;

        // Curved trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Palm_Trunk";
        trunk.transform.SetParent(tree.transform);
        trunk.transform.position = pos + Vector3.up * 2.0f;
        trunk.transform.localScale = new Vector3(0.38f, 2.0f, 0.38f);
        trunk.transform.rotation = Quaternion.Euler(6f, Random.Range(0, 360), 4f);
        trunk.GetComponent<Renderer>().sharedMaterial = matWood;

        Vector3 crownPos = pos + Vector3.up * 4.0f;

        // 6 sprawling palm fronds
        for (int f = 0; f < 6; f++)
        {
            float angle = f * 60f;
            GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frond.name = "Palm_Frond_" + f;
            frond.transform.SetParent(tree.transform);
            frond.transform.position = crownPos + Quaternion.Euler(0, angle, 0) * new Vector3(0, -0.3f, 1.2f);
            frond.transform.localScale = new Vector3(0.55f, 0.08f, 2.4f);
            frond.transform.rotation = Quaternion.Euler(22f, angle, 0);
            frond.GetComponent<Renderer>().sharedMaterial = matLeaves;
        }

        // Coconuts
        Material matCoconut = CreateMat("CoconutMat", new Color(0.28f, 0.16f, 0.08f), 0.1f, 0.3f);
        for (int c = 0; c < 3; c++)
        {
            float cAngle = c * 120f;
            GameObject coconut = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coconut.name = "Coconut_" + c;
            coconut.transform.SetParent(tree.transform);
            coconut.transform.position = crownPos + Quaternion.Euler(0, cAngle, 0) * new Vector3(0.25f, -0.2f, 0.25f);
            coconut.transform.localScale = Vector3.one * 0.32f;
            coconut.GetComponent<Renderer>().sharedMaterial = matCoconut;
            Destroy(coconut.GetComponent<Collider>());
        }
    }

    /// <summary>
    /// Builds an ancient mossy island stone ruin column.
    /// </summary>
    private void BuildIslandRuinPillar(Vector3 pos, Material matStone, Transform parent)
    {
        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Island_Ruin_Pillar";
        pillar.transform.SetParent(parent);
        pillar.transform.position = pos + Vector3.up * 1.8f;
        pillar.transform.localScale = new Vector3(0.85f, 1.8f, 0.85f);
        pillar.GetComponent<Renderer>().sharedMaterial = matStone;
    }

    private Material CreateMat(string name, Color color, float metallic = 0.1f, float glossiness = 0.4f)
    {
        Shader s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Diffuse");

        Material m = new Material(s);
        m.name = name;
        m.color = color;
        m.SetFloat("_Metallic", metallic);
        m.SetFloat("_Glossiness", glossiness);
        return m;
    }

    private Material CreateWaterMat(string name, Color color)
    {
        Material m = CreateMat(name, color, 0.2f, 0.95f);
        m.SetFloat("_Mode", 3); // Transparent
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        return m;
    }
}
