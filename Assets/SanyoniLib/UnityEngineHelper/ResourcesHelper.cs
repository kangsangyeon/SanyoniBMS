using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using B83.Image.BMP;
using NAudio;
using NAudio.Wave;

namespace SanyoniLib.UnityEngineHelper
{

    public static class ResourcesHelper
    {

        #region Image Method

        private const string BmpExtensionText = ".bmp";
        private const string JpgExtensionText = ".jpg";
        private const string PngExtensionText = ".png";

        // 이미지 로드
        //TODO: 텍스쳐를 불러오는 것과 스프라이트를 불러오는 것은 다르다. 텍스쳐->스프라이트 한 번에 가져올 수 있도록
        //메소드를 나누자.

        public static Sprite CreateSpriteWithTexture2D(Texture2D texture)
        {
            return CreateSpriteWithTexture2D(texture, 0, 0, texture.width, texture.height, new Vector2(.5f, .5f));
        }

        public static Sprite CreateSpriteWithTexture2D(Texture2D texture, int x, int y, int width, int height, Vector2 pivot)
        {
            Rect textureRect = new Rect(x, y, width, height);
            Sprite newSprite = Sprite.Create(texture, textureRect, pivot);

            return newSprite;
        }

        public static void LoadSprite(string dir, string fileName, System.Action<Sprite> callback)
        {
            GlobalInstance.Instance.StartCoroutine(CLoadSprite(dir, fileName, callback));
        }

        public static IEnumerator CLoadSprite(string dir, string fileName, System.Action<Sprite> callback)
        {
            yield return GlobalInstance.Instance.StartCoroutine(CLoadTexture(dir, fileName, texture =>
            {
                callback?.Invoke(CreateSpriteWithTexture2D(texture));
            }));
        }

        public static void LoadTextureIfExist(string dir, string fileName, System.Action<Texture2D> callback)
        {
            string realFileName = SanyoniLib.SystemHelper.PathHelper.GuessRealImageFileName(dir, fileName);
            // 실제 파일이 있을 때에만 로드한다.
            if (realFileName != null)
            {
                ResourcesHelper.LoadTexture(dir, realFileName, callback);
            }
            else
            {
                Debug.LogError($@"파일을 불러올 수 없습니다. {dir}/{realFileName}");
            }

        }

        public static void LoadTexture(string dir, string fileName, System.Action<Texture2D> callback)
        {
            GlobalInstance.Instance.StartCoroutine(CLoadTexture(dir, fileName, callback));
        }

        // 이미지 비동기 로드
        public static IEnumerator CLoadTexture(string dir, string fileName, System.Action<Texture2D> callback)
        {
            string realFileName = SanyoniLib.SystemHelper.PathHelper.GuessRealImageFileName(dir, fileName);
            string extensionText = Path.GetExtension(realFileName);

            string realFileFullPath = Path.Combine(dir, UnityWebRequest.EscapeURL(realFileName));

            Texture2D texture = null;
            UnityWebRequest www = null;

            // bmp는 Unity에서 기본지원하지 않기에 (외부)BMPLoader를 거친다.
            if (extensionText == BmpExtensionText)
            {
                www = UnityWebRequest.Get(realFileFullPath);
                yield return www.SendWebRequest();

                BMPLoader loader = new BMPLoader();
                BMPImage img = loader.LoadBMP(www.downloadHandler.data);
                texture = img.ToTexture2D();
            }
            // jpg와 png는 유니티에서 기본 지원한다. 유니티 함수를 사용해서 로드한다.
            else if (extensionText == JpgExtensionText || extensionText == PngExtensionText)
            {
                www = UnityWebRequestTexture.GetTexture(realFileFullPath);
                yield return www.SendWebRequest();
                texture = (www.downloadHandler as DownloadHandlerTexture).texture;
            }

            callback.Invoke(texture);
        }

        #endregion


        #region Audio Methods

        private const string OggVorbisExtensionText = ".ogg";
        private const string WavExtensionText = ".wav";
        private const string Mp3ExtensionText = ".mp3";

        public static void LoadAudioClip(string dir, string fileName, Action<AudioClip> callback)
        {
            GlobalInstance.Instance.StartCoroutine(CLoadAudioClip(dir, fileName, callback));
        }

        public static IEnumerator CLoadAudioClip(string dir, string fileName, Action<AudioClip> callback)
        {
            AudioClip clip = null;

            string extensionText = Path.GetExtension(fileName);

            string uri = new System.Uri(Path.Combine(dir, fileName)).AbsoluteUri;

            AudioType type = extensionText == OggVorbisExtensionText ? AudioType.OGGVORBIS
                : extensionText == WavExtensionText ? AudioType.WAV
                : extensionText == Mp3ExtensionText ? AudioType.MPEG
                : AudioType.UNKNOWN;

            // ogg와 wav는 유니티에서 기본지원하는 함수를 사용한다.
            if (type == AudioType.OGGVORBIS || type == AudioType.WAV)
            {
                //TODO: UnityWebRequest에서 +문자를 아예 공백으로 인식해서 파일명에 +이 있는 파일을 로드하지 못한다..
                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, type);
                yield return www.SendWebRequest();

                if (www.downloadHandler.data.Length != 0)
                {
                    clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.LoadAudioData();
                }
                else
                {
                    Debug.LogError($"Failed to read sound data : {www.url}");
                    Debug.LogError($"\turi: {uri},\tfileName: {fileName}");
                    //throw new System.Exception();
                }
            }
            // mpeg는 라이센스 문제로 유니티에서 기본제공하지 않기 때문에 NAudio를 사용한다.
            else if (type == AudioType.MPEG)
            {
                WWW www = new WWW(uri);

                yield return new WaitUntil(() => www.isDone == true || www.error != null);

                if (www.bytes.Length != 0)
                {
                    clip = ResourcesHelper.FromMp3Data(www.bytes);
                    clip.LoadAudioData();
                }
                else
                {
                    Debug.LogError($"Failed to read sound data : {www.url}");
                    Debug.LogError($"\t{uri}");
                    //throw new System.Exception();
                }
            }
            else
            {
                Debug.LogError("파일이 없거나 지원하지 않는 파일입니다. ogg / wav / mp3만 지원하고 있습니다.");
                Debug.LogError($"\t{uri}");
                yield break;
            }

            callback?.Invoke(clip);
        }

        public static AudioClip FromMp3Data(byte[] data)
        {
            // Load the data into a stream
            MemoryStream mp3stream = new MemoryStream(data);
            // Convert the data in the stream to WAV format
            Mp3FileReader mp3audio = new Mp3FileReader(mp3stream);
            WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(mp3audio);
            // Convert to WAV data
            WAV wav = new WAV(AudioMemStream(waveStream).ToArray());
            AudioClip audioClip = AudioClip.Create("loaded mp3", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            // Return the clip
            return audioClip;
        }

        private static MemoryStream AudioMemStream(WaveStream waveStream)
        {
            MemoryStream outputStream = new MemoryStream();
            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputStream, waveStream.WaveFormat))
            {
                byte[] bytes = new byte[waveStream.Length];
                waveStream.Position = 0;
                waveStream.Read(bytes, 0, Convert.ToInt32(waveStream.Length));
                waveFileWriter.Write(bytes, 0, bytes.Length);
                waveFileWriter.Flush();
            }
            return outputStream;
        }


        /* From http://answers.unity3d.com/questions/737002/wav-byte-to-audioclip.html */
        public class WAV
        {

            // convert two bytes to one float in the range -1 to 1
            static float bytesToFloat(byte firstByte, byte secondByte)
            {
                // convert two bytes to one short (little endian)
                short s = (short)((secondByte << 8) | firstByte);
                // convert to range from -1 to (just below) 1
                return s / 32768.0F;
            }

            static int bytesToInt(byte[] bytes, int offset = 0)
            {
                int value = 0;
                for (int i = 0; i < 4; i++)
                {
                    value |= ((int)bytes[offset + i]) << (i * 8);
                }
                return value;
            }
            // properties
            public float[] LeftChannel { get; internal set; }
            public float[] RightChannel { get; internal set; }
            public int ChannelCount { get; internal set; }
            public int SampleCount { get; internal set; }
            public int Frequency { get; internal set; }

            public WAV(byte[] wav)
            {

                // Determine if mono or stereo
                ChannelCount = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

                // Get the frequency
                Frequency = bytesToInt(wav, 24);

                // Get past all the other sub chunks to get to the data subchunk:
                int pos = 12;   // First Subchunk ID from 12 to 16

                // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
                while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
                {
                    pos += 4;
                    int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                    pos += 4 + chunkSize;
                }
                pos += 8;

                // Pos is now positioned to start of actual sound data.
                SampleCount = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
                if (ChannelCount == 2) SampleCount /= 2;        // 4 bytes per sample (16 bit stereo)

                // Allocate memory (right will be null if only mono sound)
                LeftChannel = new float[SampleCount];
                if (ChannelCount == 2) RightChannel = new float[SampleCount];
                else RightChannel = null;

                // Write to double array/s:
                int i = 0;
                while (pos < wav.Length)
                {
                    LeftChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                    pos += 2;
                    if (ChannelCount == 2)
                    {
                        RightChannel[i] = bytesToFloat(wav[pos], wav[pos + 1]);
                        pos += 2;
                    }
                    i++;
                }
            }

            public override string ToString()
            {
                return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", LeftChannel, RightChannel, ChannelCount, SampleCount, Frequency);
            }
        }

        #endregion

    }

}