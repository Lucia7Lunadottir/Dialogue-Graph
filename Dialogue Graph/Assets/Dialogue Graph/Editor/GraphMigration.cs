using UnityEngine;

namespace PG.DialogueGraphEditor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    static class GraphMigration
    {
        static readonly Dictionary<string, string> k_StringsToMigrate = new()
        {
            { "asm: UnityEditor.GraphToolkitModule}", "asm: UnityEditor.GraphToolkitModule}" },
            { "asm: UnityEditor.GraphToolkitModule}", "asm: UnityEditor.GraphToolkitModule}" },
            { "UnityEditor.GraphToolkitModule::", "UnityEditor.GraphToolkitModule::" },
            { "{fileID: 12501, guid: 0000000000000000e000000000000000, type: 0}", "{fileID: 12501, guid: 0000000000000000e000000000000000, type: 0}" }
        };

        static void MigrateFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var fullFile = File.ReadAllText(filePath);
            bool migrate = false;

            foreach (var oldAssemblyName in k_StringsToMigrate.Keys)
            {
                if (fullFile.Contains(oldAssemblyName))
                {
                    migrate = true;
                    break;
                }
            }

            if (migrate)
            {
                foreach (var (oldAssemblyName, newAssemblyName) in k_StringsToMigrate)
                {
                    fullFile = fullFile.Replace(oldAssemblyName, newAssemblyName);
                }
                File.WriteAllText(filePath, fullFile);
                Debug.Log($"Graph File has been migrated: {filePath}");
            }
        }

        [MenuItem("Window/GraphToolkit/Migrate Graph Asset Files From Package to Module")]
        internal static void MigrateGraphObjectFiles()
        {
            // Раскомментируй строку ниже, чтобы скрипт автоматически прошелся по всем ассетам в проекте
            var paths = AssetDatabase.GetAllAssetPaths(); 
        
            foreach (var graphPath in paths)
                MigrateFile(graphPath);
        }
    }
}
