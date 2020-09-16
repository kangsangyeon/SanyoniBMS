using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SanyoniBMS
{

    public class BMSObject : System.IComparable<BMSObject>, System.ICloneable, ISerializable
    {

        public int BarIndex;
        public double Beat;
        public double TimingMillis = -1;
        public ChannelType ChannelType;
        public virtual int SortingOrder { get; }


        public BMSObject() { }
        public BMSObject(int _barIndex, double _beat, ChannelType _channelType)
        {
            this.BarIndex = _barIndex;
            this.Beat = _beat;
            this.ChannelType = _channelType;
        }

        public double TimingSeconds { get { return TimingMillis / 1000; } }
        public bool IsEventChannel() { return GetChannelFirstType() == ChannelFirstType.EVENT; }
        public bool IsObjectChannel()
        {
            bool isObjectChannel = GetChannelFirstType() == ChannelFirstType.PLAYER1_NOTE ||
                                GetChannelFirstType() == ChannelFirstType.PLAYER2_NOTE ||
                                GetChannelFirstType() == ChannelFirstType.PLAYER1_INVISIBLE_NOTE ||
                                GetChannelFirstType() == ChannelFirstType.PLAYER2_INVISIBLE_NOTE ||
                                GetChannelFirstType() == ChannelFirstType.PLAYER1_LONGNOTE ||
                                GetChannelFirstType() == ChannelFirstType.PLAYER2_LONGNOTE;
            return isObjectChannel;
        }
        public ChannelFirstType GetChannelFirstType() { return (ChannelFirstType)GetChannel1(); }
        protected int GetChannel1() { return (int)ChannelType / 10; }
        protected int GetChannel2() { return (int)ChannelType % 10; }

        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType);
        }

        // 정렬 우선순위: TimingMillis > BarIndex > Beat > SortingOrder > ( > Channel1 > Channel2)
        // Channel1과 Channel2 기준 정렬은 게임 로직에 전혀 상관없다. 디버깅용으로만 사용한다.
        public virtual int CompareTo(BMSObject other)
        {
            // TimingMillis가 계산되어 있는 상태라면 이 값으로 비교한다.
            if (this.TimingMillis >= 0 && other.TimingMillis >= 0)
            {
                if (this.TimingMillis > other.TimingMillis) return 1;
                else if (this.TimingMillis < other.TimingMillis) return -1;
            }

            if (this.BarIndex > other.BarIndex) return 1;
            else if (this.BarIndex < other.BarIndex) return -1;

            else if (this.Beat > other.Beat) return 1;
            else if (this.Beat < other.Beat) return -1;

            else if (this.SortingOrder > other.SortingOrder) return 1;
            else if (this.SortingOrder < other.SortingOrder) return -1;

            //else if (this.GetChannel1() > other.GetChannel1()) return 1;
            //else if (this.GetChannel1() < other.GetChannel1()) return -1;

            //else if (this.GetChannel2() > other.GetChannel2()) return 1;
            //else if (this.GetChannel2() < other.GetChannel2()) return -1;

            else return 0;
        }

        public virtual object Clone() { return new BMSObject(this.BarIndex, this.Beat, this.ChannelType); }

        #region Serialization
        private const string BarIndexText = "BarIndex";
        private const string BeatText = "Beat";
        private const string TimingMillisText = "TimingMillis";
        private const string ChannelTypeText = "ChannelType";

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(BarIndexText, this.BarIndex, typeof(int));
            info.AddValue(BeatText, this.Beat, typeof(double));
            info.AddValue(TimingMillisText, this.TimingMillis, typeof(double));
            info.AddValue(ChannelTypeText, this.ChannelType, typeof(ChannelType));
        }

        public BMSObject(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            this.BarIndex = (int)info.GetValue(BarIndexText, typeof(int));
            this.Beat = (double)info.GetValue(BeatText, typeof(double));
            this.TimingMillis = (double)info.GetValue(TimingMillisText, typeof(double));
            this.ChannelType = (ChannelType)info.GetValue(ChannelTypeText, typeof(ChannelType));
        }

        #endregion
    }

    public class Note : BMSObject
    {
        public int KeySound;
        public GameObject Model;

        public Note() { }
        public Note(int _barIndex, double _beat, ChannelType _channelType, int _keySound) : base(_barIndex, _beat, _channelType)
        {
            this.ChannelType = _channelType;
            this.KeySound = _keySound;
        }

        public LaneType GetLaneType() { return (LaneType)GetChannel2(); }
        public int GetPlayerIndex()
        {
            if (GetChannelFirstType() == ChannelFirstType.PLAYER1_NOTE ||
                    GetChannelFirstType() == ChannelFirstType.PLAYER1_LONGNOTE ||
                    GetChannelFirstType() == ChannelFirstType.PLAYER1_INVISIBLE_NOTE)
                return 1;
            else if (GetChannelFirstType() == ChannelFirstType.PLAYER2_NOTE ||
                GetChannelFirstType() == ChannelFirstType.PLAYER2_LONGNOTE ||
                GetChannelFirstType() == ChannelFirstType.PLAYER2_INVISIBLE_NOTE)
                return 2;
            else
                return -1;
        }

        public virtual void Hit(JudgementType type)
        {
            // 키사운드 재생은 BMSPlayer에서 직접 재생하도록 변경했다.
            // 키 사운드 재생
            //BMSPlayer.Instance.m_AudioPlayer.TryPlayInDictionary(this.KeySound);

            // 게임 오브젝트 숨김
            this.Model.SetActive(false);

            //TODO: 판정에 따라 애니메이션 재생
            switch (type) { }

        }



        #region Override

        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tKeySound: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.KeySound);
        }

        public override object Clone()
        {
            Note newNote = new Note(this.BarIndex, this.Beat, this.ChannelType, this.KeySound);
            newNote.TimingMillis = this.TimingMillis;
            return newNote;
        }

        #endregion

        #region Serialization
        private const string KeySoundText = "KeySoundText";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(KeySoundText, this.KeySound, typeof(int));
        }

        public Note(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.KeySound = (int)info.GetValue(KeySoundText, typeof(int));
        }

        #endregion

    }

    public class LongNote : Note
    {
        public int EndBarIndex;
        public double EndBeat;
        public double EndTimingMillis;
        public bool Pressed;
        public GameObject BodyModel;
        public GameObject TailModel;

        public LongNote() { }
        public LongNote(int _barIndex, double _beat, ChannelType _channelType, int _keySound) : base(_barIndex, _beat, _channelType, _keySound) { }
        public LongNote(int _barIndex, double _beat, ChannelType _channelType, int _keySound, int _endBarIndex, double _endBeat) : base(_barIndex, _beat, _channelType, _keySound)
        {
            this.EndBarIndex = _endBarIndex;
            this.EndBeat = _endBeat;
        }

        public override void Hit(JudgementType type)
        {
            this.Pressed = true;

            // 판정 타입에 따른 애니메이션
            switch (type) { }

        }

        public void Release(JudgementType type)
        {
            if (this.Model != null) this.Model.SetActive(false);
            if (this.BodyModel != null) this.BodyModel.SetActive(false);
            if (this.TailModel != null) this.TailModel.SetActive(false);

        }

        #region Override

        public override object Clone()
        {
            LongNote newNote = new LongNote(this.BarIndex, this.Beat, this.ChannelType, this.KeySound, this.EndBarIndex, this.EndBeat);
            newNote.TimingMillis = this.TimingMillis;
            newNote.EndTimingMillis = this.EndTimingMillis;
            return newNote;
        }

        #endregion

        #region Serialization
        private const string EndBarIndexText = "EndBarIndex";
        private const string EndBeatText = "EndBeat";
        private const string EndTimingMillisText = "EndTimingMillis";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(EndBarIndexText, this.EndBarIndex, typeof(int));
            info.AddValue(EndBeatText, this.EndBeat, typeof(double));
            info.AddValue(EndTimingMillisText, this.EndTimingMillis, typeof(double));
        }

        public LongNote(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.EndBarIndex = (int)info.GetValue(EndBarIndexText, typeof(int));
            this.EndBeat = (double)info.GetValue(EndBeatText, typeof(double));
            this.EndTimingMillis = (double)info.GetValue(EndTimingMillisText, typeof(double));
        }

        #endregion

    }

    public class BGMEvent : BMSObject
    {
        public int KeySound;

        public BGMEvent() { }
        public BGMEvent(int _barIndex, double _beat, ChannelType _channelType, int _keySound) : base(_barIndex, _beat, _channelType)
        {
            this.KeySound = _keySound;
        }

        #region Override

        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tKeySound: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.KeySound);
        }

        public override object Clone()
        {
            BGMEvent newEvent = new BGMEvent(this.BarIndex, this.Beat, this.ChannelType, this.KeySound);
            newEvent.TimingMillis = this.TimingMillis;
            return newEvent;
        }

        #endregion

        #region Serialization
        private const string KeySoundText = "KeySound";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(KeySoundText, this.KeySound, typeof(int));
        }

        public BGMEvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.KeySound = (int)info.GetValue(KeySoundText, typeof(int));
        }

        #endregion

    }

    public class BPMEvent : BMSObject
    {
        public double BPM;

        public BPMEvent() { }
        public BPMEvent(int _barIndex, double _beat, ChannelType _channelType, double _bpm) : base(_barIndex, _beat, _channelType)
        {
            this.BPM = _bpm;
        }

        #region Override

        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tBPM: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.BPM);
        }

        public override object Clone()
        {
            BPMEvent newEvent = new BPMEvent(this.BarIndex, this.Beat, this.ChannelType, this.BPM);
            newEvent.TimingMillis = this.TimingMillis;
            return newEvent;
        }

        #endregion

        #region Serialization
        private const string BPMText = "BPM";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(BPMText, this.BPM, typeof(double));
        }

        public BPMEvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.BPM = (double)info.GetValue(BPMText, typeof(double));
        }

        #endregion

    }

    public class StopEvent : BMSObject
    {
        public int Key;
        public override int SortingOrder => -5;

        public StopEvent() { }
        public StopEvent(int _barIndex, double _beat, ChannelType _channelType, int _key) : base(_barIndex, _beat, _channelType)
        {
            this.Key = _key;
        }

        #region Override
        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tKey: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.Key);
        }

        public override object Clone()
        {
            StopEvent newEvent = new StopEvent(this.BarIndex, this.Beat, this.ChannelType, this.Key);
            newEvent.TimingMillis = this.TimingMillis;
            return newEvent;
        }

        #endregion


        #region Serialization
        private const string KeyText = "Key";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(KeyText, this.Key, typeof(int));
        }

        public StopEvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Key = (int)info.GetValue(KeyText, typeof(int));
        }

        #endregion

    }

    public class ChangeBarLengthEvent : BMSObject
    {
        public double Multiplier;
        public override int SortingOrder => -10;


        public ChangeBarLengthEvent() { }
        public ChangeBarLengthEvent(int _barIndex, double _beat, ChannelType _channelType, double _multiplier) : base(_barIndex, _beat, _channelType)
        {
            this.Multiplier = _multiplier;
        }

        #region Override
        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tMultiplier: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.Multiplier);
        }

        public override object Clone()
        {
            ChangeBarLengthEvent newEvent = new ChangeBarLengthEvent(this.BarIndex, this.Beat, this.ChannelType, this.Multiplier);
            newEvent.TimingMillis = this.TimingMillis;
            return newEvent;
        }
        #endregion

        #region Serialization
        private const string MultiplierText = "Multiplier";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(MultiplierText, this.Multiplier, typeof(double));
        }

        public ChangeBarLengthEvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Multiplier = (double)info.GetValue(MultiplierText, typeof(double));
        }

        #endregion

    }

    public class BGAEvent : BMSObject
    {
        public int Key;

        public BGAEvent() { }
        public BGAEvent(int _barIndex, double _beat, ChannelType _channelType, int _key) : base(_barIndex, _beat, _channelType)
        {
            this.Key = _key;
        }

        #region Override
        public override string ToString()
        {
            return string.Format("{{BarIndex: {0}\tBeat: {1}\tTimingMillis: {2}\tChannelType: {3}\tKey: {4}}}", this.BarIndex, this.Beat, this.TimingMillis, this.ChannelType, this.Key);
        }

        public override object Clone()
        {
            BGAEvent newEvent = new BGAEvent(this.BarIndex, this.Beat, this.ChannelType, this.Key);
            newEvent.TimingMillis = this.TimingMillis;
            return newEvent;
        }
        #endregion

        #region Serialization
        private const string KeyText = "Key";

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(KeyText, this.Key, typeof(int));
        }

        public BGAEvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.Key = (int)info.GetValue(KeyText, typeof(int));
        }

        #endregion
    }


    /* 프로그램 자체 모델 */
    public class LongNoteTail : Note
    {

        public LongNoteTail() { }
        public LongNoteTail(int _barIndex, double _beat, ChannelType _channelType)
        {
            this.BarIndex = _barIndex;
            this.Beat = _beat;
            this.ChannelType = _channelType;
        }

    }

    public class BarEvent : BMSObject
    {
        public override int SortingOrder => -20;

        public BarEvent() { }
        public BarEvent(int _barIndex, double _beat, ChannelType _channelType) : base(_barIndex, _beat, _channelType) { }


        #region Override

        public override object Clone()
        {
            BarEvent newBar = new BarEvent(this.BarIndex, this.Beat, this.ChannelType);
            newBar.TimingMillis = this.TimingMillis;
            return newBar;
        }

        #endregion

    }

}