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

#region OLD_CODE
/*
[SerializeField]
List<FailedTemplate> m_Failed = new List<FailedTemplate>();

public IEnumerable<FailedTemplate> Failed => m_Failed;

public IEnumerable<Item> Traverse()
{
    int c = 0;
    for (int i = 0; i < m_List.Count; )
    {
        if (c < m_Failed.Count && m_Failed[c].Index >= i)
        {
            yield return Item.NewFailed(m_Failed[c]);
            c++;
        }
        else
        {
            yield return Item.NewSuccess(m_List[c]);
            i++;
        }
    }
}

public struct Item
{
    public TemplateInfo Info { get; private set; }
    public FailedTemplate Failed { get; private set; }
    public bool HasFailed { get; private set; }
    public IEnumerable<LogEntry> Log => HasFailed ? Failed.Log : Info.PrecompileLog;
    public static Item NewFailed(FailedTemplate failed) => new Item
    {
        Info = null,
        Failed = failed,
        HasFailed = true,
    };
    public static Item NewSuccess(TemplateInfo info) => new Item
    {
        Info = info,
        Failed = null,
        HasFailed = false,
    };
}
        
*/
#endregion
