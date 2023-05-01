using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class Utilities
{
    public const int DEBUG_TOPIC_PLAYER_EVENTS = 0;
    public const int DEBUG_TOPIC_MATCH_EVENTS = 1;
    public const int DEBUG_TOPIC_PLAYERACTION = 2;
    public const int DEBUG_TOPIC_REPLAY = 3;
    public const int DEBUG_TOPIC_UNEXPECTED_SITUATIONS = 4;

    private static List<int> topicsActive = new List<int>() { };
//    private static List<int> topicsActive = new List<int>() { DEBUG_TOPIC_MATCH_EVENTS, DEBUG_TOPIC_PLAYERACTION, DEBUG_TOPIC_PLAYER_EVENTS, DEBUG_TOPIC_UNEXPECTED_SITUATIONS };

    public static void LogToFile(string filename, string value)
    {
        using StreamWriter file = new(filename, append: true);
        file.WriteLineAsync(value);
    }

    public static void Log(string text, int topic)
    {
        if (topicsActive.Contains(topic))
        {
            Debug.Log(text);
        }
    }
}
