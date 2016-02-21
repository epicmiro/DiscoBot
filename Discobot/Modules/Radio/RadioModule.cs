using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoBot.Modules.Radio
{
    internal class RadioModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private RadioStream _stream;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;
            _stream = null;

            manager.CreateCommands("radio", group =>
            {
                //register radio start command.
                group.CreateCommand("start").
                     Description("Starts the radio.").
                     Do(StartRadioCommand);

                //register radio stop command.
                group.CreateCommand("stop").
                     Description("Stops the radio.").
                     Do(StopRadioCommand);
            });
        }

        public async Task StartRadioCommand(CommandEventArgs e)
        {
            if (_stream != null)
            {
                _stream.Stop();
            }

            _stream = new RadioStream(DisConfig.ShoutCast.URL);

            foreach (Server server in Disco.Bot.Client.Servers)
            {
                IAudioClient voiceClient = Disco.Bot.Client.GetService<AudioService>().GetClient(server);

                if (voiceClient != null)
                {
                    _stream.Start(voiceClient);
                    Disco.Bot.Client.SetGame("Radio");
                }
            }

            await e.Channel.SendMessage("Starting radio stream.");
        }

        public Task StopRadioCommand(CommandEventArgs e)
        {
            if (_stream != null)
            {
                _stream.Stop();
                _stream = null;
            }

            return Task.CompletedTask;
        }
    }
}
