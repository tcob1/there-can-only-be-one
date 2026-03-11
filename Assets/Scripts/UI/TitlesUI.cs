using System.Collections;
using TMPro;
using UnityEngine;

public class TitlesUI : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI titleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Clear()
    {
        titleText.text = "";
    }

    public void typeText(string text)
    {
        Clear();
        StartCoroutine(TypeTextCoroutine(text));
    }

    private IEnumerator TypeTextCoroutine(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            titleText.text += text[i];
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1f);
        Clear();
        Hide();
    }
}
