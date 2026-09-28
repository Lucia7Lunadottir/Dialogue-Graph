using System;
using System.Linq;
using PG.DialogueGraph;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PG.DialogueGraphEditor
{
    [CustomEditor(typeof(DialogueManager), true)]
    public class DialogueManagerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var manager = (DialogueManager)target;
            var exposedReferencesContainer = new VisualElement();
            root.Add(exposedReferencesContainer);

            string lastSignature = null;
            exposedReferencesContainer.schedule.Execute(() =>
            {
                var graph = manager.Graph;
                var signature = graph == null || graph.exposedReferenceSlots == null
                    ? string.Empty
                    : string.Join("|", graph.exposedReferenceSlots.Select(s => s.key + ":" + s.typeAssemblyQualifiedName));

                if (signature == lastSignature) return;
                lastSignature = signature;

                RebuildExposedReferences(exposedReferencesContainer, manager, graph);
            }).Every(500);

            return root;
        }

        private static void RebuildExposedReferences(VisualElement container, DialogueManager manager, RuntimeDialogueGraph graph)
        {
            container.Clear();
            if (graph == null || graph.exposedReferenceSlots == null || graph.exposedReferenceSlots.Count == 0)
                return;

            var header = new Label("Exposed References");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginTop = 8;
            container.Add(header);

            foreach (var slot in graph.exposedReferenceSlots)
            {
                var referenceType = Type.GetType(slot.typeAssemblyQualifiedName);
                if (referenceType == null) continue;

                var propertyName = new PropertyName(slot.key);

                var field = new ObjectField(slot.key)
                {
                    objectType = referenceType,
                    value = manager.GetReferenceValue(propertyName, out _)
                };
                field.RegisterValueChangedCallback(evt =>
                {
                    Undo.RecordObject(manager, "Set Exposed Reference");
                    manager.SetReferenceValue(propertyName, evt.newValue);
                    EditorUtility.SetDirty(manager);
                });
                container.Add(field);
            }
        }
    }
}
