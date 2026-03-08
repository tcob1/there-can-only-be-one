using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private TitlesUI titlesUI;

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

    public void ShowTitle(string text)
    {
        titlesUI.Show();
        titlesUI.typeText(text);
    }
}