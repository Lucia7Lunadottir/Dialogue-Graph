using System;
using Unity.GraphToolkit.Editor;
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
            context.AddInputPort<string>("AudioKey").Build();
            context.AddInputPort<AudioResource>("Audio").Build();
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Node Key").Delayed().Build();
        }
    }
}