using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTarget : NetworkBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 0.3f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("State")]
    [SerializeField] private float currentYaw = 0f;    // Y
    [SerializeField] private float currentPitch = 0f; // X

    public Transform target; // Drag your Player Root here (at runtime)
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothSpeed = 10f;

    public CinemachineCamera vcam;

    public void AssignPlayer(GameObject player)
    {
        vcam.Target.TrackingTarget = player.transform;
    }

    public override void Spawned()
    {
        Vector3 euler = transform.localRotation.eulerAngles;
        currentYaw = euler.y;
        currentPitch = NormalizeAngle(euler.x);
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);
        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        if (HasInputAuthority)
        {
            AssignPlayer(this.gameObject);
            var camScript = Camera.main.GetComponent<CameraTarget>();
            if (camScript != null)
            {
                camScript.target = this.transform;
                Debug.Log("Camera successfully linked to Player!");
            }
            else Debug.LogError("CameraTarget script not found on Main Camera!");
        }
    }

    public void SetTarget(Transform newTarget) => target = newTarget; 

    private void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogError("Target not set for CameraTarget! Please assign a target transform.");
            return; 
        }

        // Smoothly follow the player's position
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
    }

    public float GetMouseSensitivity() => mouseSensitivity;

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle; 
    }
}