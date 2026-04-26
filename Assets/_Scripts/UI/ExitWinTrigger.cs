using UnityEngine;

public class ExitWinTrigger : MonoBehaviour
{
    [SerializeField] private GameLoopFlowController flowController;
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool showDebugLogs = true;

    private void Reset()
    {
        if (flowController == null)
            flowController = FindFirstObjectByType<GameLoopFlowController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy || !IsPlayerCollider(other))
            return;

        if (flowController == null)
        {
            if (showDebugLogs)
                Debug.LogWarning($"{name}: flowController is not assigned.", this);

            return;
        }

        flowController.RequestWinFromExit();

        if (showDebugLogs)
            Debug.Log($"{name}: player entered exit trigger, win requested.", this);
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        if (string.IsNullOrWhiteSpace(requiredTag) || other.CompareTag(requiredTag))
            return true;

        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(requiredTag))
            return true;

        Transform root = other.transform.root;
        return root != null && root.CompareTag(requiredTag);
    }
}