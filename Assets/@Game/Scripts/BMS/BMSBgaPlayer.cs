using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using B83.Image.BMP;
using Sirenix.OdinInspector;
using SanyoniLib.UnityEngineHelper;
using Sirenix.Utilities;

namespace SanyoniBMS
{

    [System.Serializable]
    public enum BgaClipType
    {
        None = -1,
        Image, Video
    }


    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSBgaPlayer : MonoBehaviour
    {
        private const string BmpExtensionText = "bmp";
        private const string JpgExtensionText = "jpg";
        private const string PngExtensionText = "png";

        private const string MpgExtensionText = "mpg";
        private const string AviExtensionText = "avi";

        #region Private Variables

        private Dictionary<int, Texture2D> m_TextureDict;
        private Dictionary<int, Sprite> m_SpriteDict;
        private Dictionary<int, string> m_VideoUrlDict;

        [SerializeField] private VideoManager m_VideoManager;
        [SerializeField] private RenderTexture m_RenderTexture;

        #endregion

        #region Properties

        // 이미지 로딩 관련
        public bool IsPrepared { get; private set; }
        public bool IsLoading { get; private set; }
        public int AllArtCount { get; private set; }
        public int LoadedArtCount { get; private set; }

        #endregion


        public void Initialize()
        {
            this.m_TextureDict = new Dictionary<int, Texture2D>();
            this.m_SpriteDict = new Dictionary<int, Sprite>();
            this.m_VideoUrlDict = new Dictionary<int, string>();

            this.m_VideoManager = GameObject.FindObjectOfType<VideoManager>();

            this.IsLoading = false;
            this.IsPrepared = false;
        }

        public void Play(Sprite spriteClip)
        {
            if (spriteClip == null) return;

            PlayGameScene.Instance.m_BgaImage.enabled = true;
            PlayGameScene.Instance.m_BgaImage.sprite = spriteClip;
            PlayGameScene.Instance.m_BgaImage.color = Color.white;
        }

        public IEnumerator CPlayVideo(string fileFullPath)
        {
            if (File.Exists(fileFullPath) == false) yield break;

            PlayGameScene.Instance.m_BgaRawImage.enabled = true;
            PlayGameScene.Instance.m_BgaRawImage.texture = this.m_RenderTexture;
            PlayGameScene.Instance.m_BgaRawImage.color = Color.white;


            yield return StartCoroutine(this.m_VideoManager.CPrepareVideoUrl(fileFullPath));
            this.m_VideoManager.PlayVideo(false);

        }

        public bool TryPlayInDictionary(int key)
        {
            if (this.m_TextureDict.ContainsKey(key))
            {
                Sprite clip = this.m_SpriteDict[key];
                if (clip != null)
                {
                    Play(clip);
                    return true;
                }
                else
                {
                    if (DebugSettings.DebugMode) Debug.Log("clip is not exist.");
                    return false;
                }
            }
            else if (this.m_VideoUrlDict.ContainsKey(key))
            {
                StartCoroutine(CPlayVideo(this.m_VideoUrlDict[key]));
                return true;
            }
            else return false;
        }


        public void Prepare(string bmsDir, BMSPatternData pattern)
        {
            StartCoroutine(CLoadBMSBgas(bmsDir, pattern));
        }

        private IEnumerator CLoadBMSBgas(string bmsDir, BMSPatternData pattern)
        {
            this.IsLoading = true;
            this.IsPrepared = false;

            int[] keys = new List<int>(pattern.Header.BmpDict.Keys).ToArray();
            this.AllArtCount = keys.Length;

            for (int i = 0; i < keys.Length; i++)
            {
                string fileName = pattern.Header.BmpDict[keys[i]];
                yield return StartCoroutine(CLoadBMSBga(keys[i], bmsDir, fileName));

                this.LoadedArtCount = i + 1;
            }

            this.IsLoading = false;
            this.IsPrepared = true;

        }

        private IEnumerator CLoadBMSBga(int key, string dir, string fileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            Texture2D texture = null;
            UnityWebRequest www = null;
            string extensionText;

            try
            {
                if (File.Exists(Path.Combine(dir, fileNameWithoutExtension) + "." + BmpExtensionText)) extensionText = BmpExtensionText;
                else if (File.Exists(Path.Combine(dir, fileNameWithoutExtension) + "." + JpgExtensionText)) extensionText = JpgExtensionText;
                else if (File.Exists(Path.Combine(dir, fileNameWithoutExtension) + "." + PngExtensionText)) extensionText = PngExtensionText;
                else if (File.Exists(Path.Combine(dir, fileNameWithoutExtension) + "." + MpgExtensionText)) extensionText = MpgExtensionText;
                else if (File.Exists(Path.Combine(dir, fileNameWithoutExtension) + "." + AviExtensionText)) extensionText = AviExtensionText;
                else throw new FileNotFoundException();
            }
            catch (FileNotFoundException e)
            {
                SanyoniLib.UnityEngineHelper.DebugHelper.LogErrorFormat(e, "파일이 없거나 지원하지 않는 파일입니다. bmp / jpg / png/ mpg / avi만 지원하고 있습니다. {0}", Path.Combine(dir, fileName));
                yield break;
            }

            string fileNameWithExtension = fileNameWithoutExtension + "." + extensionText;
            string fileFullPath = Path.Combine(dir, fileNameWithExtension);
            //string uri = "file://" + Path.Combine(dir, UnityWebRequest.EscapeURL(fileNameWithExtension));
            string uri = "file://" + fileFullPath;

            // bmp는 Unity에서 기본지원하지 않기에 (외부)BMPLoader를 거친다.
            if (extensionText == BmpExtensionText)
            {
                www = UnityWebRequest.Get(fileFullPath);
                yield return www.SendWebRequest();

                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP(www.downloadHandler.data);
                texture = img.ToTexture2D();

                this.m_TextureDict.Add(key, texture);
                this.m_SpriteDict.Add(key, ResourcesHelper.CreateSpriteWithTexture2D(texture));
            }
            // jpg와 png는 유니티에서 기본 지원한다. 유니티 함수를 사용해서 로드한다.
            else if (extensionText == JpgExtensionText || extensionText == PngExtensionText)
            {
                www = UnityWebRequestTexture.GetTexture(fileFullPath);
                yield return www.SendWebRequest();
                texture = (www.downloadHandler as DownloadHandlerTexture).texture;

                this.m_TextureDict.Add(key, texture);
                this.m_SpriteDict.Add(key, ResourcesHelper.CreateSpriteWithTexture2D(texture));
            }
            // video 파일은 fileFullPath을 저장한다.
            else if (extensionText == MpgExtensionText || extensionText == AviExtensionText)
            {
                this.m_VideoUrlDict.Add(key, fileFullPath);
            }

        }

    }

}