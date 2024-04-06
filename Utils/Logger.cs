using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGCS.Utils
{
    public static class NLogger
    {
        private static readonly Dictionary<string, DateTime> _timesDictionary = new();

        public static void Log(object log, LogColor color = LogColor.white)
        {
            Debug.Log($"<color={color}><><> {log}</color>");
        }

        public static void SetTime(string name)
        {
            Log($"Task starts: {name}",LogColor.orange);
            if (_timesDictionary.ContainsKey(name))
            {
                _timesDictionary[name] = DateTime.Now;
                return;
            }

            _timesDictionary.Add(name, DateTime.Now);
        }

        public static void ShowTimeDifference(string name)
        {
            if (!_timesDictionary.ContainsKey(name))
            {
                LogError($"{name} is not set before!");
                return;
            }

            Log($"Task finished: {name} - Time {(DateTime.Now - _timesDictionary[name]).TotalSeconds}", LogColor.green);
        }

        public static void LogError(string log)
        {
            Debug.LogError($"<><> Error!\t{log}");
        }
    }

    public enum LogColor
    {
        red,
        yellow,
        blue,
        orange,
        black,
        white,
        green
    }
}