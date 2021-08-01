using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System;
using UnityEngine;

namespace Iniciere
{
    [Serializable]
    public class TemplateInfo
    {
        TemplateLocation location;

        public TemplateInfo(TemplateLocation location)
        {
            this.location = location;
        }
        //if (!File.Exists(filepath))
        //    throw new ArgumentException($"Filepath does not exist:\n{filepath}");
        //if (end <= start)
        //    throw new ArgumentException($"Start[{start}] and end[{end}] do not");
        //this.filepath = filepath;
        //this.start = start;
        //this.end = end;
        public string Name { get; set; }
        public List<string> FileExts { get; }
            = new List<string>();
        public List<string> Langs { get; }
            = new List<string>();
        public List<string> Categories { get; }
            = new List<string>();
        public List<string> Flags { get; }
            = new List<string>();
        public List<TemplateProperty> Properties { get; }
            = new List<TemplateProperty>();
        //public string Contents { get; set; }
        public string ShortDescription { get; set; } = "";
        public string LongDescription { get; set; } = "";

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
        public TemplateLocation(string filepath, int startInChars, int countInChars)
        {
            Filepath = filepath;
            StartChar = startInChars;
            CharCount = countInChars;
        }
        public string Filepath { get; }
        public int StartChar { get; }
        public int CharCount { get; }

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
