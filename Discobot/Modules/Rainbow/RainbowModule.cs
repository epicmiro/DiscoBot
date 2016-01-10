using Discord;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoBot.Modules.Rainbow
{
    internal class RainbowModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private CancellationToken _cancelToken;

        private int currentColor = 0;

        private class ColorDefinition
        {
            public string Id;
            public string Name;
            public Color Color;
            public ColorDefinition(string name, Color color)
            {
                Name = name;
                Id = name.ToLowerInvariant();
                Color = color;
            }
        }

        private List<ColorDefinition> _colors;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("", group =>
            {
                //register skip command.
                group.CreateCommand("rainbow").
                     Parameter("nothing", ParameterType.Unparsed).
                     Description("Skips the current song in the music queue.").
                     Do(RainbowCommand);
            });

            _colors = new List<ColorDefinition>()
            {
                new ColorDefinition("Blue", Color.Blue),
                new ColorDefinition("Teal", Color.Teal),
                new ColorDefinition("Gold", Color.Gold),
                new ColorDefinition("Green", Color.Green),
                new ColorDefinition("Purple", Color.Purple),
                new ColorDefinition("Orange", Color.Orange),
                new ColorDefinition("Magenta", Color.Magenta),
                new ColorDefinition("Red", Color.Red),
                new ColorDefinition("DarkBlue", Color.DarkBlue),
                new ColorDefinition("DarkTeal", Color.DarkTeal),
                new ColorDefinition("DarkGold", Color.DarkGold),
                new ColorDefinition("DarkGreen", Color.DarkGreen),
                new ColorDefinition("DarkMagenta", Color.DarkMagenta),
                new ColorDefinition("DarkOrange", Color.DarkOrange),
                new ColorDefinition("DarkPurple", Color.DarkPurple),
                new ColorDefinition("DarkRed", Color.DarkRed),
            };
        }

        private Task RainbowCommand(CommandEventArgs arg)
        {
            if (arg.User.Name == "epicmiro")
            {
                _cancelToken = new CancellationToken();

                Task.Run(async () => { await ChangeColor(arg.Server); }, _cancelToken);     
            }
            return Task.CompletedTask;
        }

        private async Task ChangeColor(Server s)
        {
            for(int i = 0; i < 100; i++)
            {
                Role role = s.GetUser(Disco.Bot.Client.CurrentUser.Id).Roles.FirstOrDefault();

                currentColor++;

                await role.Edit(null, null, _colors[currentColor % 15].Color);
                Thread.Sleep(100);
            }
        }
    }
}
