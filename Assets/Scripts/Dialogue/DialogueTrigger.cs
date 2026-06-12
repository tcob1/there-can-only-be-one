using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Drag an optional DialogueScriptObject asset here")]
    public DialogueScriptObject dialogue;

    // Call this from a player interaction system (trigger, input, etc.)
    public void Interact()
    {
        if (dialogue == null || dialogue.lines.Length == 0)
        {
            Debug.Log($"{name} has no dialogue assigned.");
            return;
        }

        DialogueUI.Instance.ShowDialogue(dialogue.lines);
    }

    // Optional to auto-trigger on enter if prefer proximity-based
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Interact();
    }
}