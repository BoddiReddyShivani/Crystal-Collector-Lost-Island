using UnityEngine;

/// <summary>
/// Realistic 3D Faceted Crystal Collectible.
/// Generates realistic hexagonal quartz crystal geometry, gemstone cluster shards,
/// glowing point light, faceted specular reflection, and collection burst particles.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CrystalCollectible : MonoBehaviour
{
    [Header("Crystal Appearance")]
    public Color crystalColor = new Color(0.1f, 0.95f, 1.0f);
    public float crystalHeight = 1.3f;
    public float crystalRadius = 0.35f;

    [Header("Animation")]
    public float spinSpeed = 65f;
    public float bobSpeed = 2.2f;
    public float bobHeight = 0.25f;

    private Vector3 startPosition;
    private bool isCollected = false;
    private GameObject clusterParent;

    void Awake()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(1.4f, 2.0f, 1.4f);
        col.center = new Vector3(0, 0.2f, 0);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Start()
    {
        startPosition = transform.position;
        BuildRealisticCrystalCluster();
    }

    public void SetColor(Color col)
    {
        crystalColor = col;
        if (clusterParent != null)
        {
            Destroy(clusterParent);
            BuildRealisticCrystalCluster();
        }
    }

    private void BuildRealisticCrystalCluster()
    {
        clusterParent = new GameObject("CrystalVisuals");
        clusterParent.transform.SetParent(transform, false);
        clusterParent.transform.localPosition = Vector3.zero;

        Material crystalMat = CreateGemstoneMaterial(crystalColor);

        // 1. Main Center Crystal Spire (Tall & Faceted)
        GameObject mainSpire = new GameObject("MainCrystalSpire");
        mainSpire.transform.SetParent(clusterParent.transform, false);
        MeshFilter mfMain = mainSpire.AddComponent<MeshFilter>();
        mfMain.sharedMesh = GenerateFacetedCrystalMesh(crystalHeight, crystalRadius, 0.55f);
        MeshRenderer mrMain = mainSpire.AddComponent<MeshRenderer>();
        mrMain.sharedMaterial = crystalMat;

        // 2. Secondary Crystal Shard 1 (Tilted 25 degrees)
        GameObject shard1 = new GameObject("CrystalShard_1");
        shard1.transform.SetParent(clusterParent.transform, false);
        shard1.transform.localPosition = new Vector3(0.22f, -0.2f, 0.15f);
        shard1.transform.localRotation = Quaternion.Euler(18f, 35f, -22f);
        MeshFilter mf1 = shard1.AddComponent<MeshFilter>();
        mf1.sharedMesh = GenerateFacetedCrystalMesh(crystalHeight * 0.65f, crystalRadius * 0.7f, 0.35f);
        MeshRenderer mr1 = shard1.AddComponent<MeshRenderer>();
        mr1.sharedMaterial = crystalMat;

        // 3. Secondary Crystal Shard 2 (Tilted opposite)
        GameObject shard2 = new GameObject("CrystalShard_2");
        shard2.transform.SetParent(clusterParent.transform, false);
        shard2.transform.localPosition = new Vector3(-0.2f, -0.25f, -0.15f);
        shard2.transform.localRotation = Quaternion.Euler(-15f, -40f, 24f);
        MeshFilter mf2 = shard2.AddComponent<MeshFilter>();
        mf2.sharedMesh = GenerateFacetedCrystalMesh(crystalHeight * 0.5f, crystalRadius * 0.6f, 0.28f);
        MeshRenderer mr2 = shard2.AddComponent<MeshRenderer>();
        mr2.sharedMaterial = crystalMat;

        // 4. Glowing Magical Point Light
        GameObject lightObj = new GameObject("CrystalLight");
        lightObj.transform.SetParent(clusterParent.transform, false);
        lightObj.transform.localPosition = new Vector3(0, 0.2f, 0);
        Light pointLight = lightObj.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = crystalColor;
        pointLight.range = 4.0f;
        pointLight.intensity = 2.2f;
    }

    void Update()
    {
        // Smooth hover bobbing and rotation
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        if (clusterParent != null)
        {
            clusterParent.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
            // Slight sparkling wobble to catch sunlight
            float wobbleZ = Mathf.Sin(Time.time * 2.5f) * 4f;
            clusterParent.transform.localRotation = Quaternion.Euler(0, clusterParent.transform.localEulerAngles.y, wobbleZ);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected || !other.CompareTag("Player")) return;
        isCollected = true;

        SpawnCollectionBurst();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectCrystal();
        }
        else
        {
            SoundManager.Play("collect");
        }

        Destroy(gameObject);
    }

    private void SpawnCollectionBurst()
    {
        GameObject burst = new GameObject("CrystalCollectionBurst");
        burst.transform.position = transform.position;

        Material burstMat = CreateGemstoneMaterial(crystalColor);

        // Spawn 8 miniature sparkling gem shards that burst outward
        for (int i = 0; i < 8; i++)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spark.name = "Spark";
            spark.transform.SetParent(burst.transform);
            spark.transform.position = transform.position + Random.insideUnitSphere * 0.25f;
            spark.transform.localScale = Vector3.one * Random.Range(0.12f, 0.22f);
            spark.GetComponent<Renderer>().sharedMaterial = burstMat;
            Destroy(spark.GetComponent<Collider>());

            Rigidbody srb = spark.AddComponent<Rigidbody>();
            srb.useGravity = true;
            srb.linearVelocity = (Random.insideUnitSphere + Vector3.up * 1.5f).normalized * Random.Range(3.5f, 6.5f);
        }

        Destroy(burst, 1.2f);
    }

    private Material CreateGemstoneMaterial(Color color)
    {
        Shader s = Shader.Find("Standard");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
        if (s == null) s = Shader.Find("Diffuse");

        Material mat = new Material(s);
        mat.name = "Realistic_Gemstone_Mat";
        mat.color = color;
        mat.SetFloat("_Metallic", 0.35f);
        mat.SetFloat("_Glossiness", 0.95f); // High specular shine
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 2.2f);
        return mat;
    }

    /// <summary>
    /// Generates a realistic, flat-faceted hexagonal bipyramid quartz crystal mesh.
    /// Each triangle has distinct vertices so the cut facets catch sunlight brilliantly!
    /// </summary>
    public static Mesh GenerateFacetedCrystalMesh(float height, float radius, float tipHeight)
    {
        Mesh mesh = new Mesh();
        mesh.name = "RealisticFacetedCrystal";

        int sides = 6; // Hexagonal quartz crystal
        Vector3 topApex = new Vector3(0, height * 0.5f + tipHeight, 0);
        Vector3 botApex = new Vector3(0, -height * 0.5f - tipHeight * 0.7f, 0);

        Vector3[] topRing = new Vector3[sides];
        Vector3[] botRing = new Vector3[sides];

        for (int i = 0; i < sides; i++)
        {
            float angle = i * (Mathf.PI * 2f / sides);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            topRing[i] = new Vector3(x, height * 0.5f, z);
            botRing[i] = new Vector3(x, -height * 0.5f, z);
        }

        // 24 triangles * 3 = 72 vertices
        int triCount = sides * 4;
        Vector3[] vertices = new Vector3[triCount * 3];
        int[] triangles = new int[triCount * 3];
        Vector2[] uvs = new Vector2[triCount * 3];

        int vIdx = 0;

        // 1. Top Pointed Facets
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddTri(vertices, triangles, uvs, ref vIdx, topApex, topRing[next], topRing[i]);
        }

        // 2. Middle Hexagonal Prism Facets (2 triangles per quad)
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddTri(vertices, triangles, uvs, ref vIdx, topRing[i], topRing[next], botRing[next]);
            AddTri(vertices, triangles, uvs, ref vIdx, topRing[i], botRing[next], botRing[i]);
        }

        // 3. Bottom Tapered Facets
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            AddTri(vertices, triangles, uvs, ref vIdx, botApex, botRing[i], botRing[next]);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private static void AddTri(Vector3[] verts, int[] tris, Vector2[] uvs, ref int vIdx, Vector3 a, Vector3 b, Vector3 c)
    {
        tris[vIdx] = vIdx;
        verts[vIdx] = a;
        uvs[vIdx] = new Vector2(0.5f, 1f);
        vIdx++;

        tris[vIdx] = vIdx;
        verts[vIdx] = b;
        uvs[vIdx] = new Vector2(0f, 0f);
        vIdx++;

        tris[vIdx] = vIdx;
        verts[vIdx] = c;
        uvs[vIdx] = new Vector2(1f, 0f);
        vIdx++;
    }
}
