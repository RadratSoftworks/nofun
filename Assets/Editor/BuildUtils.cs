using System;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using UnityEngine;

namespace Nofun
{
    [CreateAssetMenu(menuName = "Utils/BuildUtils")]
    public class BuildUtils : ScriptableObject
    {
        [SerializeField, ReadOnly] private List<string> assemblyDefinitionSearchPaths;

        [Button]
        public void AddAssemblyDefinitionSearchPath()
        {
            var path = UnityEditor.EditorUtility.OpenFolderPanel("Select Assembly Definition Search Path", "", "");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var assetPathIndex = path.IndexOf("Assets", StringComparison.Ordinal);

            if (assetPathIndex == -1)
            {
                Debug.LogError("Selected path is not in the project folder");
                return;
            }
            else
            {
                path = path.Substring(assetPathIndex);
            }

            assemblyDefinitionSearchPaths.Add(path);
        }

        [Button]
        public void RemoveLastAssemblyDefinitionPath()
        {
            if (assemblyDefinitionSearchPaths.Count > 0)
            {
                assemblyDefinitionSearchPaths.RemoveAt(assemblyDefinitionSearchPaths.Count - 1);
            }
        }

        [Button]
        public void TurnOffAssemblyDefinitions()
        {
            foreach (var assemblyDefinition in UnityEditor.AssetDatabase.FindAssets("t:AssemblyDefinitionAsset", assemblyDefinitionSearchPaths.ToArray()))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assemblyDefinition);
                File.Move(path, Path.ChangeExtension(path, ".asmdef.disabled"));
            }

            foreach (var assemblyDefinition in UnityEditor.AssetDatabase.FindAssets("t:AssemblyDefinitionReferenceAsset", assemblyDefinitionSearchPaths.ToArray()))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assemblyDefinition);
                File.Move(path, Path.ChangeExtension(path, ".asmref.disabled"));
            }
        }
    }
}
