using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using Nofun.Parser;
using Nofun.PIP2;
using Nofun.VM;
using UnityEditor;
using UnityEngine;

namespace Nofun
{
    [CreateAssetMenu(menuName = "Utils/BuildUtils")]
    public class BuildUtils : ScriptableObject
    {
        struct ProgramInfo
        {
            public List<uint> poolItems;
            public uint startOffset;
        }

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

        [Button]
        public void ExportMophunMemory()
        {
            string filePath = EditorUtility.OpenFilePanel("Choose mophun file", "", "*.mpn");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    VMGPExecutable executable = new VMGPExecutable(fileStream);
                    VMLoader loader = new VMLoader(executable);

                    uint totalSize = VMSystem.ProgramStartOffset + loader.EstimateNeededProgramSize();
                    VMMemory memory = new VMMemory(totalSize);

                    var poolDatas = loader.Load(
                        memory.GetMemorySpan((int)VMSystem.ProgramStartOffset, (int)loader.EstimateNeededProgramSize()),
                        VMSystem.ProgramStartOffset, null);

                    ProgramInfo programInfo = new ProgramInfo()
                    {
                        startOffset = VMSystem.ProgramStartOffset,
                        poolItems = poolDatas.Select(x =>
                        {
                            if (x == null || x.DataType == PoolDataType.Import || x.DataType == PoolDataType.None)
                            {
                                if (x.Name == "vTerminateVMGP")
                                {
                                    return 0x80000001;
                                }
                                else
                                {
                                    return 0x80000000;
                                }
                            }
                            else
                            {
                                return x.DataType == PoolDataType.ImmInteger
                                    ? x.ImmediateInteger.Value
                                    : (uint)BitConverter.SingleToInt32Bits(x.ImmediateFloat.Value);
                            }
                        }).ToList()
                    };

                    string programInfoJson = JsonUtility.ToJson(programInfo);
                    File.WriteAllText(Path.ChangeExtension(filePath, ".export.info"), programInfoJson);
                    File.WriteAllBytes(Path.ChangeExtension(filePath, ".export.bin"), memory.memory);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Export mophun memory failed with exception: {ex}");
            }
        }
    }
}
