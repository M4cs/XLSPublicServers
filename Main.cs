using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace XLSPublicServers
{
    public class Main
    {
        private static XLMultiplayerServer.Server gameplayServer;

        [JsonProperty("Server_Name")]
        private static string SERVER_NAME = "";


        [JsonProperty("Port")]
        private static ushort SERVER_PORT { get; set; } = 7777;

        public static void Load(XLMultiplayerServer.Server server)
        {
            gameplayServer = server;
            if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "ServerConfig.json"))
            {
                JsonConvert.DeserializeObject<Main>(File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "ServerConfig.json"));
            }
            else
            {
                gameplayServer.LogMessageCallback("[XLSPS Plugin Error] Could not find server config file", ConsoleColor.Red);
                Console.In.Read();
                return;
            }
            SendReq();
        }

        private static async void SendReq()
        {
            var client = new HttpClient();
            gameplayServer.LogMessageCallback("[XLSPS Plugin] Starting...", ConsoleColor.Green);
            while (true)
            {
                try
                {
                    int currentPlayers = 0;
                    foreach (XLMultiplayerServer.Player player in gameplayServer.players)
                    {
                        if (player != null)
                            currentPlayers++;
                    }
                    var values = new Dictionary<string, string>
                    {
                        { "maxPlayers", gameplayServer.players.Length.ToString() },
                        { "serverName", SERVER_NAME },
                        { "serverVersion", "0.9.0" },
                        { "serverPort", SERVER_PORT.ToString() },
                        { "currentPlayers", currentPlayers.ToString() },
                        { "mapName", gameplayServer.mapList[gameplayServer.currentMapHash] }
                    };
                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync("https://api.sxlservers.com/serverinfo", content);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        gameplayServer.LogMessageCallback($"Error Connecting To SXLServers API: {response.StatusCode}", ConsoleColor.White);
                    }
                }
                catch (Exception)
                {
                    client = new HttpClient();
                }
                await Task.Delay(10000);
            }
        }
    }
}
