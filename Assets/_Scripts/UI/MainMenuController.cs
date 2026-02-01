using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button buttonNewGame;
    [SerializeField] private Button buttonQuitGame;

    private void Awake()
    {
        buttonNewGame.onClick.AddListener(() => GameManager.Instance.StartGame());
        buttonQuitGame.onClick.AddListener(() => Application.Quit());
    }
}
