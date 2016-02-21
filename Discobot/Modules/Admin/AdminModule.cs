using Discord;
using Discord.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Commands.Permissions;

namespace DiscoBot.Modules.Admin
{
    internal class AdminModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("admin", group =>
            {
                //register radio start command.
                group.CreateCommand("acceptinvite").
                     Parameter("inviteCode", ParameterType.Required).
                     Description("Accepts invite to a server.").Hide().
                     Do(AcceptCommand);

                //register radio start command.
                group.CreateCommand("leaveserver").
                     Parameter("none", ParameterType.Unparsed).
                     Description("leaves the current server.").Hide().
                     Do(LeaveCommand);
            });

        }

        private async Task LeaveCommand(CommandEventArgs e)
        {
            if(e.User.Name == "epicmiro")
                await e.Server.Leave();
        }

        private async Task AcceptCommand(CommandEventArgs e)
        {
            if (e.User.Name == "epicmiro")
            {
                Invite inv = await _client.GetInvite(e.Args[0]);
                await inv?.Accept();
            }
        }
    }
}
