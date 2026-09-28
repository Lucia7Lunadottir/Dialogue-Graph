using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace PG.DialogueGraphEditor
{
    public class ExportContext
    {
        public Dictionary<INode, string> nodeIDMap { get; set; }
        public Dictionary<string, Type> usedRefKeys { get; } = new Dictionary<string, Type>();

        public T GetPortValue<T>(IPort port)
        {
            if (port == null) return default;

            if (port.IsConnected)
            {
                if (port.FirstConnectedPort.GetNode() is IVariableNode variableNode)
                {
                    variableNode.Variable.TryGetDefaultValue(out T value);
                    return value;
                }
            }

            port.TryGetValue(out T fallbackValue);
            return fallbackValue;
        }

        public string GetRefKey<T>(IPort port)
        {
            if (port != null && port.IsConnected && port.FirstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                var key = variableNode.Variable.Name;
                if (!string.IsNullOrEmpty(key))
                {
                    usedRefKeys[key] = typeof(T);
                }
                return key;
            }
            return null;
        }
    }
}