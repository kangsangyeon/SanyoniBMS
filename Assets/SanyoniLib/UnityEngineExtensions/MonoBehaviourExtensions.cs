using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoniLib.UnityEngineExtensions
{

    public static class MonoBehaviourExtensions
    {

        public static GameObject InstantiateInSameScene(this UnityEngine.Object obj, GameObject prefab)
        {
            GameObject newObject = GameObject.Instantiate(prefab);
            SceneManager.MoveGameObjectToScene(newObject, ((GameObject)obj).scene);
            return newObject;
        }

        //public static AsyncOperation UnloadSceneAsync()
        //{

        //    SceneManager.UnloadSceneAsync();
        //}

        public static void Invoke(this MonoBehaviour me, System.Action action, float time)
        {
            me.StartCoroutine(CInvoke(action, time));
        }

        private static IEnumerator CInvoke(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action();
        }

        //public static void 

    }

}