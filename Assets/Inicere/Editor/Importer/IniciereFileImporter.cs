using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Iniciere
{
    [ScriptedImporter(0, "iniciere")]
    public class IniciereFileImporter : ScriptedImporter
    {
        const string TEMPLATE_START = @"<#iniciere";
        const string TEMPLATE_DIV = @"\=/";
        const string TEMPLATE_END = @"//>";
        [SerializeField]
        bool import, importFirstOnly;
        //List<string> files;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!import)
                return;

            var templateLocations = GetTemplates(ctx.assetPath);

            List<TemplateInfo> list = new List<TemplateInfo>();

            foreach (var tmp in templateLocations)
            {
                var log = new List<LogEntry>();

                var info = TemplateInfo.New(tmp);
                int result = Compiler.Precompile(tmp, info);

                if (result == 0)
                {
                    ctx.AddObjectToAsset(info.name, info);
                    list.Add(info);
                }
                if (importFirstOnly)
                    break;
            }

            var header = TemplateHeader.From(list);
            ctx.AddObjectToAsset(Path.GetFileNameWithoutExtension(ctx.assetPath) + "_tmps", header);
            ctx.SetMainObject(header);
        }


        public static IEnumerable<TemplateLocation> GetTemplates(string path)
        {
            using StreamReader stream = File.OpenText(path);

            var state = State.Outside;
            int infoCharPos = 0, infoCharCount = 0,
                bodyCharPos = 0, bodyCharCount = 0;

            string line = "";
            int count = 0;
            int newLineLenght = Environment.NewLine.Length;
            while (true)
            {
                line = stream.ReadLine();
                if (line == null)
                    break;
                
                switch (state)
                {
                    case State.Outside:
                        int sfind = line.IndexOf(TEMPLATE_START);
                        if (sfind > -1)
                        {
                            //Debug.Log($"ENTER at {sfind}");
                            state = State.InsideInfo;
                            infoCharPos = count + sfind + TEMPLATE_START.Length;
                        }
                        break;
                    case State.InsideInfo:
                        int dfind = line.IndexOf(TEMPLATE_DIV);
                        if (dfind > -1)
                        {
                            state = State.InsideBody;
                            infoCharCount = (count + dfind + 3) - infoCharPos;
                            bodyCharPos = count + dfind;
                        }
                        break;
                    case State.InsideBody:
                        int efind = line.IndexOf(TEMPLATE_END);
                        if (efind > -1)
                        {
                            bodyCharCount = (count + efind + 3) - bodyCharPos;

                            yield return new TemplateLocation(path,
                                infoCharPos, infoCharCount,
                                bodyCharPos, bodyCharCount);

                            state = State.Outside;
                            infoCharPos = 0; infoCharCount = 0;
                            bodyCharPos = 0; bodyCharCount = 0;
                        }
                        break;
                    default: break;
                }
                count += line.Length + newLineLenght;
            }
            
            
            yield break;
        }
        enum State : byte
        {
            Outside,
            InsideInfo,
            InsideBody,
        }
    }
}

#region OLD_CODE
/*

//static IEnumerable<TemplateLocation> NewGetTemplates(string path)
//{
//    var text = File.ReadAllText(path);

//    text.FindAll(TMP_START)
//}

//static IEnumerable<TemplateLocation> GetTemplates(string path)
//{
//    using StreamReader stream = File.OpenText(path);

//    bool inside = false;
//    int insideCount = 0;
//    int count = 0;
//    //int start = 0;
//    int i = 0;
//    int c;
//    // 4000 = 80K
//     
//    int watchDog = 50000;

//    do
//    {
//        c = stream.Read();
//        watchDog--;

//        if (watchDog < 0)
//        {
//            throw new System.Exception("Watch Dog Limit");
//        }

//        char debug = (char)c;
//        if (inside)
//        {
//            count++;
//            if ((char)c == TEMPLATE_END[insideCount])
//            {
//                insideCount++;
//                if (TEMPLATE_END.Length == insideCount)
//                {
//                    //all.Add(new TemplateLocation(path, i - count - tmpStart.Length, count + tmpStart.Length));
//                    yield return new TemplateLocation(
//                        path,
//                        i - count - TEMPLATE_START.Length,
//                        count + TEMPLATE_START.Length
//                        );

//                    inside = false;
//                    insideCount = 0;
//                    count = 0;
//                }
//            }
//            else
//            {
//                insideCount = 0;
//            }
//        }
//        else
//        {
//            if ((char)c == TEMPLATE_START[insideCount])
//            {
//                insideCount++;
//                if (TEMPLATE_START.Length == insideCount)
//                {
//                    inside = true;
//                    insideCount = 0;
//                }
//            }
//            else
//            {
//                insideCount = 0;
//            }
//        }
//        i++;
//    }
//    while (c != -1);

//}


*/
#endregion
