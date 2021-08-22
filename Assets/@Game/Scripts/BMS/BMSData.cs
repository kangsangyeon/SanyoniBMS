using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;


namespace SanyoniBMS
{

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSData
    {
        public string Directory;
        public int ID { get { return Directory.GetHashCode(); } }

        public string Title;
        public string Artist;
        public double BPM;
        /// <summary>
        /// 악곡의 패턴들을 담고 있는 배열입니다. 패턴들은 이미 정렬이 된 상태입니다. 
        /// serialize와 deserialize시에 이 배열의 내용은 제외됩니다.
        /// </summary>
        public BMSPatternData[] BMSPatternDatas;

        public BMSData() { }
        public BMSData(string _directory, string _title, string _artist, double _bpm, BMSPatternData[] _patterns)
        {
            this.Directory = _directory;
            this.Title = _title;
            this.Artist = _artist;
            this.BPM = _bpm;
            this.BMSPatternDatas = _patterns;
        }

        public override string ToString()
        {
            return string.Format("{{Title: {0}\tArtist: {1}\tBPM: {2}\tPatternCount: {3}\tDirectory: {4}}}", this.Title, this.Artist, this.BPM, this.BMSPatternDatas.Length, this.Directory);
        }

    }

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSPatternData : System.IComparable<BMSPatternData>, ISerializable
    {
        /// <summary>
        /// 패턴 파일(bms/bme/bml/...)이 위치한 디렉토리를 절대경로로 저장한 문자열입니다. 파일명은 포함하지 않습니다.
        /// </summary>
        public string Directory;
        public string FileName;
        /// <summary>
        /// 패턴 파일이 위치한 경로입니다. 디렉토리와 파일명을 모두 포함합니다. 
        /// 이 객체의 멤버변수 Directory 또는 FileName이 하나라도 null이라면 이 프로퍼티는 string.Empty를 반환합니다.
        /// </summary>
        public string FilePath
        {
            get
            {
                if (this.Directory != null && this.FileName != null) return System.IO.Path.Combine(Directory, FileName);
                else return string.Empty;
            }
        }
        public int ID { get { return FilePath.GetHashCode(); } }

        public string PatternTitle;
        public KeyMode KeyType;
        public bool ExistScratch;
        public BMSHeaderData Header;
        public BMSMainData MainData;

        /// <summary>
        /// Header가 파싱되어있음을 판별하는 변수.
        /// </summary>
        public bool IsHeaderParsed;
        /// <summary>
        /// Header의 Indexing Attribute들이 파싱되어있음을 판별하는 변수.
        /// </summary>
        public bool IsHeaderIndexingAttributeParsed;
        /// <summary>
        /// MainData가 파싱되어있음을 판별하는 변수.
        /// </summary>
        public bool IsMainDataParsed;

        public BMSPatternData() { }
        public BMSPatternData(string _fileName, string _patternTitle)
        {
            this.FileName = _fileName;
            this.PatternTitle = _patternTitle;
        }
        public BMSPatternData(string _fileName, string _patternTitle, KeyMode _keyType, BMSHeaderData _header, BMSMainData _mainData) : this(_fileName, _patternTitle)
        {
            this.KeyType = _keyType;
            this.Header = _header;
            this.MainData = _mainData;
        }

        // PlayLevel > MainData.NoteList.Count 로 정렬한다.
        public int CompareTo(BMSPatternData other)
        {
            if (this.Header.PlayLevel > other.Header.PlayLevel) return 1;
            else if (this.Header.PlayLevel < other.Header.PlayLevel) return -1;

            // MainData가 파싱되어 있을 때에만 NoteList.Count 비교를 실시한다.
            else if (this.IsMainDataParsed == true && this.MainData.NoteList != null && other.MainData.NoteList != null)
            {
                if (this.MainData.NoteList.Count > other.MainData.NoteList.Count) return 1;
                else if (this.MainData.NoteList.Count < other.MainData.NoteList.Count) return -1;
            }

            // 그 외에는 전부 동일 우선순위로 간주한다.
            return 0;
        }

        public override string ToString()
        {
            return string.Format("{{PatternTitle: {0}\tFilename: {1}}}", this.PatternTitle, this.FileName);
        }

        public override int GetHashCode()
        {
            return $"{this.ToString() + this.Header.ToString() + this.MainData.ToString()}".GetHashCode();
        }

        #region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FileName", this.FileName, typeof(string));
            info.AddValue("PatternTitle", this.PatternTitle, typeof(string));
            info.AddValue("KeyType", this.KeyType, typeof(KeyMode));
            info.AddValue("ExistScratch", this.ExistScratch, typeof(bool));
            info.AddValue("Header", this.Header, typeof(BMSHeaderData));
            info.AddValue("MainData", this.Header, typeof(BMSMainData));
        }

        public BMSPatternData(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            this.FileName = (string)info.GetValue("FileName", typeof(string));
            this.PatternTitle = (string)info.GetValue("PatternTitle", typeof(string));
            this.KeyType = (KeyMode)info.GetValue("KeyType", typeof(KeyMode));
            this.ExistScratch = (bool)info.GetValue("ExistScratch", typeof(bool));
            this.Header = (BMSHeaderData)info.GetValue("Header", typeof(BMSHeaderData));
            this.MainData = (BMSMainData)info.GetValue("MainData", typeof(BMSMainData));
        }

        #endregion

    }


    /// <summary>
    /// BMS파일의 Header Field에 위치한 메타데이터를 저장하는 인스턴스 클래스.
    /// </summary>
    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSHeaderData : ISerializable
    {
        public string Title;
        public string Artist;
        public string SubArtist;
        public double BPM;
        public string Genre;
        public int Player;
        public int PlayLevel;
        public int LNType;
        public string StageFile;
        public Dictionary<int, string> WavDict = new Dictionary<int, string>();
        public Dictionary<int, string> BmpDict = new Dictionary<int, string>();
        public Dictionary<int, int> StopDict = new Dictionary<int, int>();
        public Dictionary<int, double> BpmDict = new Dictionary<int, double>();
        public List<int> LNOBJList = new List<int>();

        public bool HasVideoBga;

        public BMSHeaderData() { }

        // 비주류 옵션들
        public int Rank;

        public override string ToString()
        {
            return $"{{Title: {this.Title}\tArtist: {this.Artist}\tSubArtist: {this.SubArtist}" +
                $"\tBPM: {this.BPM}\tGenre: {this.Genre}\tPlayer: {this.Player}\tPlayLevel: {this.PlayLevel}\tLNType: {this.LNType}}}";
        }

        #region Serialization
        private const string TitleText = "Title";
        private const string ArtistText = "Artist";
        private const string SubArtistText = "SubArtist";
        private const string BPMText = "BPM";
        private const string GenreText = "Genre";
        private const string PlayerText = "Player";
        private const string PlayLevelText = "PlayLevel";
        private const string LNTypeText = "LNType";
        private const string StageFileText = "StageFile";
        private const string WavDictText = "WavDict";
        private const string BmpDictText = "BmpDict";
        private const string StopDictText = "StopDict";
        private const string BpmDictText = "BpmDict";
        private const string LNOBJListText = "LNOBJList";

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(TitleText, this.Title, typeof(string));
            info.AddValue(ArtistText, this.Artist, typeof(string));
            info.AddValue(SubArtistText, this.SubArtist, typeof(string));
            info.AddValue(BPMText, this.BPM, typeof(double));
            info.AddValue(GenreText, this.Genre, typeof(string));
            info.AddValue(PlayerText, this.Player, typeof(int));
            info.AddValue(PlayLevelText, this.PlayLevel, typeof(int));
            info.AddValue(LNTypeText, this.LNType, typeof(int));
            info.AddValue(StageFileText, this.StageFile, typeof(string));
            info.AddValue(WavDictText, this.WavDict, typeof(Dictionary<int, string>));
            info.AddValue(BmpDictText, this.BmpDict, typeof(Dictionary<int, string>));
            info.AddValue(StopDictText, this.StopDict, typeof(Dictionary<int, int>));
            info.AddValue(BpmDictText, this.BpmDict, typeof(Dictionary<int, double>));
            info.AddValue(LNOBJListText, this.LNOBJList, typeof(List<int>));
        }

        public BMSHeaderData(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            this.Title = (string)info.GetValue(TitleText, typeof(string));
            this.Artist = (string)info.GetValue(ArtistText, typeof(string));
            this.SubArtist = (string)info.GetValue(SubArtistText, typeof(string));
            this.BPM = (double)info.GetValue(BPMText, typeof(double));
            this.Genre = (string)info.GetValue(GenreText, typeof(string));
            this.Player = (int)info.GetValue(PlayerText, typeof(int));
            this.PlayLevel = (int)info.GetValue(PlayLevelText, typeof(int));
            this.LNType = (int)info.GetValue(LNTypeText, typeof(int));
            this.StageFile = (string)info.GetValue(StageFileText, typeof(string));
            this.WavDict = (Dictionary<int, string>)info.GetValue(WavDictText, typeof(Dictionary<int, string>));
            this.BmpDict = (Dictionary<int, string>)info.GetValue(BmpDictText, typeof(Dictionary<int, string>));
            this.StopDict = (Dictionary<int, int>)info.GetValue(StopDictText, typeof(Dictionary<int, int>));
            this.BpmDict = (Dictionary<int, double>)info.GetValue(BpmDictText, typeof(Dictionary<int, double>));
            this.LNOBJList = (List<int>)info.GetValue(LNOBJListText, typeof(List<int>));
        }

        #endregion

    }


    /// <summary>
    /// BMS파일의 Main Data Field에 위치한, 실질적으로 '채보'라는것에 해당하는 데이터를 저장하는 클래스.
    /// </summary>
    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSMainData
    {
        public List<BMSObject> EventList = new List<BMSObject>();
        public List<Note> NoteList = new List<Note>();
        public List<BarEvent> BarList = new List<BarEvent>();

        public override string ToString()
        { 
            StringBuilder sb = new StringBuilder();
            sb.Append('{');
            {
                sb.AppendFormat("EventCount: {0}\t", this.EventList == null ? 0 : this.EventList.Count);
                sb.AppendFormat("NoteCount: {0}", this.NoteList == null ? 0 : this.NoteList.Count);
            }
            sb.Append('}');

            return sb.ToString();
        }
    }

}