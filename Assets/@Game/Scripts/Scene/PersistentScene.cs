using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoniBMS
{

    public class PersistentScene : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Global.PersistentSceneFlag = true;

            // Doozy 그래프가 준비되기 위해서 조금의 여유시간이 필요하다.
            yield return new WaitForSeconds(1);

            ////TODO: 왜 그런진 모르겠지만 한 번 불러왔던 씬들만 sceneCountInBuildSettings에 포함되는 것 같다.
            //// 씬 빌드인덱스가 PersistentScene을 제외한 가장 앞 씬을 불러오도록 한다.
            //for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            //{
            //    Scene scene = SceneManager.GetSceneByBuildIndex(i);

            //    if (scene == SceneManager.GetSceneByName(Global.PersistentSceneText)) continue;
            //    else if (scene.buildIndex >= 0 && scene.buildIndex < SceneManager.sceneCountInBuildSettings)
            //    {
            //        SceneManager.LoadSceneAsync(scene.buildIndex, LoadSceneMode.Additive);
            //        break;
            //    }
            //}


            Global.LoadAllBMSDatas();

            Doozy.Engine.GameEventMessage.SendEvent(Global.StartSelectMusicSceneEventText);

        }

    }

}