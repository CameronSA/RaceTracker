using System.Diagnostics;

namespace RaceTracker.LogicHelpers
{
    public static class CommandLine
    {
        public static void ExecuteCommand(string command)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
            cmd.StartInfo.WorkingDirectory = @"C:\Windows\System32";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Verb = "runas";
            cmd.StartInfo.Arguments = command;
            cmd.Start();
            cmd.WaitForExit();
        }
    }
}
