using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

public class PythonRunner
{
		// strings collecting output/error messages, in case
		public StringBuilder _outputBuilder;
		// private StringBuilder _errorBuilder;

		// The Python interpreter ('python.exe') that is used by this instance.
		public string Interpreter { get; }

		// The timeout for the underlying component in msec.
		public int Timeout { get; set; }

      // The anaconda environment to activate
      public string Environment { get; set; }

      // <param name="interpreter"> Full path to the Python interpreter ('python.exe').
      // <param name="timeout"> The script timeout in msec. Defaults to 10000 (10 sec).
      public PythonRunner(string interpreter, string environment, int timeout = 10000)
		{
			if (interpreter == null)
			{	throw new ArgumentNullException(nameof(interpreter));
			}

			if (!File.Exists(interpreter))
			{	throw new FileNotFoundException(interpreter);
			}

			Interpreter = interpreter;
			Timeout     = timeout;
         Environment = environment;
		}

      // to run a sequence of dos commands, not read from a file
      public string runDosCommands(string strCommand)
      { 
         _outputBuilder = new StringBuilder();
         string res = "";
         var pi = new ProcessStartInfo
         {
            // Separated FileName and Arguments
            FileName = "cmd.exe",
            Arguments = $"/c C:/Users/lollo/Anaconda3/condabin/conda.bat activate {Environment}&&python {strCommand}",
            UseShellExecute = false, 
            CreateNoWindow = false,
            ErrorDialog = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
         };         

         using (var process = new Process(){ StartInfo = pi,
                                             EnableRaisingEvents = true
                                           })
         {
            process.OutputDataReceived += (sender, e) =>
            {
               // could be null terminated, needs null handling
               if (e.Data != null)
               {
                  //Console.WriteLine("> "+e.Data);
                  _outputBuilder.AppendLine(e.Data);
               }
            };
         
            process.Exited += (sender, e) =>
            {
               // when Exited is called, OutputDataReceived could still being loaded
               // you need a proper release code here
               Console.WriteLine("exiting ...");
               res = _outputBuilder.ToString();
            };
         
            process.Start();
            // You need to call this explicitly after Start
            process.BeginOutputReadLine();

            /*
            // Pass multiple commands to cmd.exe
            using (var sw = process.StandardInput)
            {
               if (sw.BaseStream.CanWrite)
               {
                  //sw.WriteLine("echo off");
                  sw.WriteLine($"/ProgramData/Anaconda3/condabin/conda.bat activate {Environment}");
                  sw.WriteLine($"python {strCommand}");
                  //sw.WriteLine("exit");
               }
            }  
            */    

            // With WaitForExit, it is same as synchronous,
            // to make it truly asynchronous, you'll need to work on it from here
            process.WaitForExit();
         }
         // here no more process
         return res;
      }    

      // Converts a base64 string (as printed by python script) to a bitmap image.
      public Bitmap FromPythonBase64String(string pythonBase64String)
      {
         // Remove the first two chars and the last one.
         // First one is 'b' (python format sign), others are quote signs.
         string base64String = pythonBase64String.Substring(2, pythonBase64String.Length - 3);

         // Convert now raw base46 string to byte array.
         byte[] imageBytes = Convert.FromBase64String(base64String);

         // Read bytes as stream.
         var memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
         memoryStream.Write(imageBytes, 0, imageBytes.Length);

         // Create bitmap from stream.
         return (Bitmap)Image.FromStream(memoryStream, true);
      }
}