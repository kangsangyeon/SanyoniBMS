using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public class PlayerData
    {
        #region Properties
        public int ID => m_Name.GetHashCode();
        public int AllPlayCount
        {
            get
            {
                if (KeyModePlayCountDict != null)
                {
                    int allCount = 0;
                    foreach (var count in KeyModePlayCountDict.Values)
                        allCount += count;

                    return allCount;
                }
                else
                {
                    return -1;
                }

            }
        }

        public string Name
        {
            get => m_Name;
            private set => m_Name = value;
        }
        public int Level
        {
            get => m_Level;
            private set => m_Level = value;
        }
        public int Exp
        {
            get => m_Exp;
            private set => m_Exp = value;
        }
        public Dictionary<KeyMode, int> KeyModePlayCountDict => m_KeyModePlayCountDict;

        #endregion

        #region Private Variables

        private string m_Name = "Noname";
        private int m_Level = 1;
        private int m_Exp = 0;

        private Dictionary<KeyMode, int> m_KeyModePlayCountDict = new Dictionary<KeyMode, int>()
        {
            [KeyMode.SP4] = 0,
            [KeyMode.SP5] = 0,
            [KeyMode.SP6] = 0,
            [KeyMode.SP7] = 0,
            [KeyMode.DP10] = 0,
            [KeyMode.DP14] = 0
        };

        #endregion

    }

}


