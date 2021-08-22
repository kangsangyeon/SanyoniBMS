
namespace SanyoniBMS
{

    [System.Serializable]
    public class BMSPlayResult
    {
        public int PlayerName;

        public int CurrentCombo;
        public int MaxCombo;
        public int CurrentScore;

        public int PgreatCount;
        public int GreatCount;
        public int GoodCount;
        public int BadCount;
        public int PoorCount;

        public bool IsAutoPlay = false;

        public void Add(JudgementType type)
        {
            switch (type)
            {
                case JudgementType.PGREAT: PgreatCount++; break;
                case JudgementType.GREAT: GreatCount++; break;
                case JudgementType.GOOD: GoodCount++; break;
                case JudgementType.BAD: BadCount++; break;
                case JudgementType.POOR: PoorCount++; break;
            }

            // 판정이 None 또는 Poor만 아니면 콤보와 점수를 추가한다.
            if (type != JudgementType.None && type != JudgementType.POOR)
            {
                this.CurrentScore = this.CurrentScore + BMSJudgeCalculator.CalculateScore(this.CurrentCombo, type);

                this.CurrentCombo++;
                if (this.CurrentCombo > this.MaxCombo) this.MaxCombo = this.CurrentCombo;
            }
            else
            {
                this.CurrentCombo = 0;
            }

        }

        public override string ToString()
        {
            return string.Format("{{Pgreat: {0}\tGreat: {1}\tGood: {2}\tBad: {3}\tPoor: {4}}}", PgreatCount, GreatCount, GoodCount, BadCount, PoorCount);
        }

    }

}