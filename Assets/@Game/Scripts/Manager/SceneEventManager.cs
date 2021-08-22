using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using SanyoniLib.UnityEngineHelper;

namespace SanyoniBMS
{
    public class SceneEventManager : MonoBehaviour
    {

        private Queue<System.Action> m_OnEndOverlayAnimationCallbackQueue = new Queue<System.Action>();

        #region Public Methods

        /// <summary>
        /// Nody에서 Overlay애니메이션이 끝났을 때 반드시 이 함수를 호출하도록 해야한다.
        /// 지금으로서는 Overlay애니메이션이 끝났을 때 EndOverlayAnimation이벤트를 날리고,
        /// 그 이벤트를 수신해서 이 함수를 호출하는 게임오브젝트인 OnEndOverlayAnimation객체를 Persistent씬 안에 만들어두었다.
        /// </summary>
        public void OnEndOverlayAnimation()
        {
            //if (DebugSettings.DebugMode) Debug.Log("SceneEventManager.On<color=#ff0000>EndOverlayAnimation</color>()");

            // 큐 안의 작업을 모두 실행한다.
            while (m_OnEndOverlayAnimationCallbackQueue.Count != 0)
            {
                //Debug.Log($"\t\tCall {m_OnEndOverlayAnimationCallbackQueue.Peek().Method}");
                m_OnEndOverlayAnimationCallbackQueue.Dequeue().Invoke();
            }

        }

        public void OnStartSelectMusicScene()
        {
            if (DebugSettings.DebugMode) Debug.Log($"SceneEventManager.OnStart<color=#ff0000>SelectMusic</color>Scene(): {Global.SelectMusicSceneText}");


            Doozy.Engine.GameEventMessage.SendEvent(Global.ShowOverlayEventText);
            m_OnEndOverlayAnimationCallbackQueue.Enqueue(() =>
            {
                CloseAllLoadedScenesExceptPersistent();

                var operation = SceneManager.LoadSceneAsync(Global.SelectMusicSceneText, LoadSceneMode.Additive);
                operation.completed += _ =>
                {
                    ApplicationHelper.ManualGarbageCollect();
                    Doozy.Engine.GameEventMessage.SendEvent(Global.HideOverlayEventText);
                };

            });

        }

        public void OnStartLoadMusicScene()
        {
            if (DebugSettings.DebugMode) Debug.Log("SceneEventManager.OnStart<color=#ff0000>LoadMusic</color>Scene()");


            Doozy.Engine.GameEventMessage.SendEvent(Global.ShowOverlayEventText);
            m_OnEndOverlayAnimationCallbackQueue.Enqueue(() =>
            {
                CloseAllLoadedScenesExceptPersistent();

                var operation = SceneManager.LoadSceneAsync(Global.LoadMusicSceneText, LoadSceneMode.Additive);
                operation.completed += _ =>
                {
                    ApplicationHelper.ManualGarbageCollect();
                    Doozy.Engine.GameEventMessage.SendEvent(Global.HideOverlayEventText);
                };

            });

        }

        public void OnStartResultScene()
        {
            if (DebugSettings.DebugMode) Debug.Log("SceneEventManager.OnStart<color=#ff0000>Result</color>Scene()");


            Doozy.Engine.GameEventMessage.SendEvent(Global.ShowOverlayEventText);
            m_OnEndOverlayAnimationCallbackQueue.Enqueue(() =>
            {
                CloseAllLoadedScenesExceptPersistent();

                var operation = SceneManager.LoadSceneAsync(Global.ResultSceneText, LoadSceneMode.Additive);
                operation.completed += _ =>
                {
                    ApplicationHelper.ManualGarbageCollect();
                    Doozy.Engine.GameEventMessage.SendEvent(Global.HideOverlayEventText);
                };

            });
        }

        #endregion

        #region Private Methods

        private void CloseAllLoadedScenesExceptPersistent()
        {
            //if (DebugSettings.DebugMode) Debug.Log("SceneEventManager.<color=#ff0000>CloseAllLoadedScenesExceptPersistent</color>()");

            // Persistent Scene을 제외한 열려있는 모든 씬들의 목록을 가져온다.
            var scenes = Enumerable.Range(0, SceneManager.sceneCount)
               .Select(x => SceneManager.GetSceneAt(x))
               .Where(x => x.name != Global.PersistentSceneText)
               .ToList();

            foreach (var item in scenes)
            {
                // 씬을 언로드하기 이전에 모든 게임오브젝트들을 파괴해야만 한다.
                foreach (var go in item.GetRootGameObjects()) Destroy(go);
                SceneManager.UnloadSceneAsync(item);
            }
        }

    }

    #endregion

}