using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Iniciere
{
    public class ReflectionManager
    {
        static ReflectionManager I;

        Assembly[] assemblies;
        IEnumerable<Type> types;
        IEnumerable<MethodInfo> methods;
        IEnumerable<MethodInfo> macros;
        IEnumerable<MethodInfo> decorators;

        private ReflectionManager()
        {}

        public static ReflectionManager GetInstance()
        {
            if (I is null)
            {
                I = new ReflectionManager();
            }
            return I;
        }

        public Assembly[] GetAssemblies()
        {
            if (assemblies is null)
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            return assemblies;
        }
        public IEnumerable<Type> GetTypes()
        {
            if (types is null)
            {
                types = GetAssemblies().SelectMany(a => a.GetTypes());
            }
            return types;
        }
        public IEnumerable<MethodInfo> GetMethods()
        {
            if (methods is null)
            {
                methods = GetTypes().SelectMany(t => t.GetMethods());
            }
            return methods;
        }
        public IEnumerable<MethodInfo> GetDecoratorMethods()
        {
            if (methods is null)
            {
                methods = GetTypes().SelectMany(t => t.GetMethods());
            }
            return methods;
        }
    }
}


#region OLD_CODE
/*
const int SIZE = 20;
const int MARGIN = 10;

Rect rect = new Rect
{
    x = Screen.width - SIZE,
    y = SIZE,
    width = SIZE,
    height = SIZE,
};

if (GUI.Button(rect, "X"))
{
    Close();
}

*/
#endregion