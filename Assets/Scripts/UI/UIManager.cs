using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private TitlesUI titlesUI;
    [SerializeField] private InteractionUI interactionUI;
    [SerializeField] private FadeText heldItemNameFadeEffect;
    [SerializeField] private GameObject clock;
    [SerializeField] private GameObject inventoryBar;
    [SerializeField] private DeathScreenUI deathScreenUI;
    [SerializeField] private NotepageUI notepageUI;


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
        heldItemNameFadeEffect.gameObject.SetActive(false);
        GameManager.Instance.OnGameStart += () =>
        {
            ShowClock();
            ShowInventoryBar();
            ShowTitle("THERE CAN ONLY BE ONE");
        };

        GameManager.Instance.OnGameOver += () =>
        {
            HideClock();
            HideInventoryBar();
        };

        GameManager.Instance.OnRespawn += () =>
        {
            ShowClock();
            ShowInventoryBar();
        };
    }

    public void ToggleNotepage()
    {
        notepageUI.Toggle();
    }

    public void ShowDeathScreen()
    {
        deathScreenUI.gameObject.SetActive(true);
    }

    public void HideDeathScreen()
    {
        deathScreenUI.gameObject.SetActive(false);
    }

    public void ShowEquipText(string itemName)
    {
        heldItemNameFadeEffect.gameObject.SetActive(true);
        heldItemNameFadeEffect.Fade(itemName);
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

    private void ShowClock()
    {
        clock.SetActive(true);
    }

    private void HideClock()
    {
        clock.SetActive(false);
    }

    private void ShowInventoryBar()
    {
        inventoryBar.SetActive(true);
    }

    private void HideInventoryBar()
    {
        inventoryBar.SetActive(false);
    }
}