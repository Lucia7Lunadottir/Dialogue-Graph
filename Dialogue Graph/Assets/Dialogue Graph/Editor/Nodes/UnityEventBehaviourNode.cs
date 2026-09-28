using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class UnityEventBehaviourNode : BehaviourNode
    {
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<string>("Event Key").Delayed().Build();
        }

        public override RuntimeNodeBehaviour CreateRuntimeBehaviour()
        {
            return new RuntimeUnityEventBehaviour();
        }

        public override void ExportToRuntime(RuntimeNodeBehaviour runtimeBehaviour, ExportContext context)
        {
            if (runtimeBehaviour is RuntimeUnityEventBehaviour runtimeUnityEventBehaviour)
            {
                GetNodeOptionByName("Event Key").TryGetValue(out runtimeUnityEventBehaviour.eventKey);
            }
        }
    }
}
