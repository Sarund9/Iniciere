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
            //Debug.Log($"BREAK");
            foreach (var tmp in templateLocations)
            {
                //Debug.Log($"BREAK");
                var info = TemplateInfo.New(tmp);
                int result = NewCompiler.Precompile(tmp, info);

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

        //static IEnumerable<TemplateLocation> NewGetTemplates(string path)
        //{
        //    var text = File.ReadAllText(path);

        //    text.FindAll(TMP_START)
        //}

        static IEnumerable<TemplateLocation> GetTemplates(string path)
        {
            using StreamReader stream = File.OpenText(path);

            bool inside = false;
            int insideCount = 0;
            int count = 0;
            //int start = 0;
            int i = 0;
            int c;

            /*
            
            4000 = 80K
             */
            int watchDog = 50000;

            do
            {
                c = stream.Read();
                watchDog--;
                
                if (watchDog < 0)
                {
                    throw new System.Exception("Watch Dog Limit");
                }

                char debug = (char)c;
                if (inside)
                {
                    count++;
                    if ((char)c == TEMPLATE_END[insideCount])
                    {
                        insideCount++;
                        if (TEMPLATE_END.Length == insideCount)
                        {
                            //all.Add(new TemplateLocation(path, i - count - tmpStart.Length, count + tmpStart.Length));
                            yield return new TemplateLocation(
                                path,
                                i - count - TEMPLATE_START.Length,
                                count + TEMPLATE_START.Length
                                );

                            inside = false;
                            insideCount = 0;
                            count = 0;
                        }
                    }
                    else
                    {
                        insideCount = 0;
                    }
                }
                else
                {
                    if ((char)c == TEMPLATE_START[insideCount])
                    {
                        insideCount++;
                        if (TEMPLATE_START.Length == insideCount)
                        {
                            inside = true;
                            insideCount = 0;
                        }
                    }
                    else
                    {
                        insideCount = 0;
                    }
                }
                i++;
            }
            while (c != -1);

        }
    }
}
