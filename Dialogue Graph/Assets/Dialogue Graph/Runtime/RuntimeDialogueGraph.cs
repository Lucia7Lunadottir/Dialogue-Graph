using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace PG.DialogueGraph
{
    public class RuntimeDialogueGraph : ScriptableObject
    {
        public string entryNodeID;
        [SerializeReference] public List<RuntimeBaseNode> allNodes = new List<RuntimeBaseNode>();
        public List<ExposedReferenceSlotInfo> exposedReferenceSlots = new List<ExposedReferenceSlotInfo>();
    }

    [Serializable]
    public struct ExposedReferenceSlotInfo
    {
        public string key;
        public string typeAssemblyQualifiedName;
    }

    [Serializable]
    public abstract class RuntimeBaseNode
    {
        public abstract void Execute(DialogueManager manager);
        public string nodeID;
        public string nodeKey;
        public string nextNodeID;
        public string jsonData;
        [SerializeReference] public List<RuntimeNodeBehaviour> behaviours = new List<RuntimeNodeBehaviour>();
    }

    [Serializable]
    public abstract class RuntimeNodeBehaviour
    {
        public virtual void OnEnter(DialogueManager manager)
        {
        }
        public virtual void OnExit(DialogueManager manager)
        {
        }
    }

    [Serializable]
    public class RuntimeUnityEventBehaviour : RuntimeNodeBehaviour
    {
        public string eventKey;
        public override void OnEnter(DialogueManager manager)
        {
            manager.InvokeNodeBehaviourEvent(eventKey);
        }
    }

    [Serializable]
    public class RuntimeAnimatorBehaviour : RuntimeNodeBehaviour
    {
        public ExposedReference<Animator> animator;
        public AnimatorType parameterType;
        public string parameterName;
        public float floatValue;
        public int intValue;
        public bool boolValue;

        public override void OnEnter(DialogueManager manager)
        {
            var resolvedAnimator = animator.Resolve(manager);
            if (resolvedAnimator == null) return;

            switch (parameterType)
            {
                case AnimatorType.Trigger:
                    resolvedAnimator.SetTrigger(parameterName);
                    break;
                case AnimatorType.Int:
                    resolvedAnimator.SetInteger(parameterName, intValue);
                    break;
                case AnimatorType.Float:
                    resolvedAnimator.SetFloat(parameterName, floatValue);
                    break;
                case AnimatorType.Bool:
                    resolvedAnimator.SetBool(parameterName, boolValue);
                    break;
            }
        }
    }

    [Serializable]
    public class RuntimeDialogueNode : RuntimeBaseNode
    {
        public string speakerName;
        public string dialogueText;
        public AudioResource audioResource;
        public ExposedReference<AudioSource> audioSource;
        public override void Execute(DialogueManager manager)
        {
        }
    }
    [Serializable]
    public class RuntimeTimelineNode : RuntimeBaseNode
    {
        public PlayableAsset timeline;
        public ExposedReference<PlayableDirector> director;
        public override void Execute(DialogueManager manager)
        {
        }
    }
    [Serializable]
    public class RuntimeBackgroundNode : RuntimeBaseNode
    {
        public Sprite background;
        public override void Execute(DialogueManager manager)
        {
            manager.NextDialogue();
        }
    }
    [Serializable]
    public class RuntimeChoiceNode : RuntimeDialogueNode
    {
        public List<ChoiceData> choices = new List<ChoiceData>();
    }

    [Serializable]
    public struct ChoiceData
    {
        public string choiceText;
        public string desinationNodeID;
    }
}