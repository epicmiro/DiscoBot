using Discord;
using Discord.Commands;
using Discord.Modules;
using SoundCloud.NET.Models;
using SoundCloud.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using NAudio.Wave;
using YoutubeExtractor;
using Discord.Audio;
using System.Threading;

namespace DiscoBot.Modules.Troll
{
    internal class TrollModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            manager.CreateCommands("", group =>
            {
                //register skip command.
                group.CreateCommand("uwotm8").
                     Parameter("nothing", ParameterType.Unparsed).
                     Description("TTS u wot m8.").
                     Do(UWotM8Command);
            });
        }


        public async Task UWotM8Command(CommandEventArgs e)
        {
            await e.Channel.SendTTSMessage("What the fuck did you just fucking say about me, you little bitch? I’ll have you know I graduated top of my class in the Navy Seals,");
            await e.Channel.SendTTSMessage("and I’ve been involved in numerous secret raids on Al-Quaeda, and I have over 300 confirmed kills. I am trained in gorilla warfare and I’m the top sniper in the entire US armed forces.");
            await e.Channel.SendTTSMessage("You are nothing to me but just another target. I will wipe you the fuck out with precision the likes of which has never been seen before on this Earth, mark my fucking words. You think you can get away with saying that shit to me over the Internet?");
            await e.Channel.SendTTSMessage("Think again, fucker. As we speak I am contacting my secret network of spies across the USA and your IP is being traced right now so you better prepare for the storm, maggot.");
            await e.Channel.SendTTSMessage("The storm that wipes out the pathetic little thing you call your life. You’re fucking dead, kid. I can be anywhere, anytime, and I can kill you in over seven hundred ways, and that’s just with my bare hands. ");
            await e.Channel.SendTTSMessage("Not only am I extensively trained in unarmed combat, but I have access to the entire arsenal of the United States Marine Corps and I will use it to its full");
            await e.Channel.SendTTSMessage("extent to wipe your miserable ass off the face of the continent, you little shit. If only you could have known what unholy retribution your little “clever” comment was about to bring down upon you, ");
            await e.Channel.SendTTSMessage("maybe you would have held your fucking tongue. But you couldn’t, you didn’t, and now you’re paying the price, you goddamn idiot.");
            await e.Channel.SendTTSMessage("I will shit fury all over you and you will drown in it. You’re fucking dead, kiddo.");
        }
    }
}
