using System;
using PG.DialogueGraph;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace PG.DialogueGraphEditor
{
    [Serializable]
    public class AnimatorBehaviourNode : BehaviourNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            context.AddInputPort<Animator>("Animator Ref").Build();
            context.AddInputPort<string>("Parameter Name").Build();

            GetNodeOptionByName("Parameter Type").TryGetValue(out AnimatorType parameterType);
            switch (parameterType)
            {
                case AnimatorType.Float:
                    context.AddInputPort<float>("Float Value").Build();
                    break;
                case AnimatorType.Int:
                    context.AddInputPort<int>("Int Value").Build();
                    break;
                case AnimatorType.Bool:
                    context.AddInputPort<bool>("Bool Value").Build();
                    break;
                case AnimatorType.Trigger:
                    break;
            }
        }
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<AnimatorType>("Parameter Type").WithDefaultValue(AnimatorType.Trigger).Build();
        }

        public override RuntimeNodeBehaviour CreateRuntimeBehaviour()
        {
            return new RuntimeAnimatorBehaviour();
        }

        public override void ExportToRuntime(RuntimeNodeBehaviour runtimeBehaviour, ExportContext context)
        {
            if (runtimeBehaviour is RuntimeAnimatorBehaviour runtimeAnimatorBehaviour)
            {
                var animatorRefKey = context.GetRefKey<Animator>(GetInputPortByName("Animator Ref"));
                runtimeAnimatorBehaviour.animator = new ExposedReference<Animator> { exposedName = new PropertyName(animatorRefKey) };

                GetNodeOptionByName("Parameter Type").TryGetValue(out runtimeAnimatorBehaviour.parameterType);
                runtimeAnimatorBehaviour.parameterName = context.GetPortValue<string>(GetInputPortByName("Parameter Name"));

                switch (runtimeAnimatorBehaviour.parameterType)
                {
                    case AnimatorType.Float:
                        runtimeAnimatorBehaviour.floatValue = context.GetPortValue<float>(GetInputPortByName("Float Value"));
                        break;
                    case AnimatorType.Int:
                        runtimeAnimatorBehaviour.intValue = context.GetPortValue<int>(GetInputPortByName("Int Value"));
                        break;
                    case AnimatorType.Bool:
                        runtimeAnimatorBehaviour.boolValue = context.GetPortValue<bool>(GetInputPortByName("Bool Value"));
                        break;
                    case AnimatorType.Trigger:
                        break;
                }
            }
        }
    }
}
