using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Iniciere
{
    public class IniciereConfig : ScriptableObject
    {
        internal static IniciereConfig Instance;

        //[SerializeField]
        //List<EditorFolder> editorFolders = new List<EditorFolder>();
        //// TODO: USE LISTS
        //[SerializeField]
        //List<string> projectNamespaces = new List<string>();

        public string projectNamespace;
        public string projectEditorFolder;
        public string projectEditorNamespace; 


        //public IList EditorFolders => editorFolders;
        //public IList ProjectNamespaces => projectNamespaces;
    }

    [Serializable]
    public struct EditorFolder
    {
        public string name;
        public string path;
        public string editorNamespace;
    }
}

