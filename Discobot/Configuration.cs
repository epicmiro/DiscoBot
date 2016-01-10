using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscoBot
{
    class DisConfig
    {
        private const string path = "configuration.json";
        private static DisConfig _instance = new DisConfig();

        //Load the configuration.
        public static void Load()
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Please write your log-in information into configuration.json");
                Save();
            }

            _instance = JsonConvert.DeserializeObject<DisConfig>(File.ReadAllText(path));

        }

        //Save the configuration.
        public static void Save()
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(_instance, Formatting.Indented));
        }

        //Discord
        public class DiscordSettings
        {
            [JsonProperty("username")]
            public string Email = "example@example.com";
            [JsonProperty("password")]
            public string Password = "example";
        }

        [JsonProperty("discord")]
        private DiscordSettings _discord = new DiscordSettings();
        public static DiscordSettings Discord => _instance._discord;

        //Soundcloud
        public class SoundcloudSettings
        {
            [JsonProperty("token")]
            public string Token = "example";
        }

        [JsonProperty("soundcloud")]
        private SoundcloudSettings _soundcloud = new SoundcloudSettings();
        public static SoundcloudSettings Soundcloud => _instance._soundcloud;

        //Soundcloud
        public class ShoutcastSettings
        {
            [JsonProperty("radioURL")]
            public string URL = "http://uk1.internet-radio.com:8106";
        }

        [JsonProperty("shoutcast")]
        private ShoutcastSettings _shoutcast = new ShoutcastSettings();
        public static ShoutcastSettings ShoutCast => _instance._shoutcast;
    }
}
