using System;
using Unity.GraphToolkit.Editor;
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
            context.AddInputPort<string>("AudioKey").Build();
            context.AddInputPort<AudioResource>("Audio").Build();

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
    }
}