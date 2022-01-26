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

        public Type ValueType
        {
            get
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
        }

        public void Set(object value)
        {
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
                json = JsonUtility.ToJson(value, false);
            }
        }

        public object Get()
        {
            return JsonUtility.FromJson(json, ValueType);
        }
    }
}
