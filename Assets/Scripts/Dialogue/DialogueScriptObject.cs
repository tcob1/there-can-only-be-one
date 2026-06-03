using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Lines")]
public class DialogueScriptObject : ScriptableObject
{
    [TextArea(2, 5)]
    public string[] lines;
}