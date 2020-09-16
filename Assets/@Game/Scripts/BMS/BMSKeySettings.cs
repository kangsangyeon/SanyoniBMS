using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using SanyoniLib.UnityEngineHelper;


namespace SanyoniBMS
{

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public abstract class BMSKeySettingsBase : ISerializable
    {
        [SerializeField] protected Dictionary<KeyCode, LaneType> m_KeyCodeLaneTypeDict = new Dictionary<KeyCode, LaneType>();

        public BMSKeySettingsBase() { }

        public virtual KeyCode GetKeycodeByLane(LaneType type)
        {
            try
            {
                return this.m_KeyCodeLaneTypeDict.Single(x => x.Value == type).Key;
            }
            catch (System.Exception e)
            {
                DebugHelper.LogError(e);
                return KeyCode.None;
            }
        }

        public virtual LaneType GetLaneTypeByKeyCode(KeyCode _keyCode)
        {
            if (this.m_KeyCodeLaneTypeDict.ContainsKey(_keyCode)) return this.m_KeyCodeLaneTypeDict[_keyCode];
            else
            {
                Debug.LogErrorFormat("{0}는 할당된 키가 아닙니다.", _keyCode);
                return LaneType.NONE;
            }

        }

        public virtual void Set(LaneType _laneType, KeyCode _keyCode)
        {

            // 파라미터로 받은 laneType이 기존 value로 사용되고 있었다면 그 항목을 없앤다.
            // laneType마다 하나의 키만 할당할 수 있기 때문이다.
            try
            {
                var prevItem = this.m_KeyCodeLaneTypeDict.Single(x => x.Value == _laneType);
                this.m_KeyCodeLaneTypeDict.Remove(prevItem.Key);
            }
            catch (System.Exception e)
            {
                DebugHelper.LogError(e);
            }

            this.m_KeyCodeLaneTypeDict.Add(_keyCode, _laneType);

        }

        protected abstract void OnValidate();

        public virtual KeyCode[] GetKeys()
        {
            return this.m_KeyCodeLaneTypeDict.Keys.ToArray();
        }

        public virtual LaneType[] GetLanes()
        {
            return this.m_KeyCodeLaneTypeDict.Values.ToArray();
        }

        #region Serialization
        private const string KeyCodeLaneTypeDictText = "KeyCodeLaneTypeDict";

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(KeyCodeLaneTypeDictText, this.m_KeyCodeLaneTypeDict, typeof(Dictionary<KeyCode, LaneType>));
        }

        public BMSKeySettingsBase(SerializationInfo info, StreamingContext context)
        {
            this.m_KeyCodeLaneTypeDict = (Dictionary<KeyCode, LaneType>)info.GetValue(KeyCodeLaneTypeDictText, typeof(Dictionary<KeyCode, LaneType>));
        }

        #endregion

    }

    [System.Serializable]
    public class BMS5KeySettings : BMSKeySettingsBase
    {
        [SerializeField] private const KeyCode DefaultScratchLaneKey = KeyCode.LeftShift;
        [SerializeField] private const KeyCode DefaultNote1LaneKey = KeyCode.D;
        [SerializeField] private const KeyCode DefaultNote2LaneKey = KeyCode.F;
        [SerializeField] private const KeyCode DefaultNote3LaneKey = KeyCode.J;
        [SerializeField] private const KeyCode DefaultNote4LaneKey = KeyCode.K;
        [SerializeField] private const KeyCode DefaultNote5LaneKey = KeyCode.L;

        public BMS5KeySettings()
        {
            this.m_KeyCodeLaneTypeDict.Add(DefaultScratchLaneKey, LaneType.SCRATCH);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote1LaneKey, LaneType.NOTE1);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote2LaneKey, LaneType.NOTE2);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote3LaneKey, LaneType.NOTE3);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote4LaneKey, LaneType.NOTE4);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote5LaneKey, LaneType.NOTE5);
        }

        public override void Set(LaneType _laneType, KeyCode _keyCode)
        {
            base.Set(_laneType, _keyCode);

            //switch (_laneType)
            //{
            //    case LaneType.SCRATCH: DefaultScratchLaneKey = _keyCode; break;
            //    case LaneType.NOTE1: DefaultNote1LaneKey = _keyCode; break;
            //    case LaneType.NOTE2: DefaultNote2LaneKey = _keyCode; break;
            //    case LaneType.NOTE3: DefaultNote3LaneKey = _keyCode; break;
            //    case LaneType.NOTE4: DefaultNote4LaneKey = _keyCode; break;
            //    case LaneType.NOTE5: DefaultNote5LaneKey = _keyCode; break;
            //}

        }

        protected override void OnValidate()
        {
            Set(LaneType.SCRATCH, DefaultScratchLaneKey);
            Set(LaneType.NOTE1, DefaultNote1LaneKey);
            Set(LaneType.NOTE2, DefaultNote2LaneKey);
            Set(LaneType.NOTE3, DefaultNote3LaneKey);
            Set(LaneType.NOTE4, DefaultNote4LaneKey);
            Set(LaneType.NOTE5, DefaultNote5LaneKey);
        }

        #region Serialization
        public BMS5KeySettings(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

    }

    [System.Serializable]
    public class BMS7KeySettings : BMSKeySettingsBase
    {

        [SerializeField] private const KeyCode DefaultScratchLaneKey = KeyCode.LeftShift;
        [SerializeField] private const KeyCode DefaultNote1LaneKey = KeyCode.S;
        [SerializeField] private const KeyCode DefaultNote2LaneKey = KeyCode.D;
        [SerializeField] private const KeyCode DefaultNote3LaneKey = KeyCode.F;
        [SerializeField] private const KeyCode DefaultNote4LaneKey = KeyCode.Space;
        [SerializeField] private const KeyCode DefaultNote5LaneKey = KeyCode.J;
        [SerializeField] private const KeyCode DefaultNote6LaneKey = KeyCode.K;
        [SerializeField] private const KeyCode DefaultNote7LaneKey = KeyCode.L;

        public BMS7KeySettings()
        {
            this.m_KeyCodeLaneTypeDict.Add(DefaultScratchLaneKey, LaneType.SCRATCH);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote1LaneKey, LaneType.NOTE1);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote2LaneKey, LaneType.NOTE2);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote3LaneKey, LaneType.NOTE3);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote4LaneKey, LaneType.NOTE4);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote5LaneKey, LaneType.NOTE5);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote6LaneKey, LaneType.NOTE6);
            this.m_KeyCodeLaneTypeDict.Add(DefaultNote7LaneKey, LaneType.NOTE7);
        }

        public override void Set(LaneType _laneType, KeyCode _keyCode)
        {
            base.Set(_laneType, _keyCode);

            //switch (_laneType)
            //{
            //    case LaneType.SCRATCH: DefaultScratchLaneKey = _keyCode; break;
            //    case LaneType.NOTE1: DefaultNote1LaneKey = _keyCode; break;
            //    case LaneType.NOTE2: DefaultNote2LaneKey = _keyCode; break;
            //    case LaneType.NOTE3: DefaultNote3LaneKey = _keyCode; break;
            //    case LaneType.NOTE4: DefaultNote4LaneKey = _keyCode; break;
            //    case LaneType.NOTE5: DefaultNote5LaneKey = _keyCode; break;
            //    case LaneType.NOTE6: DefaultNote6LaneKey = _keyCode; break;
            //    case LaneType.NOTE7: DefaultNote7LaneKey = _keyCode; break;
            //}

        }

        protected override void OnValidate()
        {
            Set(LaneType.SCRATCH, DefaultScratchLaneKey);
            Set(LaneType.NOTE1, DefaultNote1LaneKey);
            Set(LaneType.NOTE2, DefaultNote2LaneKey);
            Set(LaneType.NOTE3, DefaultNote3LaneKey);
            Set(LaneType.NOTE4, DefaultNote4LaneKey);
            Set(LaneType.NOTE5, DefaultNote5LaneKey);
            Set(LaneType.NOTE6, DefaultNote5LaneKey);
            Set(LaneType.NOTE7, DefaultNote5LaneKey);
        }

        #region Serialization
        public BMS7KeySettings(SerializationInfo info, StreamingContext context) : base(info, context) { }
        #endregion

    }

}