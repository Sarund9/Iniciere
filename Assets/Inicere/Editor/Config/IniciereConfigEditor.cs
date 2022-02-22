using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    [CustomEditor(typeof(IniciereConfig))]
    public class IniciereConfigEditor : Editor
    {
        IniciereConfig obj;

        private void OnEnable()
        {
            obj = (IniciereConfig)target;
        }

        public override void OnInspectorGUI()
        {

            #region NAMESPACE
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Project Namespace");
            var newProjNamespace = GUILayout.TextField(obj.projectNamespace);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(obj, "Change Iniciere Project Namespace");
                obj.projectNamespace = newProjNamespace;
            }
            if (IsNamespaceInvalid(obj.projectNamespace))
            {
                EditorGUILayout.HelpBox("Namespace is Invalid!", MessageType.Error);
            }

            GUILayout.Space(5f);
            #endregion

            #region EDITOR_NAMESPACE
            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Project EditorNamespace");
            var newProjEditorNamespace = GUILayout.TextField(obj.projectEditorNamespace);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(obj, "Change Iniciere Project EditorNamespace");
                obj.projectEditorNamespace = newProjEditorNamespace;
            }
            if (IsNamespaceInvalid(obj.projectEditorNamespace))
            {
                EditorGUILayout.HelpBox("EditorNamespace is Invalid!", MessageType.Error);
            }

            GUILayout.Space(5f);
            #endregion

            #region USE_EDITOR_FOLDER
            EditorGUI.BeginChangeCheck();
            var newVal = GUILayout.Toggle(obj.useUniqueEditorFolder, "Use Unique Editor Folder");
            GUILayout.Space(4f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(obj, newVal ?
                    "Enable Unique editor Folder"
                    : "Disable Unique Editor Folder"
                    );

                obj.useUniqueEditorFolder = newVal;
            }
            #endregion

            #region EDITOR_PATH
            if (obj.useUniqueEditorFolder)
            {
                EditorGUI.BeginChangeCheck();
                var newFolder = GUILayout.TextField(obj.projectEditorFolder);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj, 
                        "Change Editor Folder"
                        );

                    obj.projectEditorFolder = newFolder;
                }

                // Validate
                var invalidResult = IsFolderInvalid(obj.projectEditorFolder, out var err);
                if (invalidResult != Result.Ok)
                {
                    EditorGUILayout.HelpBox($"{err}", GetMsgType());
                    MessageType GetMsgType() => invalidResult switch
                    {
                        Result.Err => MessageType.Error,
                        Result.Wrn => MessageType.Warning,
                        _ => MessageType.None,
                    };
                }
            }
            #endregion

        }

        static Result IsFolderInvalid(string path, out string msg)
        {
            if (Regex.IsMatch(path, @"[<>:""|?*\0]", RegexOptions.IgnoreCase))
            {
                msg = "EditorFolder is Invalid!\nPath contains invalid characters";
                return Result.Err;
            }
            var fullpath = $"{Application.dataPath}/{path}";
            if (!Directory.Exists(fullpath))
            {
                msg = "Directory does not exist, this path will not be used!";
                return Result.Wrn;
            }
            if (File.Exists(fullpath))
            {
                msg = "EditorFolder is Invalid!\nPath leads to Existing File!";
                return Result.Err;
            }
            // [\\/];
            if (!Regex.IsMatch(path, @"[\\/]editor[\\/]|[\\/]editor$|^editor$|^editor[\\/]", RegexOptions.IgnoreCase))
            {
                msg = "EditorFolder not an Editor folder, '/editor' will be added " +
                    "at the end of your path";
                return Result.Wrn;
            }
            if (Regex.IsMatch(path, @"^[\\/]|[\\/]$", RegexOptions.IgnoreCase))
            {
                msg = "Path should not contains Slashes at the beginning or the end";
                return Result.Err;
            }
            //if (Regex.IsMatch(path, MATCH_Win32Files))
            //{
            //   // TODO: Invalidate these file types
            //}

            msg = null;
            return Result.Ok;
        }
        enum Result
        {
            Ok,
            Err,
            Wrn,
        }


        static bool IsNamespaceInvalid(string str)
        {
            return Regex.IsMatch(str, @"[^\w.]|[^\w]\d|\.$|^\.|^\d", RegexOptions.IgnoreCase);
        }
    }
}
