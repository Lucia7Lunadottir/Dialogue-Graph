using System;
using PG.DialogueGraph;
using UnityEngine;

public class CursorForChoiceNodeActivator : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    private void OnEnable()
    {
        _manager.dialogueChanged += UpdateCursor;
    }
    private void OnDisable()
    {
        _manager.dialogueChanged -= UpdateCursor;
    }
    // Update is called once per frame
    void UpdateCursor(RuntimeDialogueNode runtimeDialogueNode)
    {
        if (runtimeDialogueNode.choices.Count > 0)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}