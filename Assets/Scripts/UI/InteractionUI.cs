using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI interactionText;

    private void Start()
    {
        Hide();
    }

    public void Show(string text)
    {
        interactionText.text = text;
        interactionText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        interactionText.gameObject.SetActive(false);
    }
}
