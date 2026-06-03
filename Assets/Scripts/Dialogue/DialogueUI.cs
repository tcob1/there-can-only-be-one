using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;   // The bottom box panel
    public TMP_Text dialogueText;      // Text inside the box
    public Button continueButton;      // Continue button

    private string[] _lines;
    private int _index;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        dialoguePanel.SetActive(false);
        continueButton.onClick.AddListener(NextLine);
    }

    public void ShowDialogue(string[] lines)
    {
        _lines = lines;
        _index = 0;
        dialoguePanel.SetActive(true);
        DisplayLine();
    }

    private void DisplayLine()
    {
        dialogueText.text = _lines[_index];
    }

    private void NextLine()
    {
        _index++;
        if (_index < _lines.Length)
            DisplayLine();
        else
            CloseDialogue();
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        _lines = null;
    }

    // Optional to also close/advance on a key press
    private void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetKeyDown(KeyCode.E))
            NextLine();
    }
}