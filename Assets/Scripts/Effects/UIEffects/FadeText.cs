using UnityEngine;
using TMPro;
using System.Collections;

// Used claude for this effect
public class FadeText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine currentCoroutine;

    private void Start()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
    }

    public void Fade(string message)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(FadeRoutine(message));
    }

    private IEnumerator FadeRoutine(string message)
    {
        // instantly show
        text.text = message;
        text.alpha = 1f;

        // wait
        yield return new WaitForSeconds(displayDuration);

        // fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            text.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        text.alpha = 0f;
        currentCoroutine = null;
    }
}