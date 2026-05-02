using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private TitlesUI titlesUI;
    [SerializeField] private InteractionUI interactionUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.OnGameStart += () =>
        {
            ShowTitle("THERE CAN ONLY BE ONE");
        };

        GameManager.Instance.OnGameOver += () =>
        {
            ShowTitle("YOU ARE NOT THE ONE");
        };
    }

    public void ShowInteractionText(string text)
    {
        interactionUI.Show(text);
    }

    public void HideInteractionText()
    {
        interactionUI.Hide();
    }

    public void ShowTitle(string text)
    {
        titlesUI.Show();
        titlesUI.typeText(text);
    }
}