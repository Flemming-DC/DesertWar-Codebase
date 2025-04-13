using System;
using System.IO;
using UnityEngine;

public static class Logger
{
    static string editorFilePath = @"C:\Users\Flemming\Documents\Spil - hjemmelavede\Unity\RTS\Assets\Log File.txt";
    static string executableFilePath = @".\Log File.txt";
    static bool hasLogged = false;

    public static void LogAndShow(string message)
    {
        Log(message);
        Debug.Log(message);
        HintManager.SetHint(message);
    }

    public static void Log(string message)
    {
        try
        {
            string filePath = Application.isEditor ? editorFilePath : executableFilePath;
            if (!hasLogged)
            {
                hasLogged = true;
                string newGameMessage = "------------ New Player or New Game -------------";
                string currentTime = DateTime.Now.ToString("MM/dd/yyyy h:mm:ss tt");
                File.AppendAllText(filePath, $"\n\n{newGameMessage}\n");
                File.AppendAllText(filePath, $"Time: {currentTime}\n");
            }
            File.AppendAllText(filePath, message + "\n");
        }
        catch { }

    }



}
