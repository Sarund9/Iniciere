﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Iniciere
{
    public static class StandartMacros
    {
        public delegate bool TryFunc<T, TResult>(T value, out TResult result);
        public static IEnumerable<TResult> SelectWhere<T, TResult>(
            this IEnumerable<T> col, TryFunc<T, TResult> selector
            )
        {
            foreach (var item in col)
            {
                if (selector(item, out var result))
                    yield return result;
            }
        }

        [IniciereMacro("INSERT")]
        static void Insert(StringBuilder build, MacroContext ctx, string obj)
        {


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

            Debug.Log($"FORMAT HAS RAN \n{debug}\n Turned Into \n{build}");
        }
    }

    public sealed class MacroContext
    {
        private MacroContext() { }

        public Assembly[] Assemblies { get; private set; }
        public IEnumerable<Type> Types { get; private set; }
        public IEnumerable<MethodInfo> Methods { get; private set; }
        public IEnumerable<MacroInstance> Macros { get; private set; }

        public static IEnumerable<MacroInstance> InitializeMacroSystem(AppDomain domain, out MacroContext ctx)
        {
#pragma warning disable IDE0017 // Simplify object initialization
            ctx = new MacroContext();
#pragma warning restore IDE0017 // Simplify object initialization

            ctx.Assemblies = domain.GetAssemblies();
            ctx.Types = ctx.Assemblies.SelectMany(a => a.GetTypes());
            ctx.Methods = ctx.Types.SelectMany(t => t.GetMethods());

            return ctx.Macros = ctx.Methods
                .SelectWhere((MethodInfo m, out MacroInstance mi) =>
                {
                    var atr = m.GetCustomAttribute<IniciereMacroAttribute>();

                    if (atr is object)
                    {
                        mi = new MacroInstance(m, atr);

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

    public class MacroInstance
    {
        public MacroInstance(MethodInfo method, IniciereMacroAttribute atr)
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
        public MacroExecutionInstance(MacroInstance macro, object[] @params)
        {
            Macro = macro;
            Params = @params;
        }

        public MacroInstance Macro { get; }
        
        /// <summary> Parameters without the first 2 inputs of All Macros </summary>
        public object[] Params { get; }
    }
}