using UnityEngine;

/// <summary>
/// Attached to rocks, spikes, hazards, and moving obstacles.
/// Any contact with the player causes immediate death and restart!
/// </summary>
public class Obstacle : MonoBehaviour
{
    [Header("Hazard Information")]
    public string hazardName = "Dangerous Island Obstacle";

    [Header("Movement (Optional Patrol)")]
    public bool isMoving = false;
    public Vector3 moveDirection = Vector3.right;
    public float moveDistance = 4f;
    public float moveSpeed = 2f;

    [Header("Rotation (Optional Spin)")]
    public bool isRotating = false;
    public Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
        // Make sure tag is set to Obstacle
        if (gameObject.tag != "Obstacle")
        {
            try { gameObject.tag = "Obstacle"; } catch { }
        }
    }

    void Update()
    {
        if (isMoving)
        {
            float pingPong = Mathf.PingPong(Time.time * moveSpeed, moveDistance) - (moveDistance * 0.5f);
            transform.position = initialPosition + moveDirection.normalized * pingPong;
        }

        if (isRotating)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckAndKill(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CheckAndKill(other.gameObject);
    }

    private void CheckAndKill(GameObject hitObject)
    {
        if (hitObject.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.KillPlayer(hazardName);
            }
        }
    }
}
