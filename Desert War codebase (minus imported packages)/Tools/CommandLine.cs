using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public static class CommandLine
{
    static List<Process> processes;

    public static Process Run(string command, bool? showCmd = null)
    {
        if (showCmd == null)
            showCmd = Application.isEditor;
        if (processes == null)
            processes = new List<Process>();

        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.WindowStyle = (bool)showCmd ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
        startInfo.FileName = "cmd.exe";
        startInfo.Arguments = "/C " + command;
        process.StartInfo = startInfo;
        process.Start();

        processes.Add(process);
        return process;
    }


    public static void CleanUp()
    {
        if (processes == null)
            return;

        foreach (var process in processes)
            if (!process.HasExited)
                process.Kill();






    }
}
