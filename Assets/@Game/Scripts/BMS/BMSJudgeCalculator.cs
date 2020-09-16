using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public class BMSJudgeCalculator
    {

        private const double PgreatTimeDivider = 8;   // 박자(beat)시간 / n
        private const double GreatTimeDivider = 6;
        private const double GoodTimeDivider = 4;
        private const double BadTimeDivider = 2;
        private const double PoorTimeDivider = 1;

        private const int PgreatBaseScore = 320;
        private const int GreatBaseScore = 300;
        private const int GoodBaseScore = 150;
        private const int BadBaseScore = 41;

        // 가변 타이밍 체계
        //public static double GreatTimeMillis
        //{
        //    get
        //    {
        //        if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying) return BMSPlayer.Instance.BeatDurationMillis / GreatTimeDivider;
        //        else return -1;

        //    }
        //}
        //public static double PgreatTimeMillis
        //{
        //    get
        //    {
        //        if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying) return BMSPlayer.Instance.BeatDurationMillis / PgreatTimeDivider;
        //        else return -1;
        //    }
        //}
        //public static double GoodTimeMillis
        //{
        //    get
        //    {
        //        if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying) return BMSPlayer.Instance.BeatDurationMillis / GoodTimeDivider;
        //        else return -1;
        //    }
        //}
        //public static double BadTimeMillis
        //{
        //    get
        //    {
        //        if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying) return BMSPlayer.Instance.BeatDurationMillis / BadTimeDivider;
        //        else return -1;
        //    }
        //}
        //public static double PoorTimeMillis
        //{
        //    get
        //    {
        //        if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying) return BMSPlayer.Instance.BeatDurationMillis / PoorTimeDivider;
        //        else return -1;
        //    }
        //}

        /**** 고정 타이밍 체계 *****/
        public static double PgreatTimeMillis = 20;
        public static double GreatTimeMillis = 40;
        public static double GoodTimeMillis = 80;
        public static double BadTimeMillis = 120;
        public static double PoorTimeMillis = 160;

        public static JudgementType Judge(double noteTimingMillis, double currentTimeMillis)
        {
            double timeDiff = System.Math.Abs(currentTimeMillis - noteTimingMillis);
            if (timeDiff <= PgreatTimeMillis) return JudgementType.PGREAT;
            else if (timeDiff <= GreatTimeMillis) return JudgementType.GREAT;
            else if (timeDiff <= GoodTimeMillis) return JudgementType.GREAT;
            else if (timeDiff <= BadTimeMillis) return JudgementType.BAD;
            else if (timeDiff <= PoorTimeMillis) return JudgementType.POOR;
            else return JudgementType.None;
        }

        /// <summary>
        /// 노트를 침으로써 얻는 점수를 계산한다. 점수는 콤보와 판정 단계에 따라 비례하여 계산된다.
        /// </summary>
        /// <param name="currentCombo">현재 콤보를 받는다. 이 수치는 이 노트의 콤보 누적을 하지 않은 바로 이전의 수치이다.
        /// 예를 들어, 내가 154콤보인 상태에서 Great판정으로 노트를 쳤다면 이 수치는 바로 직전의 노트를 콤보에 포함하지 않은 154로 전달되어야 한다.</param>
        /// <param name="judgeType">바로 직전에 친 노트를 어떤 판정으로 쳤는지에 대한 정보이다.</param>
        /// <returns>현재 노트를 침으로써 얻는 score를 리턴한다. 이 값을 현재 점수에 더해야 한다.</returns>
        public static int CalculateScore(int currentCombo, JudgementType judgeType)
        {
            switch (judgeType)
            {
                case JudgementType.PGREAT: return PgreatBaseScore + (currentCombo / 50);
                case JudgementType.GREAT: return GreatBaseScore + (currentCombo / 50);
                case JudgementType.GOOD: return GoodBaseScore + (currentCombo / 50);
                case JudgementType.BAD: return BadBaseScore + (currentCombo / 50);
                default: return 0;
            }
        }

    }

}