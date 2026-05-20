using UnityEngine;
using TMPro;
using System.Collections;

public class NotepageUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform notePage;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float offScreenX = -400f;
    [SerializeField] private float onScreenX = 50f;

    public bool IsOpen { get; private set; }

    private Coroutine currentSlide;

    void Start()
    {
        panel.SetActive(false);
        notePage.anchoredPosition = new Vector2(offScreenX, notePage.anchoredPosition.y);
    }

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        IsOpen = true;
        panel.SetActive(true);
        RefreshLog();

        if (currentSlide != null) 
        {
            StopCoroutine(currentSlide);
        }
        currentSlide = StartCoroutine(Slide(onScreenX));

        GameManager.Instance.Pause();

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    public void Close()
    {
        IsOpen = false;
        if (currentSlide != null)
        {
            StopCoroutine(currentSlide);
        }
        currentSlide = StartCoroutine(SlideOutAndHide());

        GameManager.Instance.Resume();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    private IEnumerator Slide(float targetX)
    {
        Vector2 start = notePage.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            // cubic function to ease in and out
            t = 1f - Mathf.Pow(1f - t, 3f);
            notePage.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        notePage.anchoredPosition = end;
    }

    private IEnumerator SlideOutAndHide()
    {
        yield return Slide(offScreenX);
        panel.SetActive(false);
    }

    // refreshes the log text with the current events from the EventLogger
    private void RefreshLog()
    {
        if (EventLogger.Instance == null) return;

        string text = "";
        foreach (LoggedEvent e in EventLogger.Instance.loggedEvents)
        {
            text += $"[{FormatTime(e.gameTime)}]  {e.description}\n";
        }

        logText.text = string.IsNullOrEmpty(text) ? "No events logged yet" : text;
    }

    private string FormatTime(long time)
    {
        long hour = (time / 3600) % 24;
        long min = (time / 60) % 60;
        return $"{hour:D2}:{min:D2}";
    }
}