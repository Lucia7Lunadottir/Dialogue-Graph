using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Playables;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class TimelineNode : BaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddOutputPort("out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort("in").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

            context.AddInputPort<PlayableAsset>("Timeline").Build();
            context.AddInputPort<PlayableDirector>("Director Ref").Build();
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
            // Держать последний кадр после конца (например, под выбором ответа); выключено - таймлайн отпускает сцену
            context.AddOption<bool>("Hold Last Frame").Build();
        }

        public override RuntimeBaseNode CreateRuntimeNode()
        {
            return new RuntimeTimelineNode();
        }

        public override void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            if (runtimeNode is RuntimeTimelineNode runtimeTimelineNode)
            {
                runtimeTimelineNode.timeline = context.GetPortValue<PlayableAsset>(GetInputPortByName("Timeline"));
                GetNodeOptionByName("Hold Last Frame").TryGetValue(out runtimeTimelineNode.holdLastFrame);

                var directorRefKey = context.GetRefKey<PlayableDirector>(GetInputPortByName("Director Ref"));
                runtimeTimelineNode.director = new ExposedReference<PlayableDirector> { exposedName = new PropertyName(directorRefKey) };
            }

            var nextNodePort = GetOutputPortByName("out").FirstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.nextNodeID = context.nodeIDMap[nextNodePort.GetNode()];
            }
        }
    }
}