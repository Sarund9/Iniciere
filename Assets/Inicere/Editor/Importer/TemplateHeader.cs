using System.Collections.Generic;
using UnityEngine;

namespace Iniciere
{
    public class TemplateHeader : ScriptableObject
    {
        [SerializeField]
        List<TemplateInfo> m_List = new List<TemplateInfo>();


        public IEnumerable<TemplateInfo> Templates => m_List;

        public static TemplateHeader From(List<TemplateInfo> infoList)
        {
            var newObj = CreateInstance<TemplateHeader>();

            newObj.m_List = infoList;

            return newObj;
        }

    }
}
