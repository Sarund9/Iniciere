using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Iniciere
{
    public delegate bool TryFunc<T, TResult>(T value, out TResult result);

    public static class StandartMacros
    {
        
        [IniciereMacro("IF")] //TODO: real ifs
        public static void IfMacro(StringBuilder build, MacroContext ctx, bool predicate)
        {
            if (!predicate)
                build.Clear();
        }

        [IniciereMacro("TABIF")]
        public static void TabIf(StringBuilder build, MacroContext ctx, bool predicate = true)
        {
            if (!predicate)
                return; //TODO: real ifs

            for (int i = 0; i < build.Length; i++)
            {
                if (build.IsAt(Environment.NewLine, i))
                {
                    build.Insert(i, '\t');
                }
            }
        }


        [IniciereMacro("VERIFYCSNAMESPACE")]
        static void Insert(StringBuilder build, MacroContext ctx)
        {
            var text = new TextBuilder(build);

            var namespaces = ctx.Types
                .Select(t => t.Namespace)
                .Distinct();

            while (!text.IsFinished)
            {
                //const int SPACE = 6;
                if (text.TryGoTo("using", TextBuilder.SelectionMode.Set1))
                {
                    text.Next(6, false);

                }

                text.Next();
            }

        }
        [IniciereMacro("FORMAT")]
        public static void Format(StringBuilder build, MacroContext ctx, params object[] objs)
        {
            // TODO: ignore in between '"'
            int indexless = 0;

            string debug = build.ToString();

            for (int i = 0; i < build.Length; i++)
            {
                if (build[i] == '{')
                {
                    if (Left(1) && build[i + 1] == '}')
                    {
                        string toInsert = objs[indexless].ToString();

                        build.Remove(i, 2);
                        build.Insert(i, toInsert);

                        indexless++;
                        i += toInsert.Length;
                    }
                    else if (Left(2) && build[i + 2] == '}')
                    {
                        // TODO: indexes greater than 9
                        char c = build[i + 1];
                        if (!int.TryParse(c.ToString(), out var index))
                        {
                            //ctx.ThrowExeption($"{c} is not an indexer!");
                            return;
                        }
                        if (!InRange(index))
                        {
                            //ctx.ThrowExeption($"{index} is out of range");
                            Debug.LogError($"{index} is out of range");
                            return;
                        }

                        string toInsert = objs[index].ToString();

                        build.Remove(i, 3);
                        build.Insert(i, toInsert);

                        i += toInsert.Length;
                    }
                }

                
                bool Left(int a) => i < build.Length - a;
            }

            bool InRange(int i) => i > -1 && i < objs.Length;

            //Debug.Log($"FORMAT HAS RAN \n{debug}\n Turned Into \n{build}");
        }
    }

    public sealed class MacroContext
    {
        private MacroContext() { }

        public Assembly[] Assemblies { get; private set; }
        public IEnumerable<Type> Types { get; private set; }
        public IEnumerable<MethodInfo> Methods { get; private set; }
        public IEnumerable<MacroTypeInstance> Macros { get; private set; }

        public static IEnumerable<MacroTypeInstance> InitializeMacroSystem(AppDomain domain, out MacroContext ctx)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            ctx = new MacroContext();
#pragma warning restore IDE0017 // Simplify object initialization

            ctx.Assemblies = domain.GetAssemblies();
            ctx.Types = ctx.Assemblies.SelectMany(a => a.GetTypes());
            ctx.Methods = ctx.Types.SelectMany(t => t.GetMethods());

            return ctx.Macros = ctx.Methods
                .SelectWhere((MethodInfo m, out MacroTypeInstance mi) =>
                {
                    var atr = m.GetCustomAttribute<IniciereMacroAttribute>();

                    if (atr is object)
                    {
                        mi = new MacroTypeInstance(m, atr);

                        if (!m.IsStatic && VerifyParams(m))
                            return false;

                        return true;
                    }
                    else
                    {
                        mi = null;
                        return false;
                    }
                });
        }

        private static bool VerifyParams(MethodInfo m)
        {
            var ps = m.GetParameters();
            return ps.Length >= 2 &&
                (ps[0].ParameterType == typeof(StringBuilder) && !ps[0].IsOut)
                &&
                (ps[1].ParameterType == typeof(MacroContext) && !ps[1].IsOut);

            //bool VerifyParam(ParameterInfo info) =>
            //    !info.IsOut && info.
        }

        public void ThrowExeption(string msg)
        {

        }
        public void Warn(string msg)
        {
            Debug.LogWarning(msg);
        }
    }

    public class MacroTypeInstance
    {
        public MacroTypeInstance(MethodInfo method, IniciereMacroAttribute atr)
        {
            Method = method;
            Atr = atr;
            //Params = new object[atr.InputSign.Length];
            var ps = Method.GetParameters();
            UsesParams = ps[ps.Length - 1]
                .IsDefined(typeof(ParamArrayAttribute), false);
            ParamCount = ps.Length - 2;
        }

        public MethodInfo Method { get; }
        public IniciereMacroAttribute Atr { get; }

        public bool UsesParams { get; }
        public int ParamCount { get; }

        //public object[] Params { get; }

        //public int ParamCount => Params.Length;
        public string Name => Atr.HasName? Atr.Name : Method.Name.ToUpper();
    }

    public class MacroExecutionInstance
    {
        public MacroExecutionInstance(MacroTypeInstance macro, object[] @params)
        {
            Macro = macro;
            Params = @params;
        }

        public MacroTypeInstance Macro { get; }
        
        /// <summary> Parameters without the first 2 inputs of All Macros </summary>
        public object[] Params { get; }
    }
}
