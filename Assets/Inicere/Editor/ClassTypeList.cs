using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Iniciere
{
    public class ClassTypeList
    {
        Dictionary<Type, Node> allTypes
            = new Dictionary<Type, Node>();

        List<Node> roots = new List<Node>();

        public IEnumerable<Node> Roots => roots;

        private ClassTypeList()
        {
        }

        public bool TryGetType(Type type, out Node node)
        {
            return allTypes.TryGetValue(type, out node);
        }

        public static async Task<ClassTypeList> GetNewAsync(IEnumerable<Type> classTypes)
        {
            var list = new ClassTypeList();
            await Task.Run(() =>
            {
                foreach (var type in classTypes)
                {
                    if (type == null)
                    {
                        Debug.Log("NULL TYPE WTF");
                        continue;
                    }
                    list.allTypes.Add(type, new Node(list, type));
                }
                foreach (var node in list.allTypes.Values)
                {
                    node.LinkParents();
                }
            });
            return list;
        }

        public class Node
        {
            public ClassTypeList parentList;
            public Type type;
            public bool foldout = true;

            public Node(ClassTypeList parentList, Type type)
            {
                this.parentList = parentList;
                this.type = type;
            }

            public List<Node> Children { get; } = new List<Node>();
            
            public int NumParents
            {
                get
                {
                    int n = 0;
                    Type type = this.type.BaseType;
                    while (type != typeof(object))
                    {
                        n++;
                    }
                    return n;
                }
            }
            public async Task LinkParentsAsync()
            {
                await Task.Run(() => LinkParents());
            }
            public void LinkParents()
            {
                if (type.BaseType == typeof(object))
                {
                    parentList.roots.Add(this);
                    return;
                }

                if (type.BaseType is null)
                {
                    Debug.Log($"TYPE:{type.Name} IS NULL");
                    return;
                }

                if (parentList.TryGetType(type.BaseType, out var other))
                {
                    other.Children.Add(this);
                }
            }
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