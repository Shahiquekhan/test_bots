using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airfrance_bot
{
    public class Logs
    {
        public static void LogMessage(string message)
        {
            // Write message to console
            Console.WriteLine(message);

            // Write message to log file
            WriteToLogFile(message);
        }
        public static void LogError(string errorMessage)
        {
            // Write error message to console
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();

            // Write error message to log file
            WriteToLogFile(errorMessage);
        }

        static void WriteToLogFile(string message)
        {
            // Specify the path to the log file
            string logFilePath = "log.txt";

            try
            {
                // Create a StreamWriter instance to append to the log file
                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    // Write the message along with the current timestamp
                    sw.WriteLine($"{DateTime.Now} - {message}");
                }
            }
            catch (Exception ex)
            {
                // If an error occurs while writing to the log file, write the error message to the console
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }
    }
}
