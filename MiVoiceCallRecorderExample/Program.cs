using MiVoiceCallRecorderExample.LiveCalls;
using MiVoiceCallRecorderExample.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiVoiceCallRecorderExample
{
    class Program
    {
        private static string user;
        private static string password;
        private static string extn;
        private static string sessionId;
        private static string command;
        private static string field_name;
        private static string data;
        private static bool loggedIn;
        private static string current_callid;
        private static SessionSoapClient session_client;
        private static LiveCallsSoapClient livecalls_client;
        
        static void Main(string[] args)
        {

            loggedIn = false;
            if (args.Length == 4)
            {
                Init(args);
                ProcessCommand();
            }
            else if(args.Length == 6)
            {
                Init(args);
                field_name = args[4];
                data = args[5];
                ProcessCommand();
            }
            else
            {
                Console.WriteLine("Usage: MiVoiceCallRecorderExample.exe %user% %password% %extension% %command%");
                Console.WriteLine("Where:\n%user%: The MiVoice Call Recorder User to access system (must have Call Manager Rights)");
                Console.WriteLine("%password%: The password for %user%");
                Console.WriteLine("%extension%: The extension number to find the current live call - to pause/resume or write data");
                Console.WriteLine("%command%: pause/resume or write");
                Console.WriteLine("If write, then this will be followed by the field name (this must be added to the MiVoice Call Recorder DB) and the last parameter would then be the data to write");
            }
            Logout();
        }

        private static void ProcessCommand()
        {
            if (loggedIn)
            {
                current_callid = FindCall();
                if(!string.IsNullOrEmpty(current_callid))
                {
                    switch (command.ToLower())
                    {
                        case "pause":
                            PauseRecording();
                            break;
                        case "resume":
                            ResumeRecording();
                            break;
                        case "write":
                            WriteData();
                            break;
                    }
                }
            }
        }

        private static bool Login()
        {
            try
            {
                sessionId = session_client.Login("TestApp", user, password);
                if (!string.IsNullOrEmpty(sessionId))
                {
                    Console.WriteLine($"Logged In. sessionId: {sessionId}");
                    return true;
                }
                else
                {
                    Console.WriteLine("Failed to get session id");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error. Message: {ex.Message}");
                return false;
            }
  
        }

        private static void Logout()
        {
            Console.WriteLine("Logout");
            if (loggedIn)
            {
                try
                {
                    session_client.Logout(sessionId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Logout Error. Message: {ex.Message}");
                }
            }
        }

        private static void WriteData()
        {
            Console.WriteLine($"WriteData extn: {extn} field_name: {field_name} data: {data}");
            try
            {
                livecalls_client.SetCallDataField(sessionId, current_callid, field_name, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WriteData Error. Message: {ex.Message}");
            }
        }

        private static void ResumeRecording()
        {
            Console.WriteLine($"ResumeRecording extn: {extn}");
            try
            {
                livecalls_client.RecordStart(sessionId, current_callid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResumeRecording Error. Message: {ex.Message}");
            }
            
        }

        private static void PauseRecording()
        {
            Console.WriteLine($"PauseRecording extn: {extn}");
            try
            {
                livecalls_client.RecordStop(sessionId, current_callid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PauseRecording Error. Message: {ex.Message}");
            }
        }

        private static string FindCall()
        {
            try
            {
                CallSearch search = new CallSearch();
                search.AgentID = "";
                search.Extension = extn; //agent/extension to search for
                search.TrunkID = "";
                string callid = "";
                bool result = livecalls_client.TryLookupCall(sessionId, search, out callid);
                if (result)
                {
                    Console.WriteLine($"FindCall. extn: {extn} Found Call: callid: {callid}");
                    return callid;
                }
                else
                {
                    Console.WriteLine($"FindCall. extn: {extn} No Call Found");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FindCall Error. Message: {ex.Message}");
                return string.Empty;
            }
        }

        private static void Init(string [] args)
        {
            try
            {
                user = args[0];
                password = args[1];
                extn = args[2];
                command = args[3];
                session_client = new SessionSoapClient();
                livecalls_client = new LiveCallsSoapClient();
                loggedIn = Login();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Init Error. Message: {ex.Message}");
            }

        }
    }
}
