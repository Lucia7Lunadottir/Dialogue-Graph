using System;
using System.Linq;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Audio;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class ChoiceNode : BaseNode
    {
        private const string _OPTION_ID = "portCount";
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddInputPort("in").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

            context.AddInputPort<string>("Speaker").Build();
            context.AddInputPort<string>("Dialogue").AsTextArea(maxLines:10).Build();
            context.AddInputPort<AudioResource>("Audio").Build();
            context.AddInputPort<AudioSource>("Audio Source Ref").Build();

            var option = GetNodeOptionByName(_OPTION_ID);
            option.TryGetValue(out int portCount);

            for (int i = 0; i < portCount; i++)
            {
                context.AddInputPort<string>($"Choice Text {i}").Build();
                context.AddOutputPort($"Choice {i}").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            }
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
            context.AddOption<int>(_OPTION_ID).WithDefaultValue(2).Delayed().Build();
        }

        public override RuntimeBaseNode CreateRuntimeNode()
        {
            return new RuntimeChoiceNode();
        }

        public override void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            if (runtimeNode is RuntimeChoiceNode runtimeChoiceNode)
            {
                runtimeChoiceNode.speakerName = context.GetPortValue<string>(GetInputPortByName("Speaker"));
                runtimeChoiceNode.dialogueText = context.GetPortValue<string>(GetInputPortByName("Dialogue"));
                runtimeChoiceNode.audioResource = context.GetPortValue<AudioResource>(GetInputPortByName("Audio"));

                var audioSourceRefKey = context.GetRefKey<AudioSource>(GetInputPortByName("Audio Source Ref"));
                runtimeChoiceNode.audioSource = new ExposedReference<AudioSource> { exposedName = new PropertyName(audioSourceRefKey) };

                var choiceOutputPorts = GetOutputPorts().Where(p => p.Name.StartsWith("Choice "));

                foreach (var outputPort in choiceOutputPorts)
                {
                    var index = outputPort.Name.Substring("Choice ".Length);
                    var textPort = GetInputPortByName($"Choice Text {index}");

                    var choiceData = new ChoiceData
                    {
                        choiceText = context.GetPortValue<string>(textPort),
                        desinationNodeID = outputPort.FirstConnectedPort != null
                            ? context.nodeIDMap[outputPort.FirstConnectedPort.GetNode()]
                            : null
                    };

                    runtimeChoiceNode.choices.Add(choiceData);
                }
            }
        }
    }
}