using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace PG.DialogueGraphEditor
{
    using PG.DialogueGraph;

    [ScriptedImporter(1, DialogueGraph.ASSET_EXTENSION)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);

            RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();

            var nodeIDMap = new Dictionary<INode, string>();
            foreach (var node in editorGraph.GetNodes())
            {
                nodeIDMap[node] = GUID.Generate().ToString();
            }

            var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNode != null)
            {
                var entryPort = startNode.GetOutputPorts().FirstOrDefault()?.FirstConnectedPort;
                if (entryPort != null)
                {
                    runtimeGraph.entryNodeID = nodeIDMap[entryPort.GetNode()];
                }
            }

            foreach (var iNode in editorGraph.GetNodes())
            {
                if (iNode is StartNode) continue;

                var runtimeNode = new RuntimeDialogueNode { nodeID = nodeIDMap[iNode] };
                switch (iNode)
                {
                    case DialogueNode:
                        DialogueNode dialogueNode = (DialogueNode)iNode;
                        ProcessDialogueNode(dialogueNode, runtimeNode, nodeIDMap);
                        break;
                    case ChoiceNode:
                        ChoiceNode choiceNode = (ChoiceNode)iNode;
                        ProcessChoiceNode(choiceNode, runtimeNode, nodeIDMap);
                        break;
                    case SetBackgroundNode:
                        SetBackgroundNode backgroundNode = (SetBackgroundNode)iNode;
                        ProcessSetBackgroundNode(backgroundNode, runtimeNode, nodeIDMap);
                        break;
                }


                runtimeGraph.allNodes.Add(runtimeNode);
            }
            ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
        }
        private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            node.GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            runtimeNode.speakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
            runtimeNode.dialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
            runtimeNode.audioKey = GetPortValue<string>(node.GetInputPortByName("AudioKey"));
            runtimeNode.audioResource = GetPortValue<AudioResource>(node.GetInputPortByName("Audio"));

            var nextNodePort = node.GetOutputPortByName("out").FirstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.nextNodeID = nodeIDMap[nextNodePort.GetNode()];
            }
        }
        private void ProcessSetBackgroundNode(SetBackgroundNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            node.GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            runtimeNode.background = GetPortValue<Sprite>(node.GetInputPortByName("Background"));

            var nextNodePort = node.GetOutputPortByName("out").FirstConnectedPort;
            if (nextNodePort != null)
            {
                runtimeNode.nextNodeID = nodeIDMap[nextNodePort.GetNode()];
            }
        }
        private void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode, Dictionary<INode, string> nodeIDMap)
        {
            node.GetNodeOptionByName("Node Key").TryGetValue(out runtimeNode.nodeKey);
            runtimeNode.speakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
            runtimeNode.dialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
            runtimeNode.audioKey = GetPortValue<string>(node.GetInputPortByName("AudioKey"));
            runtimeNode.audioResource = GetPortValue<AudioResource>(node.GetInputPortByName("Audio"));

            var choiceOutputPorts = node.GetOutputPorts().Where(p => p.Name.StartsWith("Choice "));

            foreach (var outputPort in choiceOutputPorts)
            {
                var index = outputPort.Name.Substring("Choice ".Length);
                var textPort = node.GetInputPortByName($"Choice Text {index}");

                var choiceData = new ChoiceData
                {
                    choiceText = GetPortValue<string>(textPort),
                    desinationNodeID = outputPort.FirstConnectedPort != null
                        ? nodeIDMap[outputPort.FirstConnectedPort.GetNode()]
                        : null
                };

                runtimeNode.choices.Add(choiceData);
            }

        }

        private T GetPortValue<T>(IPort port)
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


    }
}