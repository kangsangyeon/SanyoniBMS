using UnityEngine;
using System.Collections;

namespace SanyoniBMS
{
    #region BMSData
    public enum KeyMode
    {
        None = 0,
        SP4 = 1, SP5 = 2, SP6 = 3, SP7 = 4,
        DP10 = 11, DP14
    }
    #endregion

    #region BMSObject
    public enum ChannelType
    {
        NONE = -01,
        // Event
        EVENT_BGM = 01, EVENT_CHANGE_BAR_LENGTH = 02, EVENT_CHANGE_BPM = 03, EVENT_BGA = 04, EVENT_POORBGA = 06, EVENT_EXBPM = 08, EVENT_STOP = 09,
        // Player1
        PLAYER1_NOTE1 = 11, PLAYER1_NOTE2 = 12, PLAYER1_NOTE3 = 13, PLAYER1_NOTE4 = 14, PLAYER1_NOTE5 = 15,
        PLAYER1_SCRATCH = 16, PLAYER1_PEDAL = 17, PLAYER1_NOTE6 = 18, PLAYER1_NOTE7 = 19,
        // Player2

        // Player1 LN
        PLAYER1_LN1 = 51, PLAYER1_LN2 = 52, PLAYER1_LN3 = 53, PLAYER1_LN4 = 54, PLAYER1_LN5 = 55,
        PLAYER1_LNSCRATCH = 56, PLAYER1_LNPEDAL = 57, PLAYER1_LN6 = 58, PLAYER1_LN7 = 59,
        // Player2 LN

        // SYSTEM
        SYSTEM_EVENT_BAR = 101,
    }

    public enum ChannelFirstType
    {
        NONE = -1,
        EVENT = 0, PLAYER1_NOTE = 1, PLAYER2_NOTE = 2, PLAYER1_INVISIBLE_NOTE = 3, PLAYER2_INVISIBLE_NOTE = 4,
        PLAYER1_LONGNOTE = 5, PLAYER2_LONGNOTE = 6,
    }

    public enum LaneType
    {
        NONE = -1,
        NOTE1 = 1, NOTE2 = 2, NOTE3 = 3, NOTE4 = 4, NOTE5 = 5,
        SCRATCH = 6, PEDAL = 7, NOTE6 = 8, NOTE7 = 9
    }

    #endregion

    #region BMSJudgeCalculator

    public enum JudgementType
    {
        None = 0, POOR = 1, BAD = 2, GOOD = 3, GREAT = 4, PGREAT = 5
    }

    #endregion

    #region BMSPlayer

    public enum KeyPressState
    {
        NONE = 0,
        DOWN = 1, HOLD = 2, RELEASE = 3
    }

    public enum BMSPlayerState
    {
        None = 0, Playing = 3, Paused = 4, Finished = 5
    }

    #endregion



}
