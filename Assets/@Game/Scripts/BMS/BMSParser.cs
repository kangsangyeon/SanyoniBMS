using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System.Text;
using SanyoniLib.SystemExtensions;
using SanyoniLib.SystemHelper;
using SanyoniLib.UnityEngineHelper;

namespace SanyoniBMS
{

    public static class BMSParser
    {

        public static string BMSRootPath = @"D:\BMSFiles\";
        public static string[] BMSPaths = Directory.GetDirectories(BMSRootPath);


        /* =============== 상수 =============== */
        // 더미노트
        private const string DUMMY_NOTE = "00";


        /* =============== 헤더 속성 이름 =============== */
        private const string PlayerAttributeName = "PLAYER";
        private const string GenreAttributeName = "GENRE";
        private const string TitleAttributeName = "TITLE";
        private const string ArtistAttributeName = "ARTIST";
        private const string BPMAttributeName = "BPM";
        private const string PlayLevelAttributeName = "PLAYLEVEL";
        private const string LNTypeAttributeName = "LNTYPE";
        private const string StageFileAttributeName = "STAGEFILE";

        // 인덱싱 속성들
        private const string WavIndexingAttributeName = "WAV";
        private const string BmpIndexingAttributeName = "BMP";
        private const string StopIndexingAttributeName = "STOP";
        private const string BpmIndexingAttributeName = "BPM";
        private const string LNOBJIndexingAttributeName = "LNOBJ";


        // 비주류 속성들
        private const string RankAttributeName = "RANK";


        // 참고
        // (Window) 허용되는 특수문자: `~!@#$%^&()[];',.{}
        // (Window) 허용되지 않는 특수문자: \ / : * ? " < > | 
        /* =============== 정규식 표현들 =============== */
        private const string CommentTextRegex = @"^\*";
        private const string HeaderFieldTextRegex = @"^\#(?<Attribute>[0-9a-zA-Z]{1,})[\s](?<Value>[\w\W]+)";
        private const string MainDataFieldTextRegex = @"^\#(?<BarIndex>[0-9]{3})(?<Channel1>[0-9]{1})(?<Channel2>[0-9]{1})\:(?<PatternData>[\w.]+)";
        private const string IndexingAttributeRegex = @"^(?<IndexingAttribute>(WAV|BMP|STOP|BPM|LNOBJ)?)(?<Index>[0-9a-zA-Z]{2})$";
        private const string FileNameRegex = @"(?<FileName>[\w\s\`\~\!\@\#\$\%\^\&\(\)\[\]\;\'\,\.\{\}]+)(\.(?<FileExtension>[\w]*))$";

        private const string HeaderFieldTextRegexSubgroupAttributeName = "Attribute";
        private const string HeaderFieldTextRegexSubgroupValueName = "Value";

        private const string MainDataRegexSubgroupBarIndexName = "BarIndex";
        private const string MainDataRegexSubgroupChannel1Name = "Channel1";
        private const string MainDataRegexSubgroupChannel2Name = "Channel2";
        private const string MainDataRegexSubgroupPatternDataName = "PatternData";

        private const string IndexingAttributeRegexSubgroupIndexingAttributeName = "IndexingAttribute";
        private const string IndexingAttributeRegexSubgroupIndexName = "Index";

        private const string FileNameRegexSubgroupFileName = "FileName";
        private const string FileNameRegexSubgroupFileExtension = "FileExtension";



        /* =============== 파싱 메소드 =============== */
        /// <summary>
        /// 특정 폴더안에 있는 모든 악곡들을 파싱합니다.
        /// 각각의 악곡들은 특정 폴더 안의 하위폴더로 개별적인 폴더 안에 존재합니다.
        /// </summary>
        /// <param name="_rootDirectory">파싱할 악곡들이 모여있는 루트 폴더입니다. </param>
        /// <param name="_datas">파싱된 악곡들이 저장될 out BMSData[]입니다.</param>
        /// <param name="_parseHeader">헤더를 파싱할 것인지에 대한 여부입니다. 보통 true를 건네줍니다. </param>
        /// <param name="_parseHeaderIndexingAttribute">헤더 정보 중, 인덱싱이 필요한 속성들을 파싱할 것인지에 대한 여부입니다. 
        /// key와 value값 쌍으로 파싱되어야 하는 값들이 인덱싱 속성들에 해당됩니다.
        /// 예를 들어, wav나 bmp등과 같은 정보가 필요 없는 경우에는 false를 건네주어도 괜찮습니다. 
        /// 하지만 보통의 경우에는 true를 건네줍니다. </param>
        /// <param name="_parseMainData">메인 데이터를 파싱할 것인지에 대한 여부입니다. 
        /// 채보 정보 전부를 파싱해야 한다면 true를 건네주어야 합니다. 
        /// 하지만 채보 정보가 당장에 필요하지 않을 때 false를 건네줌으로서 성능이 향상된 파싱을 기대할 수 있습니다. </param>
        /// <returns>파싱 성공 여부입니다. </returns>
        public static bool ParseAllBMSBelowRootDirectory(string _rootDirectory, out BMSData[] _datas, bool _parseHeader = true, bool _parseHeaderIndexingAttribute = false, bool _parseMainData = false)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(_rootDirectory);

                List<BMSData> bmsDataList = new List<BMSData>();
                foreach (string dir in dirs)
                {
                    BMSData bmsData;
                    bool isSuccess = ParseBMSData(dir, out bmsData, _parseHeader, _parseHeaderIndexingAttribute, _parseMainData);

                    if (isSuccess) bmsDataList.Add(bmsData);

                }

                if (bmsDataList.Count == 0)
                {
                    Debug.LogWarningFormat("{0}에서 불러온 bms가 전혀 없습니다.", _rootDirectory);
                    throw new System.Exception();
                }

                _datas = bmsDataList.ToArray();
                return true;

            }
            catch (System.Exception e)
            {
                DebugHelper.LogError(e);
                _datas = null;
                return false;
            }

        }

        /// <summary>
        /// 단일 악곡에 대해 파싱합니다.
        /// 단일 악곡은 여러 개의 패턴을 가질 수 있고, 파싱된 악곡 정보는 BMSData로 리턴됩니다.
        /// </summary>
        /// <param name="_bmsDir">파싱할 악곡이 들어있는 디렉토리입니다.</param>
        /// <param name="_data">파싱된 악곡 정보가 리턴될 out 변수입니다.</param>
        /// <param name="_parseHeader">헤더를 파싱할 것인지에 대한 여부입니다. 보통 true를 건네줍니다.</param>
        /// <param name="_parseHeaderIndexingAttribute">헤더 정보 중, 인덱싱이 필요한 속성들을 파싱할 것인지에 대한 여부입니다. 
        /// key와 value값 쌍으로 파싱되어야 하는 값들이 인덱싱 속성들에 해당됩니다.
        /// 예를 들어, wav나 bmp등과 같은 정보가 필요 없는 경우에는 false를 건네주어도 괜찮습니다. 
        /// 하지만 보통의 경우에는 true를 건네줍니다.</param>
        /// <param name="_parseMainData">메인 데이터를 파싱할 것인지에 대한 여부입니다. 
        /// 채보 정보 전부를 파싱해야 한다면 true를 건네주어야 합니다. 
        /// 하지만 채보 정보가 당장에 필요하지 않을 때 false를 건네줌으로서 성능이 향상된 파싱을 기대할 수 있습니다.</param>
        /// <returns>파싱 성공 여부입니다.</returns>
        public static bool ParseBMSData(string _bmsDir, out BMSData _data, bool _parseHeader, bool _parseHeaderIndexingAttribute, bool _parseMainData)
        {
            try
            {
                // 디렉토리 내의 모든 파일중에서 bms, bme, bml확장자를 가진 파일들의 path만 색출해내서 저장한다.
                string[] patternPaths = Directory.EnumerateFiles(_bmsDir, "*.*", SearchOption.AllDirectories)
                    .Where(s => s.EndsWith(".bms") || s.EndsWith(".bme") || s.EndsWith(".bml")).ToArray();

                List<BMSPatternData> patternList = new List<BMSPatternData>();
                foreach (string path in patternPaths)
                {
                    BMSPatternData pattern;
                    bool isSuccess = ParseBMSPatternData(path, out pattern, _parseHeader, _parseHeaderIndexingAttribute, _parseMainData);

                    if (isSuccess) patternList.Add(pattern);

                }

                if (patternList.Count == 0)
                {
                    Debug.LogWarningFormat("{0}에서 올바른 bms파일이 검색되지 않았습니다. \n" +
                        "bms파일이 존재하는지, 혹은 bms파일 내부적으로 구문오류가 있는지 확인해주세요.", _bmsDir);

                    throw new System.Exception();
                }

                patternList.Sort();

                // 폴더 이름을 제목으로 간주한다.
                // 아티스트명과 bpm은 0번 패턴의 정보를 가져와 초기화한다.
                string title = Path.GetFileName(_bmsDir);
                string artist = patternList[0].Header.Artist;
                double bpm = patternList[0].Header.BPM;

                _data = new BMSData(_bmsDir, title, artist, bpm, patternList.ToArray());

                return true;
            }
            catch (System.Exception e)
            {
                DebugHelper.LogError(e);
                _data = null;
                return false;
            }

        }

        /// <summary>
        /// 단일 패턴파일을 파싱합니다.
        /// *.bms파일의 원본 문자열을 인자로 받고 내부적으로 파싱한 뒤, 그 결과물인 BMSPatternInstance를 리턴합니다.
        /// </summary>
        /// <param name="originalLines">bms파일을 텍스트로 읽어와 줄 단위로 구분해낸 string 배열입니다.</param>
        /// <param name="data">out하는 bms data 객체 입니다.</param>
        /// <returns> 파싱의 성공 여부를 bool값으로 리턴합니다. </returns>
        public static bool ParseBMSPatternData(string _patternPath, out BMSPatternData _pattern, bool _parseHeader, bool _parseHeaderIndexingAttribute, bool _parseMainData)
        {
            try
            {
                string directory = Path.GetDirectoryName(_patternPath);
                string fileName = Path.GetFileName(_patternPath);

                _pattern = new BMSPatternData(fileName, fileName);
                _pattern.Directory = directory;

                // 파일을 읽어들이고 각 줄을 Header lines와 MainData lines로 구분한 뒤 파싱한다.
                string[] fileLines;
                List<string> headerLines = new List<string>();
                List<string> mainDataLines = new List<string>();

                Regex commentRegex = new Regex(CommentTextRegex);
                Regex headerRegex = new Regex(HeaderFieldTextRegex);
                Regex mainDataRegex = new Regex(MainDataFieldTextRegex);

                // 성능을 높이기 위해 main data를 파싱하지 않을 때 파일 전체를 읽지 않고 파일의 앞부분에서부터 일부만 읽도록 한다.
                if (_parseMainData == false)
                {
                    FileHelper.LoadFileAndSplitByLines(_patternPath, out fileLines, 0, 25);
                }
                else
                {
                    FileHelper.LoadFileAndSplitByLines(_patternPath, out fileLines);
                }

                // 성능을 높이기 위해 main data를 파싱하지 않을 때 main data regex와 조건이 맞는지에 대한 검사를 하지 않는다.
                if (_parseMainData == false)
                {
                    foreach (var line in fileLines)
                    {
                        if (headerRegex.IsMatch(line))
                            headerLines.Add(line);
                    }
                }
                else
                {
                    foreach (var line in fileLines)
                    {
                        if (mainDataRegex.IsMatch(line))
                            mainDataLines.Add(line);
                        else if (headerRegex.IsMatch(line))
                            headerLines.Add(line);
                    }
                }


                if (_parseHeader == true)
                    ParseHeader(headerLines.ToArray(), ref _pattern, _parseHeaderIndexingAttribute);

                if (_parseMainData == true)
                {
                    ParseMainData(mainDataLines.ToArray(), ref _pattern);

                    // 키 정보는 채보 정보 파싱 후에 판별한다.
                    if (_pattern.MainData.NoteList != null)
                    {
                        _pattern.ExistScratch = _pattern.MainData.NoteList.Count(x => x.GetLaneType() == LaneType.SCRATCH) > 0;
                        if (_pattern.MainData.NoteList.Count(x => x.GetLaneType() == LaneType.NOTE7) > 0)
                            _pattern.KeyType = KeyMode.SP7;
                        else if (_pattern.MainData.NoteList.Count(x => x.GetLaneType() == LaneType.NOTE6) > 0)
                            _pattern.KeyType = KeyMode.SP6;
                        else if (_pattern.MainData.NoteList.Count(x => x.GetLaneType() == LaneType.NOTE5) > 0)
                            _pattern.KeyType = KeyMode.SP5;
                        // 5~7키가 아니라면 4키모드로 간주한다.
                        else
                            _pattern.KeyType = KeyMode.SP4;
                    }

                }


                return true;

            }
            catch (System.Exception e)
            {
                DebugHelper.LogError(e);
                _pattern = null;
                return false;
            }

        }


        /// <summary>
        /// 헤더 정보를 파싱하여 ref pattern의 Header에 저장한다.
        /// </summary>
        /// <param name="_lines">헤더 정보를 파싱할 대상인 문자열 한 줄들의 배열이다.</param>
        /// <param name="_pattern">헤더 정보를 파싱하고 담을 대상객체이다.</param>
        /// 곡 목록에서 메타데이터만 보여주기 위한 경우에는 간소화하기 위해 true를, 
        /// bms플레이에 앞서 인덱싱 속성 정보들까지 전부 필요하다면 true를 주어야 한다.</param>
        /// <returns>헤더 정보의 파싱 성공 여부를 리턴한다.</returns>
        private static bool ParseHeader(string[] _lines, ref BMSPatternData _pattern, bool _parseIndexingAttribute)
        {
            BMSHeaderData header = new BMSHeaderData();

            Regex headerRegex = new Regex(HeaderFieldTextRegex);
            Regex indexingAttributeRegex = new Regex(IndexingAttributeRegex);

            foreach (string line in _lines)
            {
                var headerRegexMatchGroup = headerRegex.Match(line).Groups;

                string attributeText = headerRegexMatchGroup[HeaderFieldTextRegexSubgroupAttributeName].Value;
                string valueText = headerRegexMatchGroup[HeaderFieldTextRegexSubgroupValueName].Value;

                bool isIndexingAttribute = indexingAttributeRegex.IsMatch(attributeText);
                // Indexing Values
                if (_parseIndexingAttribute == true && isIndexingAttribute == true)
                {
                    var indexingAttributeRegexMatchGroup = indexingAttributeRegex.Match(attributeText).Groups;
                    string indexingAttributeText = indexingAttributeRegexMatchGroup[IndexingAttributeRegexSubgroupIndexingAttributeName].Value;
                    string IndexText = indexingAttributeRegexMatchGroup[IndexingAttributeRegexSubgroupIndexName].Value;
                    int index = (int)Base36Library.Base36.Decode(IndexText);

                    switch (indexingAttributeText.ToUpper())
                    {
                        case WavIndexingAttributeName:
                            header.WavDict.Add(index, valueText);
                            break;
                        case BmpIndexingAttributeName:
                            header.BmpDict.Add(index, valueText);
                            break;
                        case StopIndexingAttributeName:
                            header.StopDict.Add(index, int.Parse(valueText));
                            break;
                        case BpmIndexingAttributeName:
                            header.BpmDict.Add(index, double.Parse(valueText));
                            break;
                        case LNOBJIndexingAttributeName:
                            header.LNOBJList.Add(index);
                            break;
                    }

                }
                // Non Indexing Values
                else if (isIndexingAttribute == false)
                {
                    switch (attributeText.ToUpper())
                    {
                        case PlayerAttributeName:
                            header.Player = int.Parse(valueText);
                            break;
                        case GenreAttributeName:
                            header.Genre = valueText;
                            break;
                        case TitleAttributeName:
                            header.Title = valueText;
                            break;
                        case ArtistAttributeName:
                            header.Artist = valueText;
                            break;
                        case BPMAttributeName:
                            header.BPM = double.Parse(valueText);
                            break;
                        case PlayLevelAttributeName:
                            header.PlayLevel = int.Parse(valueText);
                            break;
                        case LNTypeAttributeName:
                            header.LNType = int.Parse(valueText);
                            break;
                        case StageFileAttributeName:
                            header.StageFile = valueText;
                            break;
                    }

                }

            } // foreach line in lines

            _pattern.Header = header;
            _pattern.IsHeaderParsed = true;
            _pattern.IsHeaderIndexingAttributeParsed = _parseIndexingAttribute;
            return true;

        }


        // 메인 데이터 필드 파싱
        private static bool ParseMainData(string[] lines, ref BMSPatternData pattern)
        {

            if (lines.Length == 0)
            {
                Debug.LogWarning("파싱된 Main Data Field영역 안의 데이터가 없습니다. 해당 영역 안의 데이터들이 정규식 형식을 잘 갖추었는지 확인하세요. \n아 참, 혹시 #같은 기본적인 걸 빠뜨린건 아니겠죠?");
                pattern.MainData = null;
                return false;
            }

            Regex mainDataRegex = new Regex(MainDataFieldTextRegex);


            List<BMSObject> eventList = new List<BMSObject>();
            List<Note> noteList = new List<Note>();

            // 롱노트 타이밍 계산을 위하여, 롱노트의 시작부분이 발견되었을 때 이 배열에 담고
            // 나중에 롱노트의 끝부분을 찾았을 때 이 리스트에서 롱노트의 첫부분을 가져와 롱노트의 끝부분 타이밍 계산을 한다.
            LongNote[] LNs = new LongNote[10];

            int lastBarIndex = 0;

            foreach (string line in lines)
            {
                var mainDataRegexMatchGroup = mainDataRegex.Match(line).Groups;

                string barIndexText = mainDataRegexMatchGroup[MainDataRegexSubgroupBarIndexName].Value;
                string channel1Text = mainDataRegexMatchGroup[MainDataRegexSubgroupChannel1Name].Value;
                string channel2Text = mainDataRegexMatchGroup[MainDataRegexSubgroupChannel2Name].Value;
                string patternDataText = mainDataRegexMatchGroup[MainDataRegexSubgroupPatternDataName].Value;

                int barIndex = int.Parse(barIndexText);
                int channel1Number = int.Parse(channel1Text);
                int channel2Number = int.Parse(channel2Text);
                int channelFullNumber = channel1Number * 10 + channel2Number;
                ChannelType channelType = (ChannelType)channelFullNumber;

                // 채널 02(마디단축)가 아닌 이상 전부 2자리로 쪼개서 파싱해야하는 채널이다.
                bool flag = !(channelType == ChannelType.EVENT_CHANGE_BAR_LENGTH);
                if (flag)
                {

                    if (patternDataText.Length % 2 != 0)
                    {
                        string errpoint = string.Format("#{0}  {1}", barIndex, channelFullNumber);
                        string errMessage = string.Format("{0} MainData 구문이 이상합니다. 노트채널의 데이터는 2개씩 딱딱 맞게 쪼개어져야하는데 말이죠..\n혹시 해당 채널은 노트채널이 아닌가요?", errpoint);
                        Debug.LogError(errMessage);

                        pattern.MainData = null;
                        return false;
                    }

                    // LNTYPE 1에서 사용할, 지난 노트의 비트값을 가지고있는 변수
                    double prevBeat = 0;

                    int noteCount = patternDataText.Length / 2;
                    for (int i = 0; i < noteCount; i++)
                    {
                        string seperatedNoteDataText = patternDataText.Substring(2 * i, 2);

                        // 데이터가 00인 경우(더미 노트인 경우) 추가하지 않는다.
                        // 따라서 아래 코드를 거치지 않고 continue 한다.
                        // 단, 롱노트의 경우에는 LNTYPE 2의 경우 00값이 필요한 경우가 있어, 이 경우에는 continue 하지 않는다.
                        bool isLongNoteAndType2 = pattern.Header.LNType == 2 && channel1Number.InRange(5, 6);
                        if (seperatedNoteDataText == DUMMY_NOTE && isLongNoteAndType2 == false) continue;

                        int base36Value = (int)Base36Library.Base36.Decode(seperatedNoteDataText);
                        double beat = (double)i / (double)noteCount;


                        // 이벤트 채널
                        if (channelType == ChannelType.EVENT_BGM)
                        {
                            BGMEvent bgmEvent = new BGMEvent(barIndex, beat, channelType, base36Value);
                            eventList.Add(bgmEvent);
                        }
                        else if (channelType == ChannelType.EVENT_BGA)
                        {
                            BGAEvent bgaEvent = new BGAEvent(barIndex, beat, channelType, base36Value);
                            eventList.Add(bgaEvent);
                        }
                        else if (channelType == ChannelType.EVENT_CHANGE_BPM)
                        {
                            int hexValue = int.Parse(seperatedNoteDataText, System.Globalization.NumberStyles.HexNumber);
                            BPMEvent bpmEvent = new BPMEvent(barIndex, beat, channelType, hexValue);
                            eventList.Add(bpmEvent);
                        }
                        else if (channelType == ChannelType.EVENT_STOP)
                        {
                            StopEvent stopEvent = new StopEvent(barIndex, beat, channelType, base36Value);
                            eventList.Add(stopEvent);
                        }
                        else if (channelType == ChannelType.EVENT_EXBPM)
                        {
                            // 확장 BPM채널
                            if (pattern.Header.BpmDict.ContainsKey(base36Value) == true)
                            {
                                BPMEvent bpmEvent = new BPMEvent(barIndex, beat, channelType, pattern.Header.BpmDict[base36Value]);
                                eventList.Add(bpmEvent);
                            }
                            else
                            {
                                Debug.LogError("확장 BPM이벤트에 해당되는 키 값이 없습니다.");
                            }

                        }

                        // 오브젝트 채널
                        // 플레이어 1 / 2 노트
                        else if (channel1Number == 1 || channel1Number == 2)
                        {
                            Note note = new Note(barIndex, beat, channelType, base36Value);
                            noteList.Add(note);
                        }
                        // 플레이어 1 / 2 롱노트
                        else if (channel1Number == 5 || channel1Number == 6)
                        {
                            bool isStartingNote = LNs[channel2Number] == null;
                            // LNType1의 경우, 노트의 끝부분은 처음으로 0이 아닌 노트가 오는 경우이다.
                            //      우리의 경우엔 이미 0값을 배제하고 루프를 돌고있기 때문에, LNs[channel2Number]가 비어있지 않다면 무조건 롱노트의 끝부분이라 간주한다.
                            // LNType2의 경우, 노트의 끝부분은 시작롱노트와 KeySound가 다른 노트가 오는 경우이다.
                            bool isEndingNote = pattern.Header.LNType == 1 && LNs[channel2Number] != null ? true
                                                : pattern.Header.LNType == 2 && LNs[channel2Number] != null && LNs[channel2Number].KeySound != base36Value ? true : false;

                            if (isStartingNote == true)
                            {
                                LNs[channel2Number] = new LongNote(barIndex, beat, channelType, base36Value);

                                noteList.Add(LNs[channel2Number]);
                            }
                            else if (isEndingNote == true)
                            {
                                LNs[channel2Number].EndBarIndex = barIndex;
                                LNs[channel2Number].EndBeat = beat;

                                LNs[channel2Number] = null;

                                // 노트 리스트에 롱노트 타이밍 계산을 위해 꼬리(끝부분)를 추가한다.
                                noteList.Add(new LongNoteTail(barIndex, beat, channelType));
                            }

                        }
                        // 플레이어 1 / 2 숨김노트
                        else if (channel1Number == 3 || channel1Number == 4) { }

                        prevBeat = beat;

                    } // end for

                    // Bar오브젝트 저장을 위해 맨 마지막 bar index를 갱신한다.
                    lastBarIndex = lastBarIndex < barIndex ? barIndex : lastBarIndex;

                }
                // 채널 02(마디 단축)는 특별히, 쪼개지 않고 그대로 저장한다.
                else
                {
                    double doubleValue = double.Parse(patternDataText);
                    ChangeBarLengthEvent changeBarLengthEvent = new ChangeBarLengthEvent(barIndex, 0, channelType, doubleValue);
                    eventList.Add(changeBarLengthEvent);
                }

            } // foreach line in lines

            // Bar 오브젝트를 추가한다.
            for (int i = 1; i <= lastBarIndex; i++)
            {
                BarEvent newBar = new BarEvent(i, 0, ChannelType.NONE);
                eventList.Add(newBar); // 리스트의 맨 뒤에 추가하게 된다.
            }

            // 타이밍 계산 전에 반드시 event와 note의 리스트를 정렬해야 한다.
            eventList.Sort();
            noteList.Sort();

            // 타이밍 계산 전, pattern의 MainData객체를 생성한다.
            pattern.MainData = new BMSMainData();
            pattern.MainData.EventList = eventList;
            pattern.MainData.NoteList = noteList;

            // 타이밍 계산
            CalculateAllObjectsTiming(ref pattern);

            pattern.IsMainDataParsed = true;

            return true;

        }

        /// <summary>
        /// 인자로 받은 pattern의 모든 maindata 노트들의 타이밍을 계산하고 저장한다.
        /// </summary>
        /// <param name="pattern"></param>
        private static void CalculateAllObjectsTiming(ref BMSPatternData pattern)
        {

            // 타이밍 계산 전, event와 note의 리스트를 한 데 모으고 그것을 또 다시 정렬한다.
            // 이벤트 노트에 의해 시간 속성이 바뀔 수 있기 때문에, 타이밍 계산은 한 데 모아 정렬한 상태로 해야한다.
            List<BMSObject> allObjectList = new List<BMSObject>();
            allObjectList.AddRange(pattern.MainData.NoteList);
            allObjectList.AddRange(pattern.MainData.EventList);

            allObjectList.Sort();


            int currentBarIndex = 0;
            double currentBeat = 0;
            double currentBPM = pattern.Header.BPM;
            double currentTimeMillis = 0;

            double beatDurationMillis = 60000 / currentBPM;
            double barDurationMillis = beatDurationMillis * 4;

            LongNote[] LNs = new LongNote[10];  // 롱노트 헤드 찾기위한 배열
            double barDurationMultiplier = 1;   // 마디 길이 변경 (곱셈연산자)

            for (int i = 0; i < allObjectList.Count; i++)
            {
                BMSObject currentObject = allObjectList[i];

                // 다른 마디의 오브젝트를 다루게 된다면, 
                if (currentBarIndex < currentObject.BarIndex)
                {
                    // 지난 마디에서 건너뛰어진 시간만큼 더한다.
                    currentTimeMillis = currentTimeMillis + (1 - currentBeat) * barDurationMillis * barDurationMultiplier;
                    currentBeat = 0;
                    currentBarIndex++;

                    // 마디단축이 적용되어 있는 상태라면 해제한다.
                    barDurationMultiplier = 1;

                    // 현재 처리할 오브젝트의 마디가 지금 마디보다 더 앞서있다면, 그 마디 차이 시간을 계산하여 더해준다.
                    currentTimeMillis = currentTimeMillis + (currentObject.BarIndex - currentBarIndex) * barDurationMillis;
                    currentBarIndex = currentObject.BarIndex;

                }

                // 다른 비트의 오브젝트를 다루게 된다면,
                if (currentBeat < currentObject.Beat)
                {
                    currentTimeMillis = currentTimeMillis + (currentObject.Beat - currentBeat) * barDurationMillis * barDurationMultiplier;
                    currentBeat = currentObject.Beat;
                }

                currentObject.TimingMillis = currentTimeMillis;

                // 롱노트 관련 객체인 경우, 타이밍 계산 후에 추가적인 작업을 거친다.
                if (currentObject is LongNote)
                {
                    LongNote ln = currentObject as LongNote;
                    LNs[(int)ln.GetLaneType()] = ln;
                }
                else if (currentObject is LongNoteTail)
                {
                    LongNoteTail lnEndEvent = currentObject as LongNoteTail;
                    int laneIndex = (int)lnEndEvent.GetLaneType();

                    // 롱노트의 앞부분이 있었을 때에만 초기화한다.
                    if (LNs[laneIndex] != null)
                    {
                        LNs[laneIndex].EndTimingMillis = lnEndEvent.TimingMillis;
                        LNs[laneIndex] = null;
                    }
                    else
                    {
                        Debug.Log("롱노트의 앞부분을 찾을 수 없습니다.");
                    }

                }


                // BPM이벤트 오브젝트라면 다음 루프부터 새로이 사용될 시간 관련 값들을 업데이트한다.
                if (currentObject is BPMEvent)
                {
                    BPMEvent bpmEvent = currentObject as BPMEvent;
                    currentBPM = bpmEvent.BPM;
                    beatDurationMillis = 60000 / currentBPM;
                    barDurationMillis = beatDurationMillis * 4;
                }
                else if (currentObject is ChangeBarLengthEvent)
                {
                    ChangeBarLengthEvent changeBarLengthEvent = currentObject as ChangeBarLengthEvent;
                    barDurationMultiplier = changeBarLengthEvent.Multiplier;
                }
                // Stop이벤트 오브젝트라면 pattern의 stop리스트에 저장된 시간대로 멈춘다.
                else if (currentObject is StopEvent)
                {
                    //TODO: STop을 이렇게 파싱할 때 시간을 더해서 주지말고, 플레이중에 플레이시간을 멈추는식으로 해야할거같다.
                    // 아니면 이걸 Global 컴파일옵션으로 뺄까?
                    StopEvent stopEvent = currentObject as StopEvent;
                    double stopDurationMillis = (barDurationMillis * barDurationMultiplier) * (pattern.Header.StopDict[stopEvent.Key] / 192);
                    currentTimeMillis = currentTimeMillis + stopDurationMillis;
                }

            }

            // 노트 리스트에 있는 롱노트 꼬리를 모두 삭제한다.
            pattern.MainData.NoteList.RemoveAll(x => x is LongNoteTail);

            // 루프 중간에서 Bar 오브젝트가 추가되었으므로 eventList를 다시 정렬한다.
            pattern.MainData.EventList.Sort();

        }

    }

}