using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class IniciereConfig : ScriptableObject
    {

        //[SerializeField]
        //List<EditorFolder> editorFolders = new List<EditorFolder>();
        //// TODO: USE LISTS
        //[SerializeField]
        //List<string> projectNamespaces = new List<string>();

        public string projectNamespace;         // Implemented
        public string projectEditorNamespace;   // Implemented
        public bool useEditorFolder;
        public string projectEditorFolder;      // Implemented

        public static IniciereConfig Instance { get; private set; }

        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            FetchConfig();
        }
        private static void FetchConfig()
        {
            while (true)
            {
                if (Instance != null) return;

                var path = GetConfigPath();

                if (path == null)
                {
                    AssetDatabase.CreateAsset(CreateInstance<IniciereConfig>(), $"Assets/{nameof(IniciereConfig)}.asset");
                    Debug.Log("INICIERE: A config file has been created at the root of the project.");
                    continue;
                }

                Instance = AssetDatabase.LoadAssetAtPath<IniciereConfig>(path);

                break;
            }
        }
        private static string GetConfigPath()
        {
            var paths = AssetDatabase.FindAssets(nameof(IniciereConfig))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(c => c.EndsWith(".asset"))
                .ToList();
            if (paths.Count > 1) Debug.LogWarning("Multiple auto save config assets found. Delete one.");
            return paths.FirstOrDefault();
        }

    }

    [Serializable]
    public struct EditorFolder
    {
        public string name;
        public string path;
        public string editorNamespace;
    }
}

