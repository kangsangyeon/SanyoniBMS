using UnityEngine;

namespace SanyoniLib.UnityEngineHelper
{

    public static class DebugHelper
    {
        public static void LogError(System.Exception e, string message = null)
        {
            if (message == null) message = string.Empty;

            Debug.LogErrorFormat("<color=#ff0000>{0}</Color>\n<color=#0000ff>{1}</color>\n\n{2}\n", e.Message, e.StackTrace, message);
        }

        public static void LogErrorFormat(System.Exception e, string format = null, params object[] args)
        {
            string message = format == null ? string.Empty : string.Format(format, args);

            Debug.LogErrorFormat("<color=#ff0000>{0}</Color>\n<color=#0000ff>{1}</color>\n\n{2}\n", e.Message, e.StackTrace, message);
        }

    }

}