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
using DiscoBot.Modules.Admin;

namespace DiscoBot
{
    class DiscoBot
    {
        //Discord.NET Api.
        public DiscordClient Client;
        DiscordConfigBuilder Builder;

        public DiscoBot()
        {
            //Load configuration files.
            LoadConfiguration();
                  
            //Create our client.
            Client = new DiscordClient((x =>
            {
                x.AppName = "DiscoBot";
                x.AppUrl = "https://github.com/epicmiro/DiscoBot";
                x.AppVersion = DiscordConfig.LibVersion;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = false;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = OnLogMessage;
            }));

            //Load Modules.
            LoadModules();
                                
            //Set console title.
            Console.Title = $"{Client.Config.AppName} v{Client.Config.AppVersion} (Discord.Net v{DiscordConfig.LibVersion})";
        }

        public void LoadConfiguration()
        {
            //Load our configuration file.
            DisConfig.Load();
            DisConfig.Save();
        }

        public void LoadModules()
        {
            //Enable commands on this bot.
            Client.UsingCommands(x =>
            {
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            });

            //Enable modules on this bot.
            Client.UsingModules();

            //Enable audio on this bot.
            Client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableMultiserver = true;
                x.EnableEncryption = true;
            });

            //Register our custom modules.
            Client.AddModule<MusicModule>("Music", ModuleFilter.None);
            Client.AddModule<RadioModule>("Radio", ModuleFilter.None);
            Client.AddModule<AdminModule>("Admin", ModuleFilter.None);
            Client.AddModule<TrollModule>("Troll", ModuleFilter.None);

        }

        public void Run()
        {
            //Now run our client asynchronously.
            Client.ExecuteAndWait(async () =>
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
        //Display errors that occur when a user tries to run a command
        //(In this case, we hide argcount, parsing and unknown command errors to reduce spam in servers with multiple bots)
        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.GetBaseException().Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        //msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        //msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        //msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        //msg = "Unknown command.";
                        break;
                }
            }
            if (msg != null)
            {
                Client.Log.Error("Command", msg);
            }
        }
        private void OnCommandExecuted(object sender, CommandEventArgs e)
        {
            Client.Log.Info("Command", $"{e.Command.Text} ({e.User.Name})");
        }

        private void OnLogMessage(object sender, LogMessageEventArgs e)
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
