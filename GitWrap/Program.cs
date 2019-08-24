using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GitWrap
{
    class Program
    {
        private static int outputLines = 0;

 	static void Main(string[] args)
        {
          if (args.Length > 0 && args[0] == "--setBashPath" && args.Length == 2)
          {
              Properties.Settings.Default.bashpath = args[1];
              Properties.Settings.Default.Save();
              Console.Write("[*] bashPath set to: " + args[1]);
          } else if (args.Length > 0 && args[0] == "--getBashPath") {
              Console.Write("[*] Current bashPath: " + Properties.Settings.Default.bashpath);
          }
          else
          {
              executeGitWithArgs(Properties.Settings.Default.bashpath, args);
          }
        }
        static void executeGitWithArgs(String bashPath, string[] args)
        {

            if (!File.Exists(bashPath))
            {
                Console.Write("[-] Error: bash.exe not found.");
                return;
            }

            ProcessStartInfo bashInfo = new ProcessStartInfo();
            bashInfo.FileName = Path.Combine(bashPath);

            // Loop through args and pass them to git executable
            StringBuilder argsBld = new StringBuilder();

            argsBld.Append(" --login -c \"git");

            for (int i = 0; i < args.Length; i++)
            {
                argsBld.Append(" " + PathConverter.convertPathFromWindowsToLinux(args[i]));
            }

			argsBld.Append("\"");

			bashInfo.Arguments = argsBld.ToString();
            bashInfo.UseShellExecute = false;
            bashInfo.RedirectStandardOutput = true;
            bashInfo.RedirectStandardError = true;
            bashInfo.CreateNoWindow = true;

            var proc = new Process
            {
                StartInfo = bashInfo
            };

            proc.OutputDataReceived += CaptureOutput;
            proc.ErrorDataReceived += CaptureError;

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
        }

        static void CaptureOutput(object sender, DataReceivedEventArgs e)
        {
            printOutputData(e.Data);
        }

        static void CaptureError(object sender, DataReceivedEventArgs e)
        {
            printOutputData(e.Data);
        }

        static void printOutputData(String data)
        {
            if (data != null)
            {
                if (outputLines > 0)
                {
                    Console.Write(Environment.NewLine);
                }
                // Print output from git, converting WSL paths to Windows equivalents.
                Console.Write(PathConverter.convertPathFromLinuxToWindows(data));
                outputLines++;
            }
        }
    }
}
