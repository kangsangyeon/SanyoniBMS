using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
using SanyoniLib.UnityEngineHelper;
using SanyoniLib.UnityEngineExtensions;
using SanyoniLib.SystemHelper;


namespace SanyoniBMS
{

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSPlayer : SingletonMonoBehaviour<BMSPlayer>
    {

        private const float MovespeedLerpSpeed = .25f;
        private const float MovespeedLerpAllowedDifference = .1f;

        #region Public Variables

        public string m_BMSDir;
        public BMSPatternData m_BMSPatternData;

        public GameObject m_SkinPrefab;

        /// <summary>
        /// Initialize를 호출하여 초기화 작업이 완료되었음을 판별하는 플래그.
        /// </summary>
        public bool m_IsInitialized = false;
        /// <summary>
        /// Prepare을 호출하여 플레이 전 처리 작업을 전부 끝냈음을 판별하는 플래그.
        /// 전 처리 작업에는 각종 변수 초기화, 노트 오브젝트 생성, 리소스 로드 등이 포함되어 있다.
        /// </summary>
        public bool m_IsPrepared = false;
        /// <summary>
        /// BMS play중인지를 판별하는 플래그.
        /// </summary>
        public bool m_IsPlaying = false;
        /// <summary>
        /// 멈춤 상태인지를 판별하는 플래그.
        /// </summary>
        public bool m_IsPaused = false;
        /// <summary>
        /// 연주가 모두 종료된 상태인지를 판별하는 플래그.
        /// </summary>
        public bool m_IsFinished = false;
        /// <summary>
        /// 현재 자동연주 모드인지를 설정하는 변수.
        /// </summary>
        public bool m_IsAutoPlay = false;
        /// <summary>
        /// 현재 자동 스크래치 모드인지를 설정하는 변수.
        /// </summary>
        public bool m_IsAutoScratch = false;

        public float m_Health = 1f;

        public BMSPlayerState m_PlayerState = BMSPlayerState.None;


        public BMSPlayResult m_PlayResult;

        #endregion

        #region Private Variables

        [SerializeField] private Material m_BarMaterial;

        private BMSAudioPlayer m_AudioPlayer;
        private BMSBgaPlayer m_BgaPlayer;
        private BMSPlayerSkin m_Skin;

        private Dictionary<LaneType, KeyPressState> m_KeyPressStateDict;

        private List<BMSObject> m_EventList;
        private List<BarEvent> m_BarList;
        private List<Note> m_NoteList;
        private Dictionary<LaneType, List<Note>> m_NoteListByLaneDict;

        private Dictionary<LaneType, int> m_KeySoundByLaneDict;
        private Dictionary<LaneType, double> m_LatestKeysoundUpdateTimeDict;

        private GameObject NoteParent;

        #endregion

        #region Properties

        public double m_CurrentBPM;
        public double m_DestinationMovespeedPerSecond { get { return m_CurrentBPM * CurrentMovespeedMultiplier * m_ResolutionHeightDifferenceRatio * m_DefaultMovespeedMultiplier; } }
        public double m_DestinationMovespeedPerMillis { get { return m_DestinationMovespeedPerSecond / 1000; } }
        public double m_CurrentMovespeedPerSecond { get; private set; }
        public double m_CurrentMovespeedPerMillis { get { return m_CurrentMovespeedPerSecond / 1000; } }
        private double m_CurrentMovespeedMultiplier = 2;
        public double CurrentMovespeedMultiplier
        {
            get { return m_CurrentMovespeedMultiplier; }
            set
            {
                m_CurrentMovespeedMultiplier = value;
                if (m_IsPlaying) HandleAllNotes(ElapsedPlayingTimeMillis, false);
            }
        }
        private double m_ResolutionHeightDifferenceRatio { get { return VideoSettings.TargetResolution.Height / VideoSettings.OriginalResolution.Height; } }  // 해상도 차이로 인한 y값 속도 차이를 맞추기 위해서, 이 비율을 이동속도에 곱해준다.
        private const double m_DefaultMovespeedMultiplier = 3; // 이동속도가 그냥 느려보여서, 이 상수값도 곱해주도록 하자

        public double BeatDurationMillis { get { return 60000 / m_CurrentBPM; } }
        public double BeatDurationSecond { get { return BeatDurationMillis / 1000; } }
        public double BarDurationMillis { get { return BeatDurationMillis * 4; } } //TODO: 4/3박자 같은 경우에는 이런건 안먹힐텐데? 추후 하드코딩한 부분을 수정해서 4/3같은 박자들 지원을 해야겠다.
        public double BarDurationSecond { get { return BarDurationMillis / 1000; } }

        public int m_CurrentBarIndex { get; private set; }
        /// <summary>
        /// 남은 노트 또는 이벤트가 더 이상 재생될 것이 없을 때 true를 반환하는 프로퍼티.
        /// </summary>
        public bool IsBMSPlayFinished
        {
            get
            {
                if (this.m_EventList != null && this.m_EventList != null)
                    return m_EventList.Count == 0 && m_NoteList.Count == 0;
                else
                    return false;
            }
        }
        /// <summary>
        /// 씬이 끝날 때 true를 반환하는 프로퍼티.
        /// </summary>
        public bool IsPlayFinished { get; private set; }         // 씬이 끝날 때 true


        /// <summary>
        /// 준비가 완료되고 Play()함수를 호출한 시각을 저장한 값.
        /// </summary>
        public long StartPlayTimeMillis { get; private set; }
        /// <summary>
        /// Pause한 시각을 저장한 값.
        /// </summary>
        public long PauseTimeMillis { get; private set; }
        /// <summary>
        /// Pause한 뒤 해제할 때까지 걸린 값을 저장한 값.
        /// </summary>
        public long ElapsedPausedTimeMillis { get; private set; }
        /// <summary>
        /// 악곡 연주를 어디까지 진행하였는지 저장한 값. 
        /// StartPlayTimeMillis와 ElapsedPauseTimeMillis값의 영향을 받는다.
        /// </summary>
        public long ElapsedPlayingTimeMillis
        {
            get
            {
                if (this.m_IsPlaying == false) return 0;
                else if (this.m_IsPaused == false) return TimeHelper.EpochTimeMillis - StartPlayTimeMillis - ElapsedPausedTimeMillis;
                else return ElapsedPlayingTimeBeforePauseMillis;
            }
        }
        public double ElapsedPlayingTimeSeconds { get { return (double)ElapsedPlayingTimeMillis / 1000; } }
        /// <summary>
        /// 멈추기 바로 직전까지 플레이했던 시간을 기록하는 변수이다.
        /// </summary>
        public long ElapsedPlayingTimeBeforePauseMillis { get; private set; }


        public double LoadingPercentage
        {
            get
            {
                if (this.m_IsPrepared == true) return 1;
                else if (this.m_AudioPlayer == null || this.m_BgaPlayer == null) return 0;
                else return (double)(this.m_AudioPlayer.LoadedAudioClipCount + this.m_BgaPlayer.LoadedArtCount) / (double)(this.m_AudioPlayer.AllAudioClipCount + this.m_BgaPlayer.AllArtCount);
            }
        }

        #endregion

        private void Update()
        {
            // 초기화되지 않았다면 Update 루프를 빠져나온다.
            if (this.m_IsInitialized == false) return;

            if (this.m_IsPlaying == true && this.m_IsPaused == false) Tick();

        }

        public void Play()
        {
            this.m_IsPlaying = true;
            this.m_PlayerState = BMSPlayerState.Playing;

            StartPlayTimeMillis = TimeHelper.EpochTimeMillis;


            //TODO: 나중에 'mainThread에서만 unity 속성 접근가능!' 에러를 해결하게 된다면 이걸 사용하자.
            //Task task;
            //this.StartCoroutineAsync(TickAsync(), out task);
        }


        public void Initialize()
        {
            this.m_BMSDir = string.Empty;
            this.m_BMSPatternData = null;

            this.NoteParent = null;

            // 필요한 객체들을 초기화하고 생성한다.
            this.m_PlayResult = null;

            // 필요한 컴포넌트들을 초기화한다.
            if (this.m_AudioPlayer != null)
            {
                Destroy(this.m_AudioPlayer.gameObject);
                this.m_AudioPlayer = null;
            }
            if (this.m_BgaPlayer != null)
            {
                Destroy(this.m_BgaPlayer.gameObject);
                this.m_BgaPlayer = null;
            }
            if (this.m_Skin != null)
            {
                Destroy(this.m_Skin.gameObject);
                this.m_Skin = null;
            }

            // 플래그들을 초기화한다.
            this.m_PlayerState = BMSPlayerState.None;
            this.m_IsInitialized = true;
            //this.m_IsPrepared = false;
            this.m_IsPlaying = false;

            // BMSObject 리스트들을 null로 비운다.
            this.m_EventList = null;
            this.m_NoteList = null;
            this.m_BarList = null;

            // 딕셔너리를 null로 비운다.
            this.m_KeyPressStateDict = null;
            this.m_NoteListByLaneDict = null;
            this.m_KeySoundByLaneDict = null;
            this.m_LatestKeysoundUpdateTimeDict = null;

            // 시간 관련 변수들을 초기화한다.
            this.m_CurrentBPM = 0;
            StartPlayTimeMillis = 0;
            PauseTimeMillis = 0;
            ElapsedPausedTimeMillis = 0;

            this.m_CurrentBarIndex = 0;

        }

        public IEnumerator CPrepare(string bmsDir, BMSPatternData bmsData)
        {
            this.m_BMSDir = bmsDir;
            this.m_BMSPatternData = bmsData;

            // 필요한 객체를 생성한다.
            this.m_PlayResult = new BMSPlayResult();

            // 오디오, bga 플레이어를 생성하고 초기화한다.
            GameObject audioPlayerGO = new GameObject("BMSAudioPlayer");
            audioPlayerGO.transform.parent = this.transform;
            this.m_AudioPlayer = audioPlayerGO.AddComponent<BMSAudioPlayer>();
            GameObject bgaPlayerGO = new GameObject("BMSBgaPlayer");
            bgaPlayerGO.transform.parent = this.transform;
            this.m_BgaPlayer = bgaPlayerGO.AddComponent<BMSBgaPlayer>();
            this.m_AudioPlayer.Initialize();
            this.m_BgaPlayer.Initialize();

            GameObject skinGO = GameObject.Instantiate(m_SkinPrefab, this.transform);
            this.m_Skin = skinGO.GetComponent<BMSPlayerSkin>();
            this.m_Skin.Initialize();

            // 딕셔너리를 초기화한다.
            this.m_KeyPressStateDict = new Dictionary<LaneType, KeyPressState>();
            this.m_LatestKeysoundUpdateTimeDict = new Dictionary<LaneType, double>();
            foreach (var item in Global.KeySettingsDict[this.m_BMSPatternData.KeyType].GetLanes())
            {
                this.m_KeyPressStateDict.Add(item, KeyPressState.NONE);
                this.m_LatestKeysoundUpdateTimeDict.Add(item, -1);
            }

            // BMSObject Event, Note 리스트는 pattern 객체 안의 것들을 직접 사용하지 않고 사본을 만들어 사용한다.
            this.m_EventList = new List<BMSObject>();
            this.m_NoteList = new List<Note>();
            this.m_BarList = new List<BarEvent>();
            foreach (var eventObject in bmsData.MainData.EventList)
            {
                // EventList중에서 Bar만을 추출해내어 구분한다.
                if (eventObject is BarEvent)
                    this.m_BarList.Add(eventObject.Clone() as BarEvent);
                else
                    this.m_EventList.Add(eventObject.Clone() as BMSObject);
            }
            foreach (var note in bmsData.MainData.NoteList) m_NoteList.Add(note.Clone() as Note);

            Debug.Log(this.m_BarList.Count);

            // 노트들을 레인별로 정렬하고 그것을 딕셔너리로 저장한다.
            var NoteByLaneGroup = from note in m_NoteList
                                  group note by note.GetLaneType() into laneGroup
                                  select laneGroup;

            this.m_NoteListByLaneDict = new Dictionary<LaneType, List<Note>>();
            foreach (var noteListByLane in NoteByLaneGroup)
                this.m_NoteListByLaneDict.Add(noteListByLane.Key, noteListByLane.ToList());

            // Lane마다 키음 채널이 존재한다.
            this.m_KeySoundByLaneDict = new Dictionary<LaneType, int>();
            foreach (var item in NoteByLaneGroup) this.m_KeySoundByLaneDict.Add(item.Key, -1);

            // Lane별 초기 키음을 설정한다. 초기 키음은 각 레인에 있는 가장 첫 번째 노트의 키음이다.
            LaneType[] lanes = Global.KeySettingsDict[this.m_BMSPatternData.KeyType].GetLanes();
            foreach (var item in lanes)
            {
                Note note = this.m_NoteListByLaneDict[item][0];
                this.m_KeySoundByLaneDict[item] = note.KeySound;
                this.m_LatestKeysoundUpdateTimeDict[item] = note.TimingMillis;
            }

            // BPM을 설정한다.
            this.m_CurrentBPM = m_BMSPatternData.Header.BPM;

            // 초기 이동속도는 0부터 보간되지 않도록 초기화되어야 한다.
            // 노트들의 위치가 생성될 시에 제 자리에 위치할 수 있도록 하기 위함이다.
            this.m_CurrentMovespeedPerSecond = this.m_DestinationMovespeedPerSecond;

            // 모든 노트들을 생성시킨다.
            SpawnAllNotesAndBar();

            // 리소스들을 로드한다.
            LoadResources();


            // 오디오, bga의 로드가 완료되면 준비가 완료된다.
            yield return new WaitUntil(() => this.m_AudioPlayer.IsPrepared == true && this.m_BgaPlayer.IsPrepared == true);
            this.m_IsPrepared = true;
        }

        public void Input(LaneType _laneType, bool keyState)
        {
            KeyPressState currentState;
            KeyPressState prevState = this.m_KeyPressStateDict[_laneType];

            if (keyState == true)
            {
                if (prevState == KeyPressState.DOWN || prevState == KeyPressState.HOLD) currentState = KeyPressState.HOLD;
                else currentState = KeyPressState.DOWN;
            }
            else
            {
                if (prevState == KeyPressState.HOLD || prevState == KeyPressState.DOWN) currentState = KeyPressState.RELEASE;
                else currentState = KeyPressState.NONE;
            }

            this.m_KeyPressStateDict[_laneType] = currentState;
            this.Input(_laneType, currentState);
        }

        public void Input(LaneType _laneType, KeyPressState _state)
        {
            this.m_KeyPressStateDict[_laneType] = _state;

            var noteList = this.m_NoteListByLaneDict[_laneType];

            // 다음과 같은 경우에는 아무 처리도 하지 않는다.
            // 0. 버튼을 꾹 누르고 있거나 아무 상태도 아닐 때
            if (_state == KeyPressState.NONE || _state == KeyPressState.HOLD) return;
            // 1. 없는 레인에 대한 입력
            else if (noteList == null) return;
            // 2. 자동 연주모드중에 입력
            else if (this.m_IsAutoPlay == true) return;
            // 3. 자동 스크래치 연주모드중에 스크래치 입력
            else if (this.m_IsAutoScratch == true && _laneType == LaneType.SCRATCH) return;


            if (noteList.Count > 0)
            {
                // 처리 시도할 노트는 입력한 레인의 가장 첫 번째 노트이다.
                Note note = noteList[0];

                // 1. 판정시간보다 살짝이라도 뒤늦은 노트이거나, 
                // 2. 지난 노트시간과 현재 노트의 중간 이상 진행되었다면
                bool isLatedNote = note.TimingMillis <= ElapsedPlayingTimeMillis;
                bool overHalfOverDifference = (note.TimingMillis + this.m_LatestKeysoundUpdateTimeDict[_laneType]) * 0.5 < ElapsedPlayingTimeMillis;
                if (isLatedNote || overHalfOverDifference)
                {
                    this.m_KeySoundByLaneDict[_laneType] = note.KeySound;
                    this.m_LatestKeysoundUpdateTimeDict[_laneType] = note.TimingMillis;
                }

                // 롱노트
                if (note is LongNote)
                {
                    LongNote ln = note as LongNote;

                    if (ln.Pressed == false && _state == KeyPressState.DOWN && BMSJudgeCalculator.Judge(ln.TimingMillis, ElapsedPlayingTimeMillis) != JudgementType.None)
                        HitNote(_laneType, 0, true, ElapsedPlayingTimeMillis, false);
                    else if (ln.Pressed == true && _state == KeyPressState.RELEASE)
                        ReleaseNote(_laneType, 0, ElapsedPlayingTimeMillis);

                }
                // 단노트
                else
                {
                    //Debug.LogFormat("{0}\t{1}", note.TimingMillis, BMSPlayer.ElapsedPlayingTimeMillis);

                    // 판정시간이 최악의 판정시간 이내로 들어왔다면 노트를 처리할 수 있게 된다.
                    if (_state == KeyPressState.DOWN && BMSJudgeCalculator.Judge(note.TimingMillis, ElapsedPlayingTimeMillis) != JudgementType.None)
                        HitNote(_laneType, 0, false, ElapsedPlayingTimeMillis, false);
                }

            }


            // 키를 눌렀을 타이밍에만 소리를 재생한다.
            if (_state == KeyPressState.DOWN)
                this.m_AudioPlayer.TryPlayInDictionary(this.m_KeySoundByLaneDict[_laneType]);
        }

        #region Private Methods

        /// <summary>
        /// 모든 노트들과 마디의 렌더링을 위한 게임 오브젝트를 씬에 스폰하는 함수.
        /// </summary>
        private void SpawnAllNotesAndBar()
        {
            Note note = null;
            GameObject notePrefab = null;
            GameObject lnBodyPrefab = null;

            try
            {
                // 기존에 노트 parent가 있다면 삭제하고 다시 생성한다.
                if (NoteParent != null) Destroy(NoteParent);
                NoteParent = new GameObject(m_BMSPatternData.PatternTitle);
                NoteParent.transform.parent = this.transform;


                // 노트들의 게임 오브젝트 모델을 생성한다.
                for (int i = 0; i < m_NoteList.Count; i++)
                {
                    note = m_NoteList[i];
                    notePrefab = null;
                    lnBodyPrefab = null;

                    // 플레이어1의 노트가 아니면 처리하지 않고 건너뛴다.
                    if (note.GetPlayerIndex() != 1)
                        continue;

                    notePrefab = this.m_Skin.GetNotePrefabByLane(note.GetLaneType());
                    lnBodyPrefab = this.m_Skin.GetLNPrefabByLane(note.GetLaneType());

                    note.Model = GameObject.Instantiate(notePrefab, NoteParent.transform, true);

                    // 노트가 롱노트였다면 Body와 Tail모델도 생성해준다.
                    if (note is LongNote)
                    {
                        LongNote ln = note as LongNote;
                        ln.BodyModel = GameObject.Instantiate(lnBodyPrefab, NoteParent.transform, true);
                        ln.BodyModel.name = "LN Body " + ln.ChannelType.ToString();
                        ln.TailModel = GameObject.Instantiate(notePrefab, NoteParent.transform, true);
                        ln.TailModel.name = "LN Tail " + ln.ChannelType.ToString();

                        // 꼬리의 모델 생성은 Body의 길이 표현을 위해 임시적으로 생성한 것일 뿐이며 모습을 가려주어야 자연스럽게 나온다.
                        ln.TailModel.SetActive(false);

                        // 노트 앞부분은 Body보다 앞쪽에 표시되어야 자연스럽다.
                        ln.Model.GetComponent<SpriteRenderer>().sortingOrder++;

                    }
                    note.Model.name = note.ChannelType.ToString();

                }

                // 노트들의 위치를 설정한다.
                HandleAllNotes(0, true);

            }
            catch (System.Exception e)
            {
                DebugHelper.LogErrorFormat(e, "BMSPlayer.SpawnAllNotesAndBar(): {0}", note);
            }

        }


        ////TODO: 비동기로 리소스 로딩할 수 있도록 만들 것
        /// 스레드닌자말고 그...다른 스레드에서도 unity object 접근 가능하게 해주는 플러긴 사용
        private void LoadResources()
        {
            // 각종 Wav파일들, Bmp파일들, 동영상 파일들을 플레이 이전에 먼저 로드한다.
            this.m_AudioPlayer.Prepare(m_BMSDir, m_BMSPatternData);

            this.m_BgaPlayer.Prepare(m_BMSDir, m_BMSPatternData);
        }


        private void Tick()
        {
            // 이벤트 오브젝트 검사 및 실행
            HandleEventObjects(ElapsedPlayingTimeMillis);

            // 이동속도 보간
            //  - 보간중일 때에는 항상 모든 노트를 이동시켜야 한다.
            if (System.Math.Abs(m_DestinationMovespeedPerSecond - m_CurrentMovespeedPerSecond) > MovespeedLerpAllowedDifference)
                m_CurrentMovespeedPerSecond = Mathf.Lerp((float)m_CurrentMovespeedPerSecond, (float)m_DestinationMovespeedPerSecond, MovespeedLerpSpeed);

            // 노트 처리 (이동, 미처리 노트 자동삭제)
            HandleAllNotes(ElapsedPlayingTimeMillis, false);
            //Debug.LogFormat("IsPaused: {0} , ElapsedPlayingTimeMillis: {1}", this.m_IsPaused, ElapsedPlayingTimeMillis);

            // AutoPlay
            if (this.m_IsAutoPlay) AutoPlay(ElapsedPlayingTimeMillis, false);
            else if (this.m_IsAutoScratch) AutoPlay(ElapsedPlayingTimeMillis, true);

            // AutoPlay여부를 PlayResult에 기록
            if (this.m_IsAutoPlay || this.m_IsAutoScratch) this.m_PlayResult.IsAutoPlay = true;

            // 스킨 애니메이션 - 레인
            this.m_Skin.AnimateLanes(this.m_KeyPressStateDict);

            // 스킨 애니메이션 - 체력
            this.m_Skin.AnimateHealthGuage(this.m_Health);

            // 스킨 애니메이션 - 스피드
            this.m_Skin.AnimateSpeed((float)CurrentMovespeedMultiplier);

            // 판정 시간 위치 Debug
            if (DebugSettings.DebugMode) DebugDrawJudgementTime();

        }

        private void AutoPlay(double _elapsedPlayingTimeMillis, bool _onlyScratch)
        {
            foreach (var item in this.m_NoteListByLaneDict)
            {
                LaneType lane = item.Key;
                List<Note> noteByLaneList = item.Value;

                // auto scratch only인 경우 scratch lane을 제외하고 처리를 건너뛴다.
                if (_onlyScratch == true && lane != LaneType.SCRATCH) continue;

                // 해당 lane을 release했다고 간주하고 keyPressState를 초기화한다. 
                // 어짜피 keyPressState는 자동 hit판정 타이밍시에 적절한 값으로 바뀔 것이기 때문에 release했다고 간주해도 상관없다.
                if (this.m_KeyPressStateDict[lane] == KeyPressState.DOWN || this.m_KeyPressStateDict[lane] == KeyPressState.HOLD)
                    this.m_KeyPressStateDict[lane] = KeyPressState.RELEASE;
                else
                    this.m_KeyPressStateDict[lane] = KeyPressState.NONE;


                // 레인별 노트 리스트를 순회하며 위치 계산
                for (int iNote = 0; iNote < noteByLaneList.Count; iNote++)
                {
                    Note note = noteByLaneList[iNote];

                    if (note.Model == null) continue;

                    // 롱노트 여부에 따라 press(/release) 처리가 다르다.
                    if (note is LongNote)
                    {
                        LongNote ln = note as LongNote;

                        bool autoDown = ln.Pressed == false && ln.TimingMillis <= _elapsedPlayingTimeMillis;
                        bool autoRelease = ln.Pressed == true && ln.EndTimingMillis <= _elapsedPlayingTimeMillis;
                        bool autoHold = ln.Pressed == true && autoRelease == false;

                        if (autoDown == true)
                        {
                            this.HitNote(note.GetLaneType(), iNote, true, _elapsedPlayingTimeMillis, true);
                            // 롱노트down은 리스트에서 바로 삭제하지 않기 때문에 iNote를 -1할 필요가 없다.

                            this.m_KeyPressStateDict[note.GetLaneType()] = KeyPressState.DOWN;
                        }
                        else if (autoRelease == true)
                        {
                            this.ReleaseNote(note.GetLaneType(), iNote, _elapsedPlayingTimeMillis);
                            iNote--;

                            this.m_KeyPressStateDict[note.GetLaneType()] = KeyPressState.RELEASE;
                        }
                        else if (autoHold == true)
                        {
                            this.m_KeyPressStateDict[note.GetLaneType()] = KeyPressState.HOLD;
                        }

                    }
                    // 단노트
                    else
                    {
                        // 오토플레이 중이라면 타이밍에 맞게 자동 재생한다.
                        if (note.TimingMillis <= _elapsedPlayingTimeMillis)
                        {
                            //Debug.LogFormat("<color=#ff0000>AUTO</color> {0}\t{1}", note.TimingMillis, BMSPlayer.ElapsedPlayingTimeMillis);

                            this.HitNote(note.GetLaneType(), iNote, false, _elapsedPlayingTimeMillis, true);
                            iNote--;

                            // 롱노트 Hold가 아닐 때만 Down으로 변경
                            if (this.m_KeyPressStateDict[lane] != KeyPressState.HOLD)
                                this.m_KeyPressStateDict[lane] = KeyPressState.DOWN;
                        }

                    }


                    // 노트들은 시간별로 정렬이 되어있을 것이니, 
                    // 현재 노트가 아직 플레이될 타이밍이 되지 않은 노트라면 그 이후에 나오는 노트들 역시 전부 아직 플레이될 타이밍이 아닌 노트들이다. 
                    // 따라서 루프를 중단한다.
                    if (note.TimingMillis >= _elapsedPlayingTimeMillis)
                        break;

                }

            }
        }

        private void HandleEventObjects(double _elapsedPlayingTimeMillis)
        {
            // Bar는 따로 검사한다.
            for (int i = 0; i < this.m_BarList.Count; i++)
            {
                var item = this.m_BarList[i];

                if (item.TimingMillis <= _elapsedPlayingTimeMillis)
                {
                    this.m_CurrentBarIndex = item.BarIndex;

                    this.m_BarList.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }

            }

            // 이벤트 오브젝트 검사 및 실행
            for (int i = 0; i < m_EventList.Count; i++)
            {
                BMSObject bmsObject = m_EventList[i];

                if (bmsObject.TimingMillis <= _elapsedPlayingTimeMillis)
                {
                    // 타이밍에 안맞게 너무 뒤늦은 오브젝트가 온다면 이것의 처리를 하지않고 그냥 삭제한다.
                    if (BMSJudgeCalculator.Judge(bmsObject.TimingMillis, _elapsedPlayingTimeMillis) == JudgementType.None)
                    {
                        // 아무것도 하지 않는다.
                    }
                    else if (bmsObject is BGMEvent)
                    {
                        BGMEvent bgmEvent = bmsObject as BGMEvent;

                        this.m_AudioPlayer.TryPlayInDictionary(bgmEvent.KeySound);
                    }
                    else if (bmsObject is BGAEvent)
                    {
                        BGAEvent bgaEvent = bmsObject as BGAEvent;
                        m_BgaPlayer.TryPlayInDictionary(bgaEvent.Key);
                    }
                    else if (bmsObject is BPMEvent)
                    {
                        BPMEvent bpmEvent = bmsObject as BPMEvent;
                        m_CurrentBPM = bpmEvent.BPM;

                        if (m_IsPlaying) HandleAllNotes(ElapsedPlayingTimeMillis, false);
                    }
                    else if (bmsObject is StopEvent)
                    {
                        StopEvent stopEvent = bmsObject as StopEvent;
                        int value = m_BMSPatternData.Header.StopDict[stopEvent.Key];
                        // 멈춤 처리
                    }

                    m_EventList.RemoveAt(i);
                    i--;

                    // Debug
                    //if(DebugSettings.DebugMode) Debug.Log(bmsObject.ToString());

                }
                else
                {
                    // EventList는 전부 시간이 오름차순으로 정렬되어 있기 때문에
                    // 순회 도중 타이밍에 안맞는 이벤트를 마주치면 그 이후부터 전부 타이밍에 안맞는 오브젝트이기 때문에
                    // 루프를 탈출한다.
                    break;
                }

            }
        }

        /// <summary>
        /// 노트들을 현재 플레이시간에 맞게 처리한다.
        /// 노트를 현재 플레이시간에 맞게 위치시키고, 자동연주모드중이라면 자동연주한다.
        /// </summary>
        /// <param name="noteList">노트 리스트</param>
        /// <param name="_elapsedPlayingTimeMillis">BMS플레이 경과시간</param>
        /// <param name="_mustLocate">모든 노트가 반드시 배치될 필요가 있는가? 아니라면 보이지 않는 가장 첫 노트 이후의 루프는 탈출한다.
        /// 이 속성은 평시에는 false, 노트 이동속도 변경으로 인하여 모든 노트들의 위치가 바뀌어야 할 때에는 true가 될 것이다.</param>
        private void HandleAllNotes(double _elapsedPlayingTimeMillis, bool _mustLocate)
        {
            // 레인별로 위치를 계산한다.
            foreach (var item in this.m_NoteListByLaneDict)
            {

                // 이전 노트가 재배치되었음에도 카메라 영역보다 위에 있어 렌더링되지 않고 있음을 판별하는 변수.
                // _mustLocate가 false일 때 노트 배치중에 노트가 카메라 영역보다 위에 있어 이 값이 true로 변경된 경우, 최적화를 위해 그 다음 노트부터는 배치하지 않게 하기 위해 만들어진 변수이다.
                bool prevNoteIsInvisibleAfterLocated = false;

                // 레인별 노트 리스트를 순회하며 위치 계산
                List<Note> noteByLaneList = this.m_NoteListByLaneDict[item.Key];
                for (int iNote = 0; iNote < noteByLaneList.Count; iNote++)
                {
                    Note note = noteByLaneList[iNote];

                    if (note.Model == null) continue;


                    // 롱노트 여부에 따라 press(/release) 처리가 다르다.
                    if (note is LongNote)
                    {
                        LongNote ln = note as LongNote;

                        // 노트시간이 최악의 판정시간을 넘어선 경우 최악의 판정으로 처리하고 노트를 없앤다.
                        // 이미 누르고있었던 노트였다면 Release만 Poor로, 누른적이 없던 노트라면 Hit과 Release 둘 다 Poor로
                        bool hitPoor = ln.Pressed == false && note.TimingMillis <= _elapsedPlayingTimeMillis && BMSJudgeCalculator.Judge(note.TimingMillis, _elapsedPlayingTimeMillis) == JudgementType.None;
                        bool releasePoor = ln.EndTimingMillis <= _elapsedPlayingTimeMillis && BMSJudgeCalculator.Judge(ln.EndTimingMillis, _elapsedPlayingTimeMillis) == JudgementType.None;
                        if (hitPoor)
                        {
                            HitNote(item.Key, iNote, true, JudgementType.POOR, false);
                            iNote--;
                            // 롱노트는 헤드를 처리해도 테일이 끝날때까지 움직여야 하기 때문에 continue하지 않는다.
                        }
                        if (releasePoor)
                        {
                            ReleaseNote(item.Key, iNote, JudgementType.POOR);
                            iNote--;
                            continue;
                        }

                    }
                    else
                    {

                        // 노트시간이 최악의 판정시간을 넘어선 경우 최악의 판정으로 처리하고 노트를 없앤다.
                        if (note.TimingMillis <= _elapsedPlayingTimeMillis && BMSJudgeCalculator.Judge(note.TimingMillis, _elapsedPlayingTimeMillis) == JudgementType.None)
                        {
                            HitNote(item.Key, iNote, false, JudgementType.POOR, false);
                            iNote--;
                            continue;
                        }

                    }


                    // 이전 노트를 재배치했는데도 카메라 영역의 바깥에 있어 보이지 않고, 그 다음 노트인 현재 노트도 역시 보이지 않는다면
                    // 현재 노트 이후의 노트부터는 어짜피 보이고 있지도 않을 것이고 재배치해도 보이지 않을 것이기 때문에 
                    // 재배치할 필요가 없는 노트이다. 
                    // => 루프를 탈출한다.
                    if (_mustLocate == false && prevNoteIsInvisibleAfterLocated == true && Camera.main.IsVisible2D(note.Model) == false)
                    {
                        break;
                    }


                    Vector3 notePosition = CalculateObjectPosition(note.GetLaneType(), note.TimingMillis, _elapsedPlayingTimeMillis, m_CurrentMovespeedPerMillis); ;
                    //// 반드시 배치되어야 하는 노트들은 DestinationSpeed에 맞춰 배치를 하고, 후에 업데이트시에 다시 한 번 CurrentSpeed 위치로 옮긴다.
                    //if (_mustLocate == true) notePosition = CalculateObjectPosition(note.GetLaneType(), note.TimingMillis, _elapsedPlayingTimeMillis, m_DestinationMovespeedPerMillis);
                    //else notePosition = CalculateObjectPosition(note.GetLaneType(), note.TimingMillis, _elapsedPlayingTimeMillis, m_CurrentMovespeedPerMillis);

                    note.Model.transform.position = notePosition;

                    // 롱노트의 경우 Body와 Tail Model의 위치, 그리고 Body의 크기도 조절해야 한다.
                    if (note is LongNote)
                    {
                        LongNote ln = note as LongNote;

                        Vector3 tailPosition = CalculateObjectPosition(ln.GetLaneType(), ln.EndTimingMillis, _elapsedPlayingTimeMillis, m_CurrentMovespeedPerMillis);
                        ln.TailModel.transform.position = tailPosition;

                        /// 기존 롱노트 출력방식
                        /// scale을 직접 이용하는 방식이다. 하지만 이렇게 하면 sprite의 9patches 기능을 사용할 수 없기 때문에 지금은 사용하지 않는다.
                        // 롱노트 Body의 position은 head와 tail의 중간부분
                        //ln.BodyModel.transform.position = new Vector3(notePosition.x, notePosition.y + (tailPosition - notePosition).y * .5f, notePosition.z);

                        // 롱노트 Body의 scale은 (note와 tail의 거리) / (body스프라이트의 1유닛당 픽셀 높이)
                        //SpriteRenderer bodyRenderer = ln.BodyModel.GetComponent<SpriteRenderer>();
                        //float spriteHeightPerUnit = bodyRenderer.sprite.rect.height / bodyRenderer.sprite.pixelsPerUnit;
                        //float bodyScaleY = (tailPosition - notePosition).y / spriteHeightPerUnit;
                        //Vector3 bodyScale = new Vector3(1f, (float)bodyScaleY, 1f);
                        //ln.BodyModel.transform.localScale = bodyScale;
                        ///

                        /// 개선된 롱노트 제어방식
                        // 롱노트 Body의 position은 head의 position과 같다.
                        ln.BodyModel.transform.position = notePosition;

                        // 롱노트 Body의 height는 note와 tail의 거리
                        SpriteRenderer bodySpriteRenderer = ln.BodyModel.GetComponent<SpriteRenderer>();
                        bodySpriteRenderer.size = new Vector2(bodySpriteRenderer.size.x, (tailPosition - notePosition).y);
                    }


                    // 노트들은 시간별로 정렬이 되어있을 것이니, 
                    // 현재 노트의 타이밍이 아직 게임시간을 넘지 않았고
                    // 현재 노트가 카메라 영역 바깥에 있어 보이지 않는다면 이후의 노트들도 보이지 않을 것이다.
                    // => 따라서 루프를 중단한다.
                    if (note.TimingMillis >= _elapsedPlayingTimeMillis && Camera.main.IsVisible2D(note.Model) == false)
                    {
                        prevNoteIsInvisibleAfterLocated = true;

                        // Debug
                        if (DebugSettings.DebugMode) note.Model.GetComponent<Renderer>().material.color = Color.red;
                    }
                    else
                    {
                        prevNoteIsInvisibleAfterLocated = false;

                        // Debug용으로 바꿧던 color를 다시 white로 바꿈
                        if (DebugSettings.DebugMode) note.Model.GetComponent<Renderer>().material.color = Color.white;
                    }

                }

            }

        }

        private Vector3 CalculateObjectPosition(LaneType _laneType, double _noteTimingMillis, double _elapsedPlayingTimeMillis, double _movespeedPerMillis)
        {
            float notePositionX = this.m_Skin.GetNotePositionXByLane(_laneType);
            float notePositionY = CalculateObjectPositionY(_noteTimingMillis, _elapsedPlayingTimeMillis, _movespeedPerMillis);

            return new Vector3(notePositionX, notePositionY);
        }

        private float CalculateObjectPositionY(double _timingMillis, double _elapsedPlayingTimeMillis, double _movespeedPerMillis)
        {
            double timeDiffMillis = _timingMillis - _elapsedPlayingTimeMillis;
            return (float)(timeDiffMillis * _movespeedPerMillis) + this.m_Skin.JudgementPositionY;
        }

        /// <summary>
        /// 노트의 판정시간과 플레이시간으로 노트를 판정 처리한다.
        /// </summary>
        /// <param name="_lane"></param>
        /// <param name="_noteIndex"></param>
        /// <param name="_elapsedPlayingTimeMillis"></param>
        /// <param name="_playSound">노트의 keysound를 재생할 것인지를 설정한다. 주로 자동연주시에는 true, 수동연주시에는 false값을 사용한다.</param>
        private void HitNote(LaneType _lane, int _noteIndex, bool _isLN, double _elapsedPlayingTimeMillis, bool _playSound)
        {
            Note note = this.m_NoteListByLaneDict[_lane].ElementAt(_noteIndex);
            HitNote(_lane, _noteIndex, _isLN, BMSJudgeCalculator.Judge(note.TimingMillis, _elapsedPlayingTimeMillis), _playSound);
        }

        /// <summary>
        /// 노트를 파라미터로 주어진 판정으로 처리했음을 간주하고 처리한다.
        /// </summary>
        /// <param name="_lane"></param>
        /// <param name="_noteIndex"></param>
        /// <param name="_judgeType"></param>
        /// <param name="_playSound">노트의 keysound를 재생할 것인지를 설정한다. 주로 자동연주시에는 true, 수동연주시에는 false값을 사용한다.</param>
        private void HitNote(LaneType _lane, int _noteIndex, bool isLN, JudgementType _judgeType, bool _playSound)
        {
            Note note = this.m_NoteListByLaneDict[_lane].ElementAt(_noteIndex);

            // 소리 재생
            if (_playSound)
            {
                // 키음 관련 변수 정보를 업데이트한다.
                this.m_LatestKeysoundUpdateTimeDict[_lane] = note.TimingMillis;
                this.m_KeySoundByLaneDict[_lane] = note.KeySound;

                // 소리를 재생한다.
                this.m_AudioPlayer.TryPlayInDictionary(note.KeySound);
            }

            // 노트 Hit
            note.Hit(_judgeType);

            // 기록 갱신
            this.m_PlayResult.Add(_judgeType);

            // 체력 갱신
            //TODO: 조금 더 코드를 정리하고 시스템을 구체화할 필요가 있어보인다
            if (_judgeType == JudgementType.POOR || _judgeType == JudgementType.POOR)
                this.m_Health -= .085f;
            else
                this.m_Health += .035f;

            this.m_Health = Mathf.Clamp(this.m_Health, 0f, 1f);

            // 롱노트가 아닌 경우에만 리스트에서 삭제한다.
            // 롱노트는 Release에서 이 처리를 해주어야 한다.
            if (isLN == false)
            {
                // 리스트에서 삭제
                this.m_NoteListByLaneDict[_lane].RemoveAt(_noteIndex);

                // 게임 종료 검사를 위해서 레인별 노트 리스트가 아닌 전체 노트리스트도 사용하기 때문에
                // 전체 노트 리스트에서도 노트를 삭제해주어야 한다.
                this.m_NoteList.Remove(note);
            }

            // 스킨을 업데이트한다.
            this.m_Skin.AnimateComboAndJudge(this.m_PlayResult.CurrentCombo, this.m_PlayResult.MaxCombo, this.m_PlayResult.CurrentScore, _judgeType);

        }

        private void ReleaseNote(LaneType _lane, int _noteIndex, double _elapsedPlayingTimeMillis)
        {
            LongNote ln = this.m_NoteListByLaneDict[_lane][_noteIndex] as LongNote;
            ReleaseNote(_lane, _noteIndex, BMSJudgeCalculator.Judge(ln.EndTimingMillis, _elapsedPlayingTimeMillis));
        }

        private void ReleaseNote(LaneType _lane, int _noteIndex, JudgementType _judgeType)
        {
            LongNote ln = this.m_NoteListByLaneDict[_lane][_noteIndex] as LongNote;

            // 노트 Release
            ln.Release(_judgeType);

            // 기록 갱신
            this.m_PlayResult.Add(_judgeType);

            // 체력 갱신
            //TODO: 조금 더 코드를 정리하고 시스템을 구체화할 필요가 있어보인다
            if (_judgeType == JudgementType.POOR || _judgeType == JudgementType.POOR)
                this.m_Health -= .085f;
            else
                this.m_Health += .035f;

            this.m_Health = Mathf.Clamp(this.m_Health, 0f, 1f);

            // 리스트에서 삭제
            this.m_NoteListByLaneDict[_lane].RemoveAt(_noteIndex);

            // 게임 종료 검사를 위해서 레인별 노트 리스트가 아닌 전체 노트리스트도 사용하기 때문에
            // 전체 노트 리스트에서도 노트를 삭제해주어야 한다.
            this.m_NoteList.Remove(ln);

            // 스킨을 업데이트한다.
            this.m_Skin.AnimateComboAndJudge(this.m_PlayResult.CurrentCombo, this.m_PlayResult.MaxCombo, this.m_PlayResult.CurrentScore, _judgeType);

        }

        /// <summary>
        /// 플레이 멈춤 여부를 설정한다. 
        /// pause값으로 true를 건네주면 게임 플레이와 음악 재생이 멈춘다. 
        /// false를 건네주면 멈추었던 게임 플레이와 음악 재생이 멈춘 부분부터 계속된다.
        /// </summary>
        /// <param name="pause">멈춤 여부를 설정하는 값이다.</param>
        public void SetPause(bool pause)
        {
            // 아래와 같이 m_IsPausing을 검사하는 조건을 넣는 이유는 
            // 구동 시 최초 1회에 한해 OnApplicationFocus(true)가 호출됨으로 인해 SetPause(false)가 호출되는데, 
            // 이 때 호출되지 않는 것을 막기 위함이다.
            if (pause == true && m_IsPaused == false)
            {
                // 플래그 값을 설정하기 이전에 ElapsedPlayingTimeMillis값을 대입해야 한다.
                // ElapsedPlayingTimeMillis는 IsPausing값에 영향을 받는 프로퍼티이기 때문이다.
                PauseTimeMillis = TimeHelper.EpochTimeMillis;
                ElapsedPlayingTimeBeforePauseMillis = ElapsedPlayingTimeMillis;

                this.m_IsPaused = true;

                //Time.timeScale = 0;
                this.m_AudioPlayer.PauseAll();
            }
            else if (pause == false && m_IsPaused == true)
            {
                this.m_IsPaused = false;

                // 게임이 시작되기 전에는 멈춘 시간을 더하여 타이밍을 맞출 필요가 없다.
                // 게임이 시작되기 전에 이 구문이 호출된다면 그 만큼 게임 플레이시간이 지연되기 때문에 불필요하다.
                if (ElapsedPlayingTimeBeforePauseMillis > 0)
                {
                    ElapsedPausedTimeMillis += TimeHelper.EpochTimeMillis - PauseTimeMillis;
                }

                //Time.timeScale = 1;
                this.m_AudioPlayer.ResumeAll();
            }

        }

        private void OnRenderObject()
        {
            if (m_IsPlaying == false)
                return;

            if (m_BarMaterial == null)
                return;

            try
            {
                // Bar 렌더링
                m_BarMaterial.SetPass(0);
                GL.PushMatrix();
                GL.MultMatrix(transform.localToWorldMatrix);

                double _timeDifferenceMillis = 0.0;
                float _worldPositionY = 0f;
                Vector3 _worldPoint1 = Vector3.zero;
                Vector3 _worldPoint2 = Vector3.zero;

                for (int i = 0; i < this.m_BarList.Count; i++)
                {
                    var item = this.m_BarList[i];
                    
                    // 지나간 시간의 Bar에 대해서는 그리지 않는다.
                    if (item.TimingMillis - ElapsedPlayingTimeMillis <= float.Epsilon)
                        continue;

                    _timeDifferenceMillis = (float)item.TimingMillis - ElapsedPlayingTimeMillis;

                    //TODO: 마디 표시줄의 가로 길이를 임시로 정해두었다. 추후에 바꿀 것.
                    _worldPositionY = (float)((_timeDifferenceMillis * m_CurrentMovespeedPerMillis) + m_Skin.JudgementPositionY);
                    _worldPoint1 = new Vector3(-700f, _worldPositionY, 0f);
                    _worldPoint2 = new Vector3(700f, _worldPositionY, 0f);

                    // 화면 바깥으로 넘어가는 Bar는 그리지 않는다.
                    if (Camera.main.IsVisible2D(_worldPoint1) == false)
                        break;

                    GL.Begin(GL.LINES);
                    GL.Color(Color.white);
                    GL.Vertex(_worldPoint1);
                    GL.Vertex(_worldPoint2);
                    GL.End();
                }

                //Debug.Log("OnRenderObject - Success");
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                GL.PopMatrix();
            }


        }


        #endregion

        #region Debug

        private void DebugDrawJudgementTime()
        {
            float koolPositionY = (float)(BMSJudgeCalculator.PgreatTimeMillis * this.m_CurrentMovespeedPerMillis) + this.m_Skin.JudgementPositionY;
            float coolPositionY = (float)(BMSJudgeCalculator.GreatTimeMillis * this.m_CurrentMovespeedPerMillis) + this.m_Skin.JudgementPositionY;
            float goodPositionY = (float)(BMSJudgeCalculator.GoodTimeMillis * this.m_CurrentMovespeedPerMillis) + this.m_Skin.JudgementPositionY;
            Debug.DrawLine(new Vector3(-1000, koolPositionY), new Vector3(1000, koolPositionY), Color.red);
            Debug.DrawLine(new Vector3(-1000, coolPositionY), new Vector3(1000, coolPositionY), Color.green);
            Debug.DrawLine(new Vector3(-1000, goodPositionY), new Vector3(1000, goodPositionY), Color.blue);
        }

        // Bar line 그리기
        //private void OnRenderObject()
        //{
        //    if (this.m_IsPlaying == false) return;

        //    GL.PushMatrix();

        //    for (int i = 0; i < this.m_BarList.Count; i++)
        //    {
        //        float y = CalculateObjectPositionY(this.m_BarList[i].TimingMillis, ElapsedPlayingTimeMillis, m_CurrentMovespeedPerMillis);
        //        // 판정선 위치가 대략 0.25정도 되는데, 이 이하로 떨어지면 어짜피 안보이기 때문에 그리지 않는다.
        //        if (y < -12)
        //        {
        //            this.m_BarList.RemoveAt(i);
        //            i--;
        //            continue;
        //        }
        //        // 너무 위에 있어도 그릴 필요가 없다.
        //        if (y > 12.0f) break;

        //        GL.Begin(GL.LINES);
        //        GL.Color(Color.white);

        //        GL.Vertex(new Vector3(-12.12917f, y, 0.0f));
        //        GL.Vertex(new Vector3(-5.129167f, y, 0.0f));

        //        GL.End();
        //    }

        //    GL.PopMatrix();

        //}


        #endregion

    }

}