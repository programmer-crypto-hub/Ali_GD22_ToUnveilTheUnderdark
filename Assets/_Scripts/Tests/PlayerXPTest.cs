using UnityEngine;

public class PlayerExperienceTest : MonoBehaviour
{
    public PlayerProgression progression;

    private void Awake()
    {
        if (progression == null)
            progression = FindFirstObjectByType<PlayerProgression>();
    }

    private void Update()
    {
        if (progression == null)
            return;

        // Добавить 50 опыта по нажатию клавиши X
        if (Input.GetKeyDown(KeyCode.X))
        {
            progression.AddXP(50f);
        }
    }
}