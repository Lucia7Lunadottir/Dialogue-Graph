using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Audio;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class DialogueNode : BaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddOutputPort("out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort("in").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

            context.AddInputPort<string>("Speaker").Build();
            context.AddInputPort<string>("Dialogue").AsTextArea(maxLines:10).Build();
            context.AddInputPort<AudioResource>("Audio").Build();
            context.AddInputPort<AudioSource>("Audio Source Ref").Build();
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
        }

        public override RuntimeBaseNode CreateRuntimeNode()
        {
            return new RuntimeDialogueNode();
        }

        public override void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            if (runtimeNode is RuntimeDialogueNode runtimeDialogueNode)
            {
                runtimeDialogueNode.speakerName = context.GetPortValue<string>(GetInputPortByName("Speaker"));
                runtimeDialogueNode.dialogueText = context.GetPortValue<string>(GetInputPortByName("Dialogue"));
                runtimeDialogueNode.audioResource = context.GetPortValue<AudioResource>(GetInputPortByName("Audio"));

                var audioSourceRefKey = context.GetRefKey<AudioSource>(GetInputPortByName("Audio Source Ref"));
                runtimeDialogueNode.audioSource = new ExposedReference<AudioSource> { exposedName = new PropertyName(audioSourceRefKey) };
            }

            var nextNodePort = GetOutputPortByName("out").FirstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.nextNodeID = context.nodeIDMap[nextNodePort.GetNode()];
            }
        }
    }
}
