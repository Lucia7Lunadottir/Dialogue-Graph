using PG.DialogueGraph;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventInvokeInNode : MonoBehaviour
{
    [SerializeField] private DialogueManager _manager;
    [SerializeField] private bool _findIfNull;
    
    [Space(10)]
    [SerializeField] private DialogueEvent[] _events;
    public Dictionary<string, UnityEvent> events = new Dictionary<string, UnityEvent>();

    [Header("End")]
    [SerializeField] private bool _invokeEventAfterEndDialogue;
    [SerializeField] private UnityEvent _endEvent;
    [System.Serializable]
    public class DialogueEvent
    {
        public string nodeKey;
        public UnityEvent unityEvent;
    }
    private void Awake()
    {
        if (_manager == null && _findIfNull)
        {
            _manager = FindAnyObjectByType<DialogueManager>();
        }
        foreach (var e in _events)
        {
            events.Add(e.nodeKey, e.unityEvent);
        }
        _manager.dialogueStarted += DialogueStarted;
        _manager.dialogueEnded += DialogueEnded;
        if (_invokeEventAfterEndDialogue)
        {
            _manager.dialogueEnded += OnEndInvoke;
        }
    }
    private void OnDestroy()
    {
        _manager.dialogueStarted -= DialogueStarted;
        _manager.dialogueEnded -= DialogueEnded;
        if (_invokeEventAfterEndDialogue)
        {
            _manager.dialogueEnded -= OnEndInvoke;
        }
    }
    void DialogueStarted(RuntimeDialogueGraph runtimeDialogueGraph)
    {
        _manager.dialogueChanged += NodeEventInvoke;
    }
    void DialogueEnded()
    {
        _manager.dialogueChanged -= NodeEventInvoke;
    }
    void NodeEventInvoke(RuntimeDialogueNode node)
    {
        if (node != null && events.TryGetValue(node.nodeKey, out UnityEvent unityEvent))
        {
            unityEvent?.Invoke();
        }
    }
    public void OnInvoke(string nodeKey)
    {
        events[nodeKey]?.Invoke();
    }
    public void OnEndInvoke()
    {
        _endEvent?.Invoke();
    }
}
