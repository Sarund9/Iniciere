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
        List<LogEntry> precompileLog = new List<LogEntry>();
        [SerializeField]
        bool isFailed;

        public static TemplateInfo New(TemplateLocation location)
        {
            var obj = CreateInstance<TemplateInfo>();
            obj.location = location;
            return obj;
        }
        
        public string TmpName
        {
            get => m_TemplateName;
            set => m_TemplateName = value;
        }
        public List<string> FileExts => fileExts;
        public List<string> Langs => langs;
        public List<string> Categories => categories;
        public List<string> Flags => flags;
        public IList<TemplateProperty> Properties => properties;
        public List<LogEntry> PrecompileLog => precompileLog;
        public bool IsFailed => isFailed;

        public TemplateProperty FileNameProperty => Properties.FirstOrDefault(x => x.IsFileName);

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

        public string Filepath => location.Filepath;

        public string GetInfoContents() => location.GetInfoContents();

        public string GetBodyContents() => location.GetBodyContents();

        public void LogTrc(string msg) =>
            precompileLog.Add(new LogEntry(LogLevel.Trc, msg));
        public void LogMsg(string msg) =>
            precompileLog.Add(new LogEntry(LogLevel.Msg, msg));
        public void LogWrn(string msg) =>
            precompileLog.Add(new LogEntry(LogLevel.Wrn, msg));
        public void LogErr(string msg)
        {
            isFailed = true;
            precompileLog.Add(new LogEntry(LogLevel.Err, msg));
        }

        public int ErrCode()
        {
            return isFailed ? 1 : 0;
        }

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
        int infoCharPos, infoCharCount,
            bodyCharPos, bodyCharCount;

        public TemplateLocation(string filepath, int startInChars, int infoPosInChars, int bodyPosInChars = 0)
        {
            this.filepath = filepath;
            //startChar = startInChars;
            infoCharPos = infoPosInChars;
            bodyCharPos = bodyPosInChars;

            // Verification
            if (startInChars < infoPosInChars || infoPosInChars < bodyPosInChars)
            {
                throw new ArgumentException("Bad Template Location");
            }
        }

        public TemplateLocation(string filepath,
            int infoCharPos, int infoCharCount,
            int bodyCharPos, int bodyCharCount)
        {
            this.filepath = filepath;
            
            this.infoCharPos = infoCharPos;
            this.infoCharCount = infoCharCount;

            this.bodyCharPos = bodyCharPos;
            this.bodyCharCount = bodyCharCount;
            
            // Verification
            if (infoCharPos >= bodyCharPos ||
                infoCharCount < 1 ||
                bodyCharCount < 1)
            {
                throw new ArgumentException("Bad Template Location");
            }
        }

        public string Filepath => filepath;
        public int StartChar => 0;
        public int InfoCharPos => infoCharPos;
        public int InfoCharCount => infoCharCount;
        public int BodyCharPos => bodyCharPos;
        public int BodyCharCount => bodyCharCount;

        string infocontentCache = null;
        string bodycontentCache = null;

        public string GetInfoContents() //TODO: Cache Results
        {
            if (!string.IsNullOrEmpty(infocontentCache))
            {
                return infocontentCache;
            }

            using var read = File.OpenText(Filepath);

            char[] contents = new char[InfoCharCount];

            read.ReadBlock(new char[InfoCharPos], 0, InfoCharPos);
            read.ReadBlock(contents, 0, InfoCharCount);
            
            return infocontentCache = new string(contents);
        }

        public string GetBodyContents()
        {
            if (!string.IsNullOrEmpty(bodycontentCache))
            {
                return bodycontentCache;
            }

            using var read = File.OpenText(Filepath);

            char[] contents = new char[BodyCharCount];

            read.ReadBlock(new char[BodyCharPos], 0, BodyCharPos);
            read.ReadBlock(contents, 0, BodyCharCount);

            return bodycontentCache = new string(contents);
        }

        public override string ToString()
        {
            return $"TMP IN '{Filepath}' AT {StartChar} FOR {InfoCharCount} + {BodyCharPos}";
        }
    }

}
