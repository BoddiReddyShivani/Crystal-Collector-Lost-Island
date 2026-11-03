using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float walkSpeed = 6.5f;
    public float sprintSpeed = 11.0f;
    public float jumpHeight = 1.6f;
    public float gravity = -22f;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Distance Tracking")]
    public float distanceTraveled { get; private set; }
    public float distanceToPortal { get; private set; }

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private Vector3 startPosition;
    private Vector3 lastPosition;
    private float stepTimer = 0f;
    private bool isDead = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Start()
    {
        startPosition = transform.position;
        lastPosition = transform.position;
        distanceTraveled = 0f;
    }

    public void ResetPosition(Vector3 newPos)
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        transform.position = newPos;
        startPosition = newPos;
        lastPosition = newPos;
        verticalVelocity = Vector3.zero;
        distanceTraveled = 0f;
        isDead = false;
        if (controller != null) controller.enabled = true;
    }

    public void SetDead(bool dead)
    {
        isDead = dead;
    }

    void Update()
    {
        // Don't move if dead or game is not actively playing
        if (isDead || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        // Check if player fell into the abyss/water below the island
        if (transform.position.y < -3.5f)
        {
            GameManager.Instance.KillPlayer("Fell into the Ocean Abyss!");
            return;
        }

        // Calculate distance traveled
        Vector3 curPosHoriz = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 lastPosHoriz = new Vector3(lastPosition.x, 0, lastPosition.z);
        float deltaDist = Vector3.Distance(curPosHoriz, lastPosHoriz);
        distanceTraveled += deltaDist;
        lastPosition = transform.position;

        // Update distance to portal if portal exists
        if (GameManager.Instance.portal != null)
        {
            distanceToPortal = Vector3.Distance(transform.position, GameManager.Instance.portal.transform.position);
        }

        // Input
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Sprint boost
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 forward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = (forward * z + right * x).normalized;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Rotate character towards movement
        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 14f * Time.deltaTime);

            // Footstep audio
            if (controller.isGrounded)
            {
                stepTimer += Time.deltaTime * (isSprinting ? 1.5f : 1.0f);
                if (stepTimer >= 0.38f)
                {
                    stepTimer = 0f;
                    SoundManager.Play("footstep");
                }
            }
        }
        else
        {
            stepTimer = 0.3f;
        }

        // Jump & Gravity
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            SoundManager.Play("jump");
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    /// <summary>
    /// CharacterController collision detection - instant death when touching obstacles!
    /// </summary>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        if (hit.gameObject.CompareTag("Obstacle") || hit.gameObject.GetComponent<Obstacle>() != null)
        {
            string hazard = "Island Hazard";
            Obstacle obs = hit.gameObject.GetComponent<Obstacle>();
            if (obs != null && !string.IsNullOrEmpty(obs.hazardName))
                hazard = obs.hazardName;

            GameManager.Instance.KillPlayer(hazard);
        }
    }
}
