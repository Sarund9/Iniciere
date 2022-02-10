using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Iniciere
{
    /// <summary>
    /// Serializes any Value into Unity as a Json String
    /// </summary>
    [Serializable]
    public class UBox
    {
        [SerializeField]
        string typeID = "", aseID = "", json = "";

        Type typeCache;
        object valueCache;

        private Type GetValueType()
        {
            if (string.IsNullOrEmpty(json))
                return null;

            if (typeCache is null)
            {
                typeCache = Type.GetType(typeID);
                if (typeCache is null)
                {
                    typeCache = Type.GetType(aseID);
                }
            }
            return typeCache;
        }

        public void Set(object value)
        {
            valueCache = null;
            if (value is null || value == null)
            {
                typeID = ""; aseID = ""; json = "";
                typeCache = null;
            }
            else
            {
                typeCache = value.GetType();
                typeID = typeCache.FullName;
                aseID = typeCache.AssemblyQualifiedName;
                if (typeCache == typeof(string))
                    json = (string)value;
                else
                    json = JsonUtility.ToJson(value, false);
            }
        }

        public object Get()
        {
            if (valueCache is null) {
                var type = GetValueType();
                if (type == typeof(string))
                    valueCache = json;
                else
                    valueCache = JsonUtility.FromJson(json, type);
            }
            return valueCache;
        }
    }

    /// <summary>
    /// Serializes functions by MethodInfo into unity
    /// </summary>
    [Serializable]
    public class UFuncBox
    {
        [SerializeField]
        string typeID = "", aseID = "", fnname = "";

        Type typeCache;
        MethodInfo infoCache;

        Type GetDeclType()
        {
            if (string.IsNullOrEmpty(fnname))
                return null;
            if (typeCache is null)
            {
                typeCache = Type.GetType(typeID);
                if (typeCache is null)
                {
                    typeCache = Type.GetType(aseID);
                }
            }
            return typeCache;
        }
        MethodInfo GetFnInfo()
        {
            if (infoCache is null) {
                var type = GetDeclType();
                infoCache = type.GetMethod(fnname);
            }
            return infoCache;
        }

        public void Set<T>(T del)
            where T : Delegate
        {
            if (del is null || del == null)
            {
                typeID = ""; aseID = ""; fnname = "";
                typeCache = null;
            }
            else
            {
                infoCache = del.GetMethodInfo();
                typeCache = infoCache.DeclaringType;

                typeID = typeCache.FullName;
                aseID = typeCache.AssemblyQualifiedName;
                fnname = infoCache.Name;
            }
        }

        public T Get<T>()
            where T : Delegate
        {
            var info = GetFnInfo();

            return (T)info.CreateDelegate(typeof(T));
        }
    }
}
