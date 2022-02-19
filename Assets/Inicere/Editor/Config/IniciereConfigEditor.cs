using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [CustomEditor(typeof(IniciereConfig))]
    public class IniciereConfigEditor : Editor
    {
        [InitializeOnLoadMethod]
        private static void OnInitialize()
        {
            FetchConfig();
        }
        private static void FetchConfig()
        {
            while (true)
            {
                if (IniciereConfig.Instance != null) return;

                var path = GetConfigPath();

                if (path == null)
                {
                    AssetDatabase.CreateAsset(CreateInstance<IniciereConfig>(), $"Assets/{nameof(IniciereConfig)}.asset");
                    Debug.Log("INICIERE: A config file has been created at the root of the project.");
                    continue;
                }

                IniciereConfig.Instance = AssetDatabase.LoadAssetAtPath<IniciereConfig>(path);

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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // TODO: Namespace
            // TODO: 

        }
    }
}

