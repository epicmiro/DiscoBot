using Discord;
using Discord.Modules;
using Discord.Commands;
using Discord.Audio;
using DiscoBot.Modules.Music;
using System;
using System.Text;
using System.Threading.Tasks;
using DiscoBot.Modules.Troll;
using DiscoBot.Modules.Radio;
using DiscoBot.Modules.Rainbow;

namespace DiscoBot
{
    class DiscoBot
    {
        //Discord.NET Api.
        public DiscordClient Client;
        public DiscordConfig Config;

        public DiscoBot()
        {
            //Load configuration files.
            LoadConfiguration();
                  
            //Create our client.
            Client = new DiscordClient(Config);

            //Load Modules.
            LoadModules();

            //Enable Error Logging.
            EnableLogging();
                                
            //Set console title.
            Console.Title = $"{Client.Config.AppName} v{Client.Config.AppVersion} (Discord.Net v{DiscordConfig.LibVersion})";
        }

        public void LoadConfiguration()
        {
            //Load our configuration file.
            DisConfig.Load();
            DisConfig.Save();

            //Set up our discord configuration.
            Config = new DiscordConfig();
            Config.AppName = "DiscoBot";
            Config.AppUrl = "https://github.com/epicmiro/DiscoBot";
            Config.AppVersion = DiscordConfig.LibVersion;
            Config.LogLevel = LogSeverity.Info;
            Config.CacheToken = true;
        }

        public void LoadModules()
        {
            //Enable commands on this bot.
            Client.UsingCommands(new CommandServiceConfig()
            {
                CommandChar = '!',
                HelpMode = HelpMode.Public
            });

            //Enable modules on this bot.
            Client.UsingModules();

            //Enable audio on this bot.
            Client.UsingAudio(new AudioServiceConfig()
            {
                Mode = AudioMode.Outgoing,
                EnableMultiserver = false,
                Channels = 2
            });

            //Register our custom modules.
            Client.AddModule<MusicModule>("Music", ModuleFilter.None);
            Client.AddModule<RadioModule>("Radio", ModuleFilter.None);
            Client.AddModule<RainbowModule>("Rainbow", ModuleFilter.None);
            Client.AddModule<TrollModule>("Troll", ModuleFilter.None);

        }

        private void EnableLogging()
        {
            //Enable message logging.
            Client.Log.Message += (s, e) => LogMessage(e);

            //Enable command error logging.
            Client.Commands().CommandErrored += (s, e) =>
            {
                string message = e.Exception?.GetBaseException().Message;

                if (message != null)
                    Client.Log.Error("Command", e.Exception.ToString());
            };
        }

        public void Run()
        {
            //Now run our client asynchronously.
            Client.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await Client.Connect(DisConfig.Discord.Email, DisConfig.Discord.Password);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Client.Log.Error($"Login Failed", ex);
                        await Task.Delay(Client.Config.FailedReconnectDelay);
                    }
                }
            });
        }

        //Log any messages. Copied from DiscordBot (https://github.com/RogueException/DiscordBot)
        private void LogMessage(LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();

            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
    }
}
