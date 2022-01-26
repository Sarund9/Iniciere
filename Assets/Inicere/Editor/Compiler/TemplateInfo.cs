using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using UnityEngine;

namespace Iniciere
{
    //[Serializable]
    public class TemplateInfo : ScriptableObject
    {
        [SerializeField]
        TemplateLocation location;

        [SerializeField]
        string shortDescription = "", longDescription = "";

        [SerializeField]
        string m_TemplateName;
        [SerializeField]
        List<string> fileExts = new List<string>();
        [SerializeField]
        List<string> langs = new List<string>();
        [SerializeField]
        List<string> categories = new List<string>();
        [SerializeField]
        List<string> flags = new List<string>();
        [SerializeField]
        List<TemplateProperty> properties = new List<TemplateProperty>();
        [SerializeField]
        TemplateProperty fileNameProperty;

        //public TemplateInfo(TemplateLocation location)
        //{
        //    this.location = location;
        //}
        public static TemplateInfo New(TemplateLocation location)
        {
            var obj = CreateInstance<TemplateInfo>();
            obj.location = location;
            return obj;
        }
        //if (!File.Exists(filepath))
        //    throw new ArgumentException($"Filepath does not exist:\n{filepath}");
        //if (end <= start)
        //    throw new ArgumentException($"Start[{start}] and end[{end}] do not");
        //this.filepath = filepath;
        //this.start = start;
        //this.end = end;
        public string TmpName
        {
            get => m_TemplateName;
            set => m_TemplateName = value;
        }
        public List<string> FileExts => fileExts;
        public List<string> Langs => langs;
        public List<string> Categories => categories;
        public List<string> Flags => flags;
        public List<TemplateProperty> Properties => properties;
        
        public TemplateProperty FileNameProperty
        {
            get => fileNameProperty;
            set => fileNameProperty = value;
        }

        public string ShortDescription
        {
            get => shortDescription;
            set => shortDescription = value;
        }
        public string LongDescription
        {
            get => longDescription;
            set => longDescription = value;
        }

        public string GetContents() => location.GetContents();

        /// <summary> Load a string from the file </summary>
        //public string GetFromFile()
        //{
        //    return File
        //        .ReadLines(filepath)
        //        .Take(end)
        //        .Skip(start)
        //        .Aggregate(new StringBuilder(), (build, str) =>
        //        {
        //            build.Append(str);
        //            return build;
        //        })
        //        .ToString();
        //}
    }
    
    [Serializable]
    public class TemplateLocation
    {
        [SerializeField]
        string filepath;
        [SerializeField]
        int startChar, charCount;

        public TemplateLocation(string filepath, int startInChars, int countInChars)
        {
            this.filepath = filepath;
            startChar = startInChars;
            charCount = countInChars;
        }
        public string Filepath => filepath;
        public int StartChar => startChar;
        public int CharCount => charCount;

        string contentCache = null;

        public string GetContents() //TODO: Cache Results
        {
            if (contentCache is object)
            {
                return contentCache;
            }

            using var read = File.OpenText(Filepath);

            char[] contents = new char[CharCount];

            read.ReadBlock(new char[StartChar + 1], 0, StartChar + 1);
            read.ReadBlock(contents, 0, CharCount);
            

            //var d = new string(contents);

            //Debug.Log($"Getting contents at {StartChar} for {CharCount}, returns lenght {d.Length}:\n{d}");

            return contentCache = new string(contents);
        }
    }

}
