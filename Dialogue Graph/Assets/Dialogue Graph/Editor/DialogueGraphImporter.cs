using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace PG.DialogueGraphEditor
{
    using PG.DialogueGraph;

    [ScriptedImporter(2, DialogueGraph.ASSET_EXTENSION)]
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

            var exportContext = new ExportContext { nodeIDMap = nodeIDMap };

            var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNode != null)
            {
                var entryPort = startNode.GetOutputPortByName("out")?.FirstConnectedPort;
                if (entryPort != null)
                {
                    runtimeGraph.entryNodeID = nodeIDMap[entryPort.GetNode()];
                }
            }

            foreach (var iNode in editorGraph.GetNodes())
            {
                if (iNode is StartNode) continue;

                if (iNode is IRuntimeExportable exportable)
                {
                    var runtimeNode = exportable.CreateRuntimeNode();

                    runtimeNode.nodeID = nodeIDMap[iNode];
                    exportable.ExportToRuntime(runtimeNode, exportContext);

                    var behavioursPort = iNode.GetOutputPortByName("Behaviours");
                    if (behavioursPort != null)
                    {
                        var connectedPorts = new List<IPort>();
                        behavioursPort.GetConnectedPorts(connectedPorts);

                        foreach (var connectedPort in connectedPorts)
                        {
                            if (connectedPort.GetNode() is BehaviourNode behaviourNode)
                            {
                                var runtimeBehaviour = behaviourNode.CreateRuntimeBehaviour();
                                behaviourNode.ExportToRuntime(runtimeBehaviour, exportContext);
                                runtimeNode.behaviours.Add(runtimeBehaviour);
                            }
                        }
                    }

                    runtimeGraph.allNodes.Add(runtimeNode);
                }
            }

            foreach (var refKey in exportContext.usedRefKeys)
            {
                runtimeGraph.exposedReferenceSlots.Add(new ExposedReferenceSlotInfo
                {
                    key = refKey.Key,
                    typeAssemblyQualifiedName = refKey.Value.AssemblyQualifiedName
                });
            }

            ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
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