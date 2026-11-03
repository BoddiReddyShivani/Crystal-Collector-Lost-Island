using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    public Renderer portalRenderer;
    public Color lockedColor = new Color(0.85f, 0.15f, 0.15f);
    public Color unlockedColor = new Color(0.1f, 1f, 0.75f);
    public bool isUnlocked { get; private set; }

    private Transform ringChild;

    void Awake()
    {
        if (transform.childCount > 0)
        {
            ringChild = transform.GetChild(0);
        }
    }

    void Start()
    {
        SetColor(lockedColor);
    }

    void Update()
    {
        if (ringChild != null)
        {
            float speed = isUnlocked ? 75f : 30f;
            ringChild.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
        }
    }

    public void Unlock()
    {
        isUnlocked = true;
        SetColor(unlockedColor);
    }

    public void Lock()
    {
        isUnlocked = false;
        SetColor(lockedColor);
    }

    void SetColor(Color c)
    {
        if (portalRenderer && portalRenderer.material)
        {
            portalRenderer.material.color = c;
            portalRenderer.material.SetColor("_EmissionColor", c * 2.2f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isUnlocked)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowHint("Portal is locked! Collect all crystals to open it.");
            }
        }
    }
}
