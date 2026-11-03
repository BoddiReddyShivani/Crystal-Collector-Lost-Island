using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6.5f, -9f);
    public float smoothSpeed = 8f;

    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.35f;

    public void Shake(float duration = 0.35f, float magnitude = 0.35f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        if (shakeDuration > 0)
        {
            smoothed += Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime;
        }

        transform.position = smoothed;
        transform.LookAt(target.position + Vector3.up * 1.2f);
    }
}
