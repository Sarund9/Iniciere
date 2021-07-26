using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Iniciere
{
    public class InicereScriptFinder
    {
        const string FILE_EX = "iniciere";
        const string FOLDER_NAME = "InicereTemplates";

        public static IEnumerable<string> FindFilePaths() =>
            GetDirectories(Directory.GetCurrentDirectory())
                .SelectMany(d => Directory.GetFiles(d, $"*.{FILE_EX}"));
        //.Select(f => File.ReadAllText(f));
        //string path = Application.dataPath + "/";
        //path = path.Replace('/','\\');
        //Debug.Log($"PATH: \n{path}");
        //foreach (var file in files)
        //{
        //    Debug.Log($"FILE: \n{file}");
        //}

        public static IEnumerable<string> FindTemplatesInFiles(
            IEnumerable<string> filepaths,
            string tmpStart = "<#iniciere",
            string tmpEnd = "#/>")
        {
            List<string> all = new List<string>();
            foreach (var path in filepaths)
            {
                var contents = File.ReadAllLines(path);
                StringBuilder builder = new StringBuilder();
                bool inside = false;
                foreach (var line in contents)
                {
                    if (inside)
                    {
                        if (line.StartsWithOrWhitespace(tmpEnd))
                        {
                            inside = false;
                            builder.Append(line);
                            all.Add(builder.ToString());
                            builder.Clear();
                        }
                        else
                        {
                            builder.AppendLine(line);
                        }
                    }
                    else
                    {
                        if (line.StartsWithOrWhitespace(tmpStart))
                        {
                            inside = true;
                            builder.AppendLine(line);
                        }
                    }
                }
            }
            return all;
        }

        public static List<TemplateLocation> FindTemplatesLite(
            IEnumerable<string> filepaths,
            string tmpStart = "<#iniciere",
            string tmpEnd = "#/>")
        {
            List<TemplateLocation> all = new List<TemplateLocation>();
            
            foreach (var path in filepaths)
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
                        if ((char)c == tmpEnd[insideCount])
                        {
                            insideCount++;
                            if (tmpEnd.Length == insideCount)
                            {
                                all.Add(new TemplateLocation(path, i - count - tmpStart.Length, count + tmpStart.Length));

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
                        if ((char)c == tmpStart[insideCount])
                        {
                            insideCount++;
                            if (tmpStart.Length == insideCount)
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
            

            return all;
        }
        //var contents = File.ReadAllLines(path);
        //foreach (var line in contents)
        //{
        //    if (line.StartsWithOrWhitespace(tmpStart))
        //    {
        //        var tmpname = StringUtils.CaptureInBetween(line);
        //    }
        //}

        public static IEnumerable<string> GetDirectories(string path)
        {
            //Debug.Log($"PATH: \n{path}");
            var dirs = Directory.GetDirectories(path);

            foreach (var dir in dirs)
            {
                if (dir.EndsWith(FOLDER_NAME))
                    yield return dir;
                else
                {
                    foreach (var subdir in GetDirectories(path + "/" + Path.GetFileName(dir)))
                        yield return subdir;
                }
            }

        }
    }
}

//var filteered = (string)input.Clone();

//var lines = filteered.Split('\n');
//for (int i = 0; i < lines.Length; i++)
//{
//    StringUtils.FilterComments(ref lines[i]);
//    GUILayout.Label(lines[i]);
//}
//output = EditorGUILayout.TextArea(output);
