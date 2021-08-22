using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SanyoniBMS
{
    public static class BMSHelper
    {

        // https://blog.naver.com/perditus93/220171603566 포스트 참고
        public static float GetDefaultTotalValue(int noteCount)
        {
            if (noteCount <= 338) return 260f;
            else if (noteCount <= 364) return 266.5f;
            else if (noteCount <= 392) return 279.5f;
            else if (noteCount <= 421) return 292.5f;
            else if (noteCount <= 452) return 305.5f;
            else if (noteCount <= 485) return 318.5f;
            else if (noteCount <= 520) return 331.5f;
            else if (noteCount <= 557) return 344.5f;
            else if (noteCount <= 597) return 357.5f;
            else if (noteCount <= 639) return 370.5f;
            else if (noteCount <= 684) return 383.5f;
            else if (noteCount <= 733) return 396.5f;
            else if (noteCount <= 785) return 409.5f;
            else if (noteCount <= 841) return 422.5f;
            else if (noteCount <= 902) return 435.5f;
            else if (noteCount <= 968) return 448.5f;
            else if (noteCount <= 1040) return 461.5f;
            else if (noteCount <= 1119) return 474.5f;
            else if (noteCount <= 1205) return 487.5f;
            else if (noteCount <= 1300) return 500.5f;
            else if (noteCount <= 1405) return 513.5f;
            else if (noteCount <= 1523) return 526.5f;
            else if (noteCount <= 1655) return 539.5f;
            else if (noteCount <= 1803) return 552.5f;
            else if (noteCount <= 1972) return 565.5f;
            else if (noteCount <= 2167) return 578.5f;
            else if (noteCount <= 2392) return 591.5f;
            else if (noteCount <= 2657) return 604.5f;
            else if (noteCount <= 2971) return 617.5f;
            else if (noteCount <= 3353) return 630.5f;
            else if (noteCount <= 3824) return 643.5f;
            else if (noteCount <= 4420) return 656.5f;
            else if (noteCount <= 5200) return 669.5f;
            else if (noteCount <= 6264) return 682.5f;
            else if (noteCount <= 7800) return 695.5f;
            else return 708.5f;
        }

    }

}