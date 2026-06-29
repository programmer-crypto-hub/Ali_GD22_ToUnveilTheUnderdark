using UnityEngine;
using Fusion;

public class CameraTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -10f); 
    [Range(0.5f, 20f)] public float smoothSpeed = 5f;

    public void Render()
    {
        if (target == null) return;
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.unscaledDeltaTime);
    }
}