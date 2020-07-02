using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using XLMultiplayerServer;

namespace XLSPublicServers
{
    public class Main
    {
        private static Plugin pluginInfo;

        [JsonProperty("Server_Name")]
        private static string SERVER_NAME = "";


        [JsonProperty("Port")]
        private static ushort SERVER_PORT { get; set; } = 7777;

        public static void Load(Plugin plugin)
        {
            pluginInfo = plugin;
            pluginInfo.OnToggle = OnToggle;
        }

        private static void OnToggle(bool enabled)
        {
            if (pluginInfo.enabled)
            {
                if (File.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "ServerConfig.json"))
                {
                    JsonConvert.DeserializeObject<Main>(File.ReadAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar.ToString() + "ServerConfig.json"));
                }
                else
                {
                    pluginInfo.LogMessage("[XLSPS Plugin Error] Could not find server config file", ConsoleColor.Red);
                    Console.In.Read();
                }
                SendReq();
            }
        }

        private static async void SendReq()
        {
            var client = new HttpClient();
            pluginInfo.LogMessage("[XLSPS Plugin] Starting...", ConsoleColor.Green);
            while (true)
            {
                try
                {

                    var values = new Dictionary<string, string>
                    {
                        { "maxPlayers", pluginInfo.maxPlayers.ToString() },
                        { "serverName", SERVER_NAME },
                        { "serverVersion", pluginInfo.serverVersion},
                        { "serverPort", SERVER_PORT.ToString() },
                        { "currentPlayers", pluginInfo.playerList.Count.ToString() },
                        { "mapName", pluginInfo.currentMap }
                    };
                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync("https://api.sxlservers.com/serverinfo", content);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        pluginInfo.LogMessage($"Error Connecting To SXLServers API: {response.StatusCode}", ConsoleColor.White);
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
