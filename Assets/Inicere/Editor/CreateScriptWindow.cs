using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Iniciere
{
    public class CreateScriptWindow : EditorWindow
    {
        //List<TemplateLocation> templates = new List<TemplateLocation>();
        
        List<Task<TemplateInfo>> precompiling = new List<Task<TemplateInfo>>();

        List<TemplateInfo> templates = new List<TemplateInfo>();
        //Thread thread;

        [MenuItem("Assets/Create/Script", priority = 80)] //80 is C# Script Priority
        public static void Create()
        {
            var win = CreateInstance<CreateScriptWindow>();

            win.minSize = new Vector2(460, 580);
            win.maxSize = new Vector2(2000, 1600);

            //win.position = new Rect(200, 200);
#if UNITY_2020_1_OR_NEWER
            Rect pos = EditorGUIUtility.GetMainWindowPosition();
#else
            Rect pos = Extensions.GetEditorMainWindowPos2019();
#endif

            Vector2 scale = new Vector2(0.4f, 0.7f);
            Vector2 pivot = new Vector2(pos.width / 2, pos.height / 2);
            win.position = pos.ScaleSizeBy(scale, pivot);

            win.titleContent = new GUIContent("Create new Script file", "Create a new Script File");

            //win.ShowModalUtility();
            win.ShowUtility();
        }

        //void ProcessAsync(Action action, Action callback = null)
        //{
        //    thread = new Thread(Exec);
        //    thread.Start(action);
        //    void Exec(object del)
        //    {
        //        (del as Action).Invoke();
        //        callback?.Invoke();
        //        Thread.Sleep(1);
        //    }
        //}

        private void OnEnable()
        {
            //((Action)FindFiles).BeginInvoke(OnFinished)
            //templates.CollectionChanged += TemplateColChanged;
            var _ = FindFiles();
        }

        private void TemplateColChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //e.NewItems[0]
            }
        }

        private void OnGUI()
        {
            //GUILayout.Label("Template Window");

            foreach (var item in precompiling)
            {
                GUILayout.Label($"Template: [{item.}]");
            }
        }

        private void Update()
        {
            
        }
        private void OnInspectorUpdate()
        {
            for (int i = 0; i < precompiling.Count; i++)
            {
                if (precompiling[i].IsCompleted)
                {
                    var info = precompiling[i].Result;
                    if (info != null)
                    {
                        //Debug.Log($"Template precompiled: {info.Name}");
                        templates.Add(info);
                    }
                    precompiling.RemoveAt(i);
                    i--;
                }
            }
        }

        private async Task FindFiles(Action callback = null)
        {
            IEnumerable<string> filepaths = await Task.Run(() => InicereScriptFinder.FindFilePaths());

            List<TemplateLocation> templates = await Task.Run(() => InicereScriptFinder.FindTemplatesLite(filepaths));

            foreach (var item in templates)
            {
                //this.templates.Add(item);

                var task = PrecompileTemplate(item);
                precompiling.Add(task);
            }
            //return templates;
        }
        private async Task<TemplateInfo> PrecompileTemplate(TemplateLocation template)
        {
            TemplateInfo info = null;
            int result = await Task.Run(() => Compiler.Precompile(template, out info));

            if (result != 0)
            {
                //Task.
                return null;
            }

            return info;
        }
    }
}


#region OLD_CODE
/*
const int SIZE = 20;
const int MARGIN = 10;

Rect rect = new Rect
{
    x = Screen.width - SIZE,
    y = SIZE,
    width = SIZE,
    height = SIZE,
};

if (GUI.Button(rect, "X"))
{
    Close();
}

*/
#endregion