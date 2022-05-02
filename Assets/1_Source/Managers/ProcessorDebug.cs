using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class ProcessorDebug
    {

        public static void Log(object source, string message)
        {
            if (!Debug.isDebugBuild)
                return;

            Debug.Log(FormatLog(source, message));
        }
        public static void LogWarning(object source, string message)
        {
            if (!Debug.isDebugBuild)
                return;

            Debug.LogWarning(FormatLog(source, message));
        }
        public static void LogError(object source, string message)
        {
            if (!Debug.isDebugBuild)
                return;

            Debug.LogError(FormatLog(source, message));
        }

        private static string FormatLog(object source, string message)
        {
            string resultLog = DateTime.Now + " ::: " + source + " ::: " + message;
            return resultLog;
        }
    }
}
