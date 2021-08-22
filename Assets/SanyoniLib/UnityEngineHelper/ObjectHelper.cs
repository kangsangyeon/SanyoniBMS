using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniLib.UnityEngineHelper
{

    public static class ObjectHelper
    {

        public static T FindUniqueObjectOfType<T>() where T : UnityEngine.Object
        {
            T[] objs = UnityEngine.Object.FindObjectsOfType<T>();
            if (objs == null || objs.Length == 0)
            {
                // 인스턴스 없음 오류
                Debug.LogFormat("씬에 로드되어있는 해당 타입에 대한 인스턴스가 없습니다.");
                return null;
            }
            if (objs.Length > 1)
            {
                // 여러개 인스턴스 있음 오류
                Debug.LogFormat("여러개의 인스턴스가 씬에 로드되어 있습니다.");
                return null;
            }

            return objs[0];
        }

    }

}