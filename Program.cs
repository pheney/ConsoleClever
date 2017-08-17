using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClever
{
    /// <summary>
    /// 2017-8-17
    /// Ref: https://www.cleverbot.com/api/
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            bool exit = false;
            string reply ="", input;

            cs = GetConversationToken(csFile);

            while (!exit)
            {
                if (!string.IsNullOrWhiteSpace(reply))
                {
                    Console.CursorLeft = 0;
                    Console.ForegroundColor = botColor;
                    Console.WriteLine("BOT: " + reply);
                }
                Console.ForegroundColor = userColor;
                Console.Write("You: ");
                input = Console.ReadLine();
                exit = string.IsNullOrWhiteSpace(input);
                if (!exit)
                {
                    Console.ForegroundColor = botColor;
                    Console.Write("...");
                    reply = GetReply(input);
                }
            }

            PutConversationToken(csFile, cs);
        }

        private static ConsoleColor userColor = ConsoleColor.Green;
        private static ConsoleColor botColor = ConsoleColor.Magenta;
        private static ConsoleColor systemColor = ConsoleColor.White;
        private static ConsoleColor warningColor = ConsoleColor.Yellow;

        private static string csFile = "ConversationToken.txt";
        private static string cs;

        private static string GetConversationToken(string fileName)
        {
            string result = "";

            try
            {
                result = System.IO.File.ReadAllText(fileName);
                Console.ForegroundColor = systemColor;
                Console.WriteLine("Previous conversation loaded. Welcome back.");
            }
            catch (Exception)
            {
                //  ignore
                Console.ForegroundColor = warningColor;
                Console.WriteLine("Unable to load conversation token. This will be a new conversation.");
            }

            return result;
        }

        private static void PutConversationToken(string fileName, string token)
        {
            try
            {
                System.IO.File.WriteAllText(fileName, token);
                Console.ForegroundColor = systemColor;
                Console.WriteLine("Conversation saved");
                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.ForegroundColor = warningColor;
                Console.WriteLine("Unabled to save conversation token: " + cs);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Max calls per month, for free account
        /// </summary>
        private static int maxAPIcalls = 5000;
        private static int callCount;

        static string baseUrl = "https://www.cleverbot.com/getreply?";
        static string keyTemplate = "key={key}";
        static string stateTemplate = "cs={cs}";
        static string inputTempalte = "input={input}";

        static string key = "kv85jCC3ypppmqcLrWHip1cdFZrloGIQ";
        static string de = "5";


        /// <summary>
        /// Sample response:
        ///     {
        ///         "cs":"76nxdxIJO2...AAA",
        ///         "interaction_count":"1",
        ///         "input":"",
        ///         "output":"Good afternoon.",
        ///         "conversation_id":"AYAR1E3ITW",
        ///         ...
        ///     }
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetReply(string input)
        {
            string result = "";

            string url = baseUrl
                + keyTemplate.Replace("{key}", decrypt(key, de))
                + "&" + inputTempalte.Replace("{input}", input);

            if (!string.IsNullOrWhiteSpace(cs)) url += "&" + stateTemplate.Replace("{cs}", cs);
            
            //  Build the web request
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.ContentType = "application/json";

            //  Create an empty response
            WebResponse webResp = null;

            try
            {
                //  Execute the request and put the result into response
                webResp = webRequest.GetResponse();
                var encoding = ASCIIEncoding.ASCII;
                using (var reader = new System.IO.StreamReader(webResp.GetResponseStream(), encoding))
                {
                    //  Convert the json string to a json object
                    JObject json = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd());

                    //  store conversation state
                    cs = (string)json["cs"];

                    //  Get conversation reply
                    result = (string)json["output"];
                }
            }
            catch (WebException e)
            {
                //401: unauthorised due to missing or invalid API key or POST request, the Cleverbot API only accepts GET requests
                //404: API not found
                //413: request too large if you send a request over 16Kb
                //502 or 504: unable to get reply from API server, please contact us
                //503: too many requests from a single IP address or API key
                Console.WriteLine();
                Console.WriteLine(e.ToString());
            }
            return result;
        }

        /// <summary>
        /// 2017-8-17
        /// </summary>
        private static string decrypt(string source, string key)
        {
            return source.Substring(int.Parse(key));
        }
    }
}
