using System;
using UnityEngine;

namespace SanyoniLib.UnityEngineExtensions
{

    public static class TransformExtensions
    {

        //var cube = this.transform.FirstChildOrDefault(x => x.name == "deeply_nested_cube");
        public static Transform FirstChildOrDefault(this Transform parent, Func<Transform, bool> query)
        {
            if (parent.childCount == 0)
            {
                return null;
            }

            Transform result = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (query(child))
                {
                    return child;
                }
                result = FirstChildOrDefault(child, query);
            }

            return result;
        }


        public static Transform FindRecursive(this Transform current, string name)
        {
            //sb.Append(current.name + "\n");

            // check if the current bone is the bone we're looking for, if so return it
            if (current.name == name)
                return current;

            // search through child bones for the bone we're looking for
            for (int i = 0; i < current.childCount; ++i)
            {
                // the recursive step; repeat the search one step deeper in the hierarchy
                var child = current.GetChild(i);

                Transform found = FindRecursive(child, name);

                // a transform was returned by the search above that is not null,
                // it must be the bone we're looking for
                if (found != null)
                    return found;
            }

            // bone with name was not found
            return null;
        }


    }

}