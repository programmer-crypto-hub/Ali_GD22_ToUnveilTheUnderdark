using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;

    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private int characterWrapLimit = 200;
    [SerializeField] private RectTransform rectTransform;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // Hide on start
    }

    private void Update()
    {
        // Follow the mouse position
        Vector2 position = Input.mousePosition;

        // Pivot adjustment so it doesn't appear under the cursor
        float pivotX = position.x / Screen.width;
        float pivotY = position.y / Screen.height;
        rectTransform.pivot = new Vector2(pivotX, pivotY);

        transform.position = position;
    }

    public void Show(string content, string header = "")
    {
        headerText.text = header;
        headerText.gameObject.SetActive(!string.IsNullOrEmpty(header));
        contentText.text = content;

        // Wrap text if it's too long
        int headerLength = headerText.text.Length;
        int contentLength = contentText.text.Length;
        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}