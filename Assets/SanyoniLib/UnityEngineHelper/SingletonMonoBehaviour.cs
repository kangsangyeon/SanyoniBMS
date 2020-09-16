using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoniLib.UnityEngineHelper
{

    public class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {

        /// <summary>
        /// DontDestroy씬에 올려놓을 것인지 아닐지를 설정하는 변수.
        /// </summary>
        protected bool EnableDontDestroy
        {
            get { return this.m_EnableDontDestroy; }
            set
            {
                // 기존 값과 비교하여 값이 바뀌었을 때만 실행한다.
                if (this.m_EnableDontDestroy == false && value == true)
                {
                    DontDestroyOnLoad(gameObject);
                }
                else if(this.m_EnableDontDestroy == true && value == false)
                {
                    // DontDestroy를 해제하는 것은 간단하다. 
                    // Dont Destroy는 사실, 오브젝트를 "DontDestroyOnLoad"라는 특별한 이름의 씬으로 옮김으로써 파괴되지 않게 하는 것이기 때문이다.
                    // 그렇기 때문에 다시 원래 씬으로 되돌려놓기만 하면 된다.
                    if (this.gameObject.scene != originalScene && originalScene.isLoaded == true)
                        SceneManager.MoveGameObjectToScene(this.gameObject, originalScene);
                    else
                    {
                        Debug.Log("{0}가 속해있던 원래 씬으로 되돌아가려 했으나, 원래 씬이 로드되어있지 않아 실패하였습니다. 현재 활성화되어있는 씬으로 옮깁니다.", this.gameObject);
                        SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
                    }
                }

                this.m_EnableDontDestroy = value;
            }
        }
        private bool m_EnableDontDestroy = false;
        /// <summary>
        /// 중복된 인스턴스를 발견하여 파괴할 때, 게임오브젝트를 파괴할 것인지 컴포넌트만을 파괴할 것인지를 설정하는 변수.
        /// </summary>
        protected bool enableDestroyGameobject = false;
        /// <summary>
        /// 중복된 인스턴스를 발견했을 때, 에러 로그를 출력할 것인지를 설정하는 변수.
        /// </summary>
        protected bool enableLogErrorWhenDuplicated = false;
        /// <summary>
        /// 원래 있던 씬을 저장하는 변수. DontDestroy씬에서 다시 원래 씬으로 되돌려놓을 때 사용하기 위함이다.
        /// </summary>
        protected Scene originalScene;

        private static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();
                // instance가 없을 시에 생성한다는 코드는 없앤다. 
                // 언제 인스턴스가 생길 지 예측하기가 힘들고 관리하기 힘들어서 바꾸었다.
                //if (_instance == null)
                //{
                //    GameObject _newGO = new GameObject(typeof(T).Name);
                //    _instance = _newGO.AddComponent<T>();
                //}

                return _instance;
            }
        }


        private bool detectedMultipleInstance = false;

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) => { detectedMultipleInstance = false; };
                originalScene = this.gameObject.scene;
            }
            else if (_instance != this && detectedMultipleInstance == false)
            {
                detectedMultipleInstance = true;

                T[] findedObjects = FindObjectsOfType<T>();
                string[] attachedGameObjects = new string[findedObjects.Length];
                for (int i = 0; i < attachedGameObjects.Length; i++) attachedGameObjects[i] = findedObjects[i].gameObject.name;

                if (enableLogErrorWhenDuplicated)
                {
                    Debug.LogErrorFormat("Singleton타입의 <color=#ff0000>{0}</color> 인스턴스가 여러 곳에서 생성되었습니다. \n" +
                        "다음 오브젝트들 아래 붙어있는 이 타입의 컴포넌트들 중 가장 첫 번째 인스턴스를 제외한 나머지 인스턴스를 파괴합니다. \n" +
                        "<color=#ff0000>{1}</color>",
                        typeof(T).Name, string.Join(",", attachedGameObjects));
                }

                // 첫 번째 인스턴스를 제외한 나머지 인스턴스를 파괴한다.
                for (int i = 1; i < findedObjects.Length; i++)
                {
                    if (enableDestroyGameobject) Destroy(findedObjects[i].gameObject);
                    else Destroy(findedObjects[i]);
                }

            }

        }

        protected void OnDestroy()
        {
            _instance = null;
        }

    }

}