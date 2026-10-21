using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace PG.DialogueGraph
{
    public class DialogueManager : MonoBehaviour, IExposedPropertyTable
    {
        [SerializeField] protected RuntimeDialogueGraph _graph;
        public RuntimeDialogueGraph Graph => _graph;

        [SerializeField] private List<ExposedReferenceEntry> _exposedReferences = new();
        private Dictionary<PropertyName, Object> _exposedReferenceLookup;

        [System.Serializable]
        public struct ExposedReferenceEntry
        {
            public string id;
            public Object value;
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            BuildExposedReferenceLookup();
            idValid = _exposedReferenceLookup.TryGetValue(id, out var value);
            return value;
        }

        public void SetReferenceValue(PropertyName id, Object value)
        {
            BuildExposedReferenceLookup();
            _exposedReferenceLookup[id] = value;

            var idString = id.ToString();
            var index = _exposedReferences.FindIndex(e => e.id == idString);
            var entry = new ExposedReferenceEntry { id = idString, value = value };
            if (index >= 0) _exposedReferences[index] = entry;
            else _exposedReferences.Add(entry);
        }

        public void ClearReferenceValue(PropertyName id)
        {
            BuildExposedReferenceLookup();
            _exposedReferenceLookup.Remove(id);
            _exposedReferences.RemoveAll(e => e.id == id.ToString());
        }

        private void BuildExposedReferenceLookup()
        {
            if (_exposedReferenceLookup != null) return;
            _exposedReferenceLookup = new Dictionary<PropertyName, Object>();
            foreach (var entry in _exposedReferences)
            {
                _exposedReferenceLookup[new PropertyName(entry.id)] = entry.value;
            }
        }

        [SerializeField] protected InputActionProperty _nextDialogueProperty;
        protected InputAction _nextDialogueAction;

        [SerializeField] protected bool _startDialogueOnStart;

        [Header("UI")]
        [SerializeField] protected GameObject _dialoguePanel;
        [SerializeField] protected TMP_Text _speakerNameObject;
        [SerializeField] protected TMP_Text _dialogueTextObject;

        [Header("Choice Button UI")]
        [SerializeField] protected Button _choiceButtonPrefab;
        [SerializeField] protected Transform _choiceButtonContainer;

        protected Dictionary<string, RuntimeBaseNode> _nodeLookup = new Dictionary<string, RuntimeBaseNode>();
        public RuntimeBaseNode currentBaseNode { get; protected set; }

        public event System.Action<RuntimeDialogueGraph> dialogueStarted;
        public event System.Action<RuntimeBaseNode> dialogueChanged;
        public event System.Action<RuntimeBaseNode> previousDialogueChanged;
        public event System.Action dialogueEnded;
        public event System.Action<string> nodeBehaviourEvent;
        public void InvokeNodeBehaviourEvent(string key) => nodeBehaviourEvent?.Invoke(key);

        #region Invoke event Actions
        protected void InvokePreviousDialogueChanged(RuntimeBaseNode runtimeDialogueNode) => previousDialogueChanged?.Invoke(runtimeDialogueNode);
        protected void InvokeDialogueChanged(RuntimeBaseNode runtimeDialogueNode) => dialogueChanged?.Invoke(runtimeDialogueNode);
        protected void InvokeDialogueStarted(RuntimeDialogueGraph runtimeDialogueGraph) => dialogueStarted?.Invoke(runtimeDialogueGraph);
        protected void InvokeDialogueEnded() => dialogueEnded?.Invoke(); 
        #endregion



        private void Awake()
        {
            _nextDialogueAction = InputSystem.actions.FindAction(_nextDialogueProperty.reference.name);
        }
        private void Start()
        {
            if (_startDialogueOnStart)
            {
                StartDialogue(_graph);
            }
        }
        protected void ShowNode(string nodeID)
        {
            if (!_nodeLookup.ContainsKey(nodeID))
            {
                return;
            }

            var previousNode = currentBaseNode;
            currentBaseNode = _nodeLookup[nodeID];

            if (previousNode != null)
            {
                foreach (var behaviour in previousNode.behaviours)
                {
                    behaviour.OnExit(this);
                }
            }

            dialogueChanged?.Invoke(currentBaseNode);

            if (currentBaseNode is RuntimeDialogueNode runtimeDialogueNode)
            {
                ShowUI(runtimeDialogueNode.speakerName, runtimeDialogueNode.dialogueText);
                RemoveChoices();

                if (runtimeDialogueNode is RuntimeChoiceNode runtimeChoiceNode && runtimeChoiceNode.choices.Count > 0)
                {
                    RebuildChoices(runtimeChoiceNode);
                }
            }

            foreach (var behaviour in currentBaseNode.behaviours)
            {
                behaviour.OnEnter(this);
            }

            currentBaseNode.Execute(this);
        }

        public virtual void ShowUI(string speaker, string dialogue)
        {
            _speakerNameObject.SetText(speaker);
            _dialogueTextObject.SetText(dialogue);
            _dialoguePanel.SetActive(true);
        }

        public void HideUI()
        {
            _speakerNameObject.SetText("");
            _dialogueTextObject.SetText("");

            _dialoguePanel.SetActive(false);
            RemoveChoices();
        }

        public virtual void RebuildChoices(RuntimeChoiceNode runtimeChoiceNode)
        {
            foreach (var choice in runtimeChoiceNode.choices)
            {
                Button button = Instantiate(_choiceButtonPrefab, _choiceButtonContainer);

                TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = choice.choiceText;
                }

                if (button != null)
                {
                    button.onClick.AddListener(() =>
                    {
                        if (!string.IsNullOrEmpty(choice.desinationNodeID))
                        {
                            ShowNode(choice.desinationNodeID);
                        }
                        else
                        {
                            EndDialogue();
                        }
                    });
                }
            }
        }
        public void RemoveChoices()
        {
            foreach (Transform item in _choiceButtonContainer)
            {
                Destroy(item.gameObject);
            }
        }

        protected void EndDialogue()
        {
            HideUI();

            if (currentBaseNode != null)
            {
                foreach (var behaviour in currentBaseNode.behaviours)
                {
                    behaviour.OnExit(this);
                }
            }

            currentBaseNode = null;
            dialogueEnded?.Invoke();

            _nextDialogueAction.performed -= NextDialogueInput;
        }
        public void StartDialogue() => StartDialogue(_graph);
        public void StartDialogue(RuntimeDialogueGraph runtimeDialogueGraph)
        {
            if (currentBaseNode != null)
            {
                EndDialogue();
            }

            _graph = runtimeDialogueGraph;
            dialogueStarted?.Invoke(_graph);
            foreach (var node in _graph.allNodes)
            {
                _nodeLookup[node.nodeID] = node;
            }

            // До ShowNode: диалог может закончиться сразу (EndNode) - EndDialogue отпишет, а не останется висящая подписка
            _nextDialogueAction.performed -= NextDialogueInput;
            _nextDialogueAction.performed += NextDialogueInput;

            if (!string.IsNullOrEmpty(_graph.entryNodeID))
            {
                ShowNode(_graph.entryNodeID);
            }
            else
            {
                EndDialogue();
            }
        }
        // Update is called once per frame
        void NextDialogueInput(InputAction.CallbackContext context)
        {
            if (currentBaseNode != null && currentBaseNode is RuntimeDialogueNode && currentBaseNode is not RuntimeChoiceNode)
            {
                NextDialogue();
            }
        }

        public void NextDialogue()
        {
            if (!string.IsNullOrEmpty(currentBaseNode.nextNodeID))
            {
                InvokePreviousDialogueChanged(currentBaseNode);
                ShowNode(currentBaseNode.nextNodeID);
            }
            else
            {
                EndDialogue();
            }
        }
    }
}
