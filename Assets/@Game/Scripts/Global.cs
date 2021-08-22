using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using SanyoniLib.SystemHelper;
using System.Runtime.CompilerServices;
using Sirenix.Utilities;

namespace SanyoniBMS
{

    public static partial class Global
    {
        /***** Scene Texts *****/
        public const string PersistentSceneText = "_Persistent";
        public const string SelectModeSceneText = "Select Mode Scene";
        public const string SelectMusicSceneText = "Select Music Scene";
        public const string LoadMusicSceneText = "Load Music Scene";
        public const string PlayGameSceneText = "Play Game Scene";
        public const string ResultSceneText = "Result Scene";
        /**/

        /***** Event Texts *****/
        /***** 오버레이 애니메이션 *****/
        public const string ShowOverlayEventText = "ShowOverlay";
        public const string HideOverlayEventText = "HideOverlay";
        /**/

        /***** 씬 전환 *****/
        public const string StartSelectMusicSceneEventText = "StartSelectMusicScene";
        public const string StartLoadMusicSceneEventText = "StartLoadMusicScene";
        public const string StartPlayGameSceneEventText = "StartPlayGameScene";
        public const string StartResultSceneEventText = "StartResultScene";
        /**/

        /***** 기타 *****/
        public const string RetryBMSEventText = "Retry BMS";
        /**/

        /***** Doozy *****/
        /***** View Category *****/
        public const string LoadMusicSceneCategoryName = "SanyoniBMS - LoadMusicScene";
        public const string PlayGameSceneCategoryName = "SanyoniBMS - PlayGameScene";
        public const string SelectModeSceneCategoryName = "SanyoniBMS - SelectModeScene";
        public const string SelectMusicSceneCategoryName = "SanyoniBMS - SelectMusicScene";
        /**/

        /***** View *****/
        public const string PlayGameScenePauseViewName = "Pause";
        public const string PlayGameSceneClearViewName = "Clear";
        public const string PlayGameSceneFullComboViewName = "FullCombo";
        public const string PlayGameSceneBgaViewName = "Bga";

        /**/

        /***** Directories and Files *****/
        // Extension
        private const string SanyoniBMSDataFileExtensionText = ".sbd";
        private const string SanyoniBMSPlayerDataFileExtensionText = ".sbd-player";

        // File
        private const string GlobalSettingsJsonFileName = "Settings.json";

        // Path
        private static readonly string GlobalSettingsJsonFilePath = Path.Combine(Application.persistentDataPath, Global.GlobalSettingsJsonFileName);

        /**/

        /***** Parsing *****/
        private const string JsonKeySettingsDictKeyText = "KeySettingsDict";
        private const string JsonVideoSettingsDataKeyText = "VideoSettingsData";
        private const string JsonPlayerDataKeyText = "VideoSettingsData";
        private static readonly JsonSerializerSettings JsonSerializerSettingsConst = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        /**/


        /***** 가변 정적 멤버 *****/
        public static bool PersistentSceneFlag = false;
        public static bool TestSceneModeFlag = false;

        public static PlayerData Player;
        public static string[] BMSRootDirectories = new string[] { @"D:\BMSFiles" };
        public static Dictionary<string, List<BMSData>> BMSDataListByDictionary;


        public static Dictionary<KeyMode, BMSKeySettingsBase> KeySettingsDict;
        public static VideoSettingsData VideoSettingsData;


        static Global()
        {
            // AppSettings - 

            LoadSettings();
            LoadDatas();

            //string[] filePaths = Directory.GetFiles(Application.persistentDataPath, $"{SanyoniBMSPlayerDataFileExtensionText}$");
            //filePaths.ForEach(s => Debug.Log(s));
        }

        public static void InitializeSettings()
        {
            KeySettingsDict = new Dictionary<KeyMode, BMSKeySettingsBase>();

            // 초기 설정을 불러온다.
            //TODO: 초기 설정 애셋을 찾아 그것으로 불러오도록 할 것.
            KeySettingsDict.Add(KeyMode.SP5, new BMS5KeySettings());
            KeySettingsDict.Add(KeyMode.SP7, new BMS7KeySettings());

            VideoSettingsData = new VideoSettingsData();
        }

        public static void InitializeDatas()
        {
            Player = new PlayerData();

        }

        public static void LoadSettings()
        {
            string filePath = GlobalSettingsJsonFilePath;
            Debug.LogFormat("Global.LoadSettings(): {0} 설정을 읽어옵니다.", filePath);

            try
            {
                if (File.Exists(filePath) == false) throw new FileNotFoundException();

                string fileText = File.ReadAllText(filePath, TextHelper.GetEncoding(filePath));

                // 이곳에서 json으로 저장된 속성들을 불러옵니다.
                JObject json = JObject.Parse(fileText);
                Global.KeySettingsDict = JsonConvert.DeserializeObject<Dictionary<KeyMode, BMSKeySettingsBase>>
                    (json[JsonKeySettingsDictKeyText].ToString(), Global.JsonSerializerSettingsConst);
                Debug.Log("\tKeySettings Loaded.");

                Global.VideoSettingsData = JsonConvert.DeserializeObject<VideoSettingsData>
                    (json[JsonVideoSettingsDataKeyText].ToString(), Global.JsonSerializerSettingsConst);
                Debug.Log("\tVideoSettings Loaded.");

            }
            catch (FileNotFoundException e)
            {
                // 세이브파일을 찾을 수 없습니다.
                // 기본 설정을 불러옵니다.
                Debug.LogFormat("\t\t{0}을 찾을 수 없습니다. 기존 설정을 불러옵니다.", filePath);

                InitializeSettings();
                SaveSettings();
            }
            catch (System.Exception e)
            {
                // 로드에 실패하면 
                //1. 애플리케이션 초기 기동이라면 기본값들을 불러온다.
                //2. 초기 기동이 아니라면 설정데이터가 불러올 수 없음을 알리고 기본값으로 불러와 덮어쓰는지 묻는다.
                Debug.LogFormat("\t\t{0}을 불러오는 데 실패하였습니다. 기존 설정을 불러옵니다.", filePath);

                InitializeSettings();
                SaveSettings();
            }
        }

        public static void LoadDatas()
        {
            Debug.LogFormat("Global.LoadDatas(): 플레이어 정보를 읽어옵니다.");

            try
            {
                string[] filePaths = Directory.GetFiles(Application.persistentDataPath)
                    .Where(s => s.EndsWith(SanyoniBMSPlayerDataFileExtensionText)).ToArray();
                if (filePaths.Length == 0)
                    throw new FileNotFoundException();

                foreach (var path in filePaths)
                {
                    string fileText = File.ReadAllText(path, TextHelper.GetEncoding(path));

                    // 이곳에서 json으로 저장된 속성들을 불러옵니다.
                    JObject json = JObject.Parse(fileText);
                    Global.Player = JsonConvert.DeserializeObject<PlayerData>
                        (json[JsonPlayerDataKeyText].ToString(), Global.JsonSerializerSettingsConst);
                    Debug.LogFormat("\t{0} 을 읽어왔습니다.", path);

                }

                Debug.Log("\tPlayerData Loaded.");

            }
            catch (FileNotFoundException e)
            {
                // 세이브파일을 찾을 수 없습니다.
                // 기본 플레이어 정보를 불러오고 저장합니다.
                Debug.LogFormat("\t\t플레이어 정보를 찾을 수 없습니다. 기본 플레이어 정보를 불러오고 저장합니다.");

                InitializeDatas();
                SaveDatas();
            }
            catch (System.Exception e)
            {
                Debug.LogFormat("\t\t플레이어 정보를 찾을 수 없습니다. 기본 플레이어 정보를 불러오고 저장합니다.");

                InitializeDatas();
                SaveDatas();
            }

        }

        public static void SaveSettings()
        {
            // 이곳에서 json으로 저장할 속성들을 추가하고, json 객체에 추가합니다
            string keySettingsDictJsonText = JsonConvert.SerializeObject(Global.KeySettingsDict, Global.JsonSerializerSettingsConst);
            string videoSettingsDataText = JsonConvert.SerializeObject(Global.VideoSettingsData, Global.JsonSerializerSettingsConst);

            JObject json = new JObject();
            json.Add(JsonKeySettingsDictKeyText, JObject.Parse(keySettingsDictJsonText));
            json.Add(JsonVideoSettingsDataKeyText, JObject.Parse(videoSettingsDataText));

            string filePath = Path.Combine(Application.persistentDataPath, Global.GlobalSettingsJsonFileName);
            File.WriteAllText(filePath, json.ToString());
            Debug.LogFormat("Global.SaveSettings(): {0}에 저장하였습니다.", filePath);
        }

        public static void SaveDatas()
        {
            string playerDataJsonText = JsonConvert.SerializeObject(Global.Player, Global.JsonSerializerSettingsConst);

            JObject json = new JObject();
            json.Add(JsonPlayerDataKeyText, JObject.Parse(playerDataJsonText));

            string filePath = Path.Combine(Application.persistentDataPath, Global.Player.ID.ToString() + SanyoniBMSPlayerDataFileExtensionText);
            File.WriteAllText(filePath, json.ToString());
            Debug.LogFormat("Global.SaveDatas(): {0}에 저장하였습니다.", filePath);
        }

        public static void LoadAllBMSDatas()
        {
            Global.BMSDataListByDictionary = new Dictionary<string, List<BMSData>>(capacity: Global.BMSRootDirectories.Length);

            foreach (string root in Global.BMSRootDirectories)
            {
                BMSData[] bmsDatas;
                BMSParser.ParseAllBMSBelowRootDirectory(root, out bmsDatas, true, false, false);

                Global.BMSDataListByDictionary.Add(root, bmsDatas.ToList());

                foreach (var item in bmsDatas)
                {
                    BMSDataPool.DataPool.Add(item.ID, item);
                    foreach (var subItem in item.BMSPatternDatas) BMSDataPool.PatternPool.Add(subItem.ID, subItem);
                }

                ///
                //Global.BMSDataListByDictionary.Add(root, new List<BMSData>());

                //foreach (var directory in Directory.GetDirectories(root))
                //{
                //    BMSData data = null;

                //    //Directory.GetFiles(directory, SanyoniBMSDataFileExtensionText, SearchOption.TopDirectoryOnly);
                //    string[] sbdFiles = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
                //                        .Where(s => s.EndsWith(SanyoniBMSDataFileExtensionText)).ToArray();

                //    // BMS 디렉토리 안에 sbd파일이 무조건 1개만 있어야 한다.
                //    // 1개 이상이라면 디렉토리 안에 있는 sbd파일을 그냥 모두 지워버리고 새로 만든다.
                //    if (sbdFiles.Length != 1)
                //    {
                //        foreach (var filePath in sbdFiles) File.Delete(filePath);
                //    }
                //    // 최근 파일 변경보다 최근 디렉토리 변경이 더 최근에 일어났다면 업데이트한다.
                //    else if (Directory.GetLastWriteTime(sbdFiles[0]) < Directory.GetLastWriteTime(directory)) { }
                //    else
                //    {
                //        LoadBMSData(sbdFiles[0], out data);

                //        // 파일 안에 기록된 디렉토리와 현재 디렉토리가 다르다면, 
                //        // 즉 여기 있어서 안될 파일을 로드해왔다면 지우고 다시 만든다.
                //        if (data.Directory != directory)
                //        {
                //            File.Delete(sbdFiles[0]);
                //            BMSParser.ParseBMSData(directory, out data);
                //        }
                //        else
                //        {
                //            Debug.Log($"Global.LoadAllBMSDatas(): 기존 파일을 로드합니다. <color=#ff0000>{directory}</color>");
                //            Global.BMSDataListByDictionary[root].Add(data);
                //            continue;
                //        }

                //    }

                //    // 새로이 bms파일을 파싱하고 sbd파일을 생성한다.
                //    BMSParser.ParseBMSData(directory, out data);
                //    Global.SaveBMSData(data);

                //    Global.BMSDataListByDictionary[root].Add(data);
                //}

            }

        }

        public static void LoadBMSData(string filePath, out BMSData data)
        {
            try
            {
                if (File.Exists(filePath) == false) throw new FileNotFoundException();

                string fileText = File.ReadAllText(filePath, TextHelper.GetEncoding(filePath));
                data = JsonConvert.DeserializeObject<BMSData>(fileText, JsonSerializerSettingsConst);

                return;
            }
            catch (FileNotFoundException e)
            {
                Debug.LogFormat("Global.LoadBMSData(): {0}을 찾을 수 없습니다.", filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogFormat("Global.LoadSettings(): {0}을 불러오는 데 실패하였습니다.", filePath);
            }

            data = null;

        }

        public static void SaveAllBMSDatas()
        {
            if (Global.BMSDataListByDictionary != null)
            {
                foreach (var pair in Global.BMSDataListByDictionary)
                {
                    // BMSData마다 파일로 저장한다.
                    foreach (var data in pair.Value)
                    {
                        string filePath = Path.Combine(data.Directory, PathHelper.ConvertToValidFileName(data.Title) + SanyoniBMSDataFileExtensionText);
                        SaveBMSData(filePath, data);

                        //Debug.LogFormat("Global.SaveBMSDatas(): {0} 저장하였습니다.", filePath);
                    }

                }

            }

        }

        public static void SaveBMSData(BMSData data)
        {
            SaveBMSData(Path.Combine(data.Directory, PathHelper.ConvertToValidFileName(data.Title) + SanyoniBMSDataFileExtensionText), data);
        }

        public static void SaveBMSData(string filePath, BMSData data)
        {
            string dataJsonText = JsonConvert.SerializeObject(data, Global.JsonSerializerSettingsConst);
            JObject json = JObject.Parse(dataJsonText);

            File.WriteAllText(filePath, json.ToString());
        }

    }

}