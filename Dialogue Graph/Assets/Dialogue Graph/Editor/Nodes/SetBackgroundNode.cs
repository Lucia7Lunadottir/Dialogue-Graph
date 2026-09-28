using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;
namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class SetBackgroundNode : BaseNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddOutputPort("out").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddInputPort("in").WithConnectorUI(PortConnectorUI.Arrowhead).Build();

            context.AddInputPort<Sprite>("Background").Build();
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
            base.OnDefineOptions(context);
        }

        public override RuntimeBaseNode CreateRuntimeNode()
        {
            return new RuntimeBackgroundNode();
        }

        public override void ExportToRuntime(RuntimeBaseNode runtimeNode, ExportContext context)
        {
            GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            if (runtimeNode is RuntimeBackgroundNode runtimeBackgroundNode)
            {
                runtimeBackgroundNode.background = context.GetPortValue<Sprite>(GetInputPortByName("Background"));
            }

            var nextNodePort = GetOutputPortByName("out").FirstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.nextNodeID = context.nodeIDMap[nextNodePort.GetNode()];
            }
        }
    }
}