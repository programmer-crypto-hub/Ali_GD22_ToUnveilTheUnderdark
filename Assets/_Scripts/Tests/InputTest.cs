using UnityEngine;

public class InputTest : MonoBehaviour
{
    private void Update()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager doesn't exist!");
            return;
        }

        Vector2 move = InputManager.Instance.MoveInput;

        if (move != Vector2.zero)
            Debug.Log($"Move: {move}");

        if (InputManager.Instance.JumpPressed)
            Debug.Log("Jump pressed!");

        if (InputManager.Instance.AttackPressed)
            Debug.Log("Attack pressed!");

        InputManager.Instance.ResetButtonFlags();
    }
}
