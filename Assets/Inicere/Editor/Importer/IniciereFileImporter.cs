using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Iniciere
{
    [ScriptedImporter(0, "iniciere")]
    public class IniciereFileImporter : ScriptedImporter
    {
        const string TMP_START = "<#iniciere";
        const string TMP_END = "#/>";

        [SerializeField]
        bool import;
        //List<string> files;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!import)
                return;

            var templateLocations = GetTemplates(ctx.assetPath)
                .ToList();

            foreach (var tmp in templateLocations)
            {
                var info = TemplateInfo.New(tmp);
                int result = Compiler.Precompile(tmp, info);

                if (result == 0)
                {
                    ctx.AddObjectToAsset(info.TmpName, info);
                }
            }

            

        }

        static IEnumerable<TemplateLocation> GetTemplates(string path)
        {
            using StreamReader stream = File.OpenText(path);

            bool inside = false;
            int insideCount = 0;
            int count = 0;
            //int start = 0;
            int i = 0;
            int c;
            do
            {
                c = stream.Read();
                char debug = (char)c;
                if (inside)
                {
                    count++;
                    if ((char)c == TMP_END[insideCount])
                    {
                        insideCount++;
                        if (TMP_END.Length == insideCount)
                        {
                            //all.Add(new TemplateLocation(path, i - count - tmpStart.Length, count + tmpStart.Length));
                            yield return new TemplateLocation(path, i - count - TMP_START.Length, count + TMP_START.Length);

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
                    if ((char)c == TMP_START[insideCount])
                    {
                        insideCount++;
                        if (TMP_START.Length == insideCount)
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
