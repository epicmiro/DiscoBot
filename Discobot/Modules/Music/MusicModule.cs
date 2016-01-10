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

namespace DiscoBot.Modules.Music
{
    internal class MusicModule : IModule
    {
        private ModuleManager _manager;
        private DiscordClient _client;
        private MusicQueue _queue;
        private SoundCloudManager _soundCloud;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = _manager.Client;

            //create our soundcloud API
            _soundCloud = new SoundCloudManager(DisConfig.Soundcloud.Token);

            //create our music queue
            _queue = new MusicQueue();

            manager.CreateCommands("", group =>
            {
                //register skip command.
                group.CreateCommand("skip").
                     Parameter("nothing", ParameterType.Unparsed).
                     Description("Skips the current song in the music queue.").
                     Do(SkipCommand);

                //register request command
                group.CreateCommand("request").
                    Parameter("url", ParameterType.Required).
                    Description("Adds youtube or soundcloud video to DiscoBot queue.").
                    Do(RequestCommand);

                //register join room command
                group.CreateCommand("joinroom").
                    Parameter("room", ParameterType.Unparsed).
                    Description("Joins a voice channel that you specified.").
                    Do(JoinVoiceCommand);

                //register join room command
                group.CreateCommand("leaveroom").
                    Parameter("room", ParameterType.Unparsed).
                    Description("Leaves any voice channel the bot is connected to in this server.").
                    Do(LeaveVoiceCommand);
            });

        }

        private async Task RequestCommand(CommandEventArgs e)
        {
                var urlToDownload = e.Args[0];
                var newFilename = Guid.NewGuid().ToString();
                var mp3OutputFolder = @"c:\mp3\";

            if (urlToDownload.Contains("soundcloud"))
            {
                Track track = _soundCloud.GetTrack(e.Args[0]);
                string inPath = Path.Combine(mp3OutputFolder, newFilename + ".mp3");

                if (!track.Streamable)
                {
                    await e.Channel.SendMessage("\"" + track.Title + "\" is not streamable :C");
                }

                try
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(track.StreamUrl + "?client_id=" + _soundCloud.ClientID, inPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }

                var outFile = inPath.Remove(inPath.Length - 4) + "_c" + ".wav";

                try
                {
                    using (var reader = new MediaFoundationReader(inPath))
                    {
                        var outFormat = new WaveFormat(48000, 16, 2);
                        using (var resampler = new MediaFoundationResampler(reader, outFormat))
                        {
                            resampler.ResamplerQuality = 60;
                            WaveFileWriter.CreateWaveFile(outFile, resampler);
                        }
                    }

                    File.Delete(inPath);

                }
                catch (Exception e2)
                {
                    Console.Write(e2.ToString());
                    return;
                }

                await e.Channel.SendMessage("Added \"" + track.Title + "\" to the queue. It will be played soon.");

                _queue.musicQueue.Enqueue(Tuple.Create<string, string>(outFile, track.Title));
                Thread thread = new Thread(() => { _queue.PlayNextMusicToAllVoiceClients(); });
                thread.Start();

            }
            else
            {

                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(e.Args[0]);

                VideoInfo video = videoInfos.Where(info => info.CanExtractAudio).OrderByDescending(info => info.AudioBitrate).First();

                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                string inPath = Path.Combine(mp3OutputFolder, newFilename + video.AudioExtension);

                try
                {
                    var audioDownloader = new AudioDownloader(video, inPath);
                    audioDownloader.Execute();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to download youtube link.");
                    Console.Write(ex.ToString());
                }

                var outFile = inPath.Remove(inPath.Length - 4) + "_c" + ".wav";

                try
                {
                    using (var reader = new MediaFoundationReader(inPath))
                    {
                        var outFormat = new WaveFormat(48000, 16, 2);
                        using (var resampler = new MediaFoundationResampler(reader, outFormat))
                        {
                            resampler.ResamplerQuality = 60;
                            WaveFileWriter.CreateWaveFile(outFile, resampler);
                        }
                    }

                    File.Delete(inPath);

                }
                catch (Exception e2)
                {
                    Console.Write(e2.ToString());
                    return;
                }

                await e.Channel.SendMessage("Added \"" + video.Title + "\" to the queue. It will be played soon.");

                _queue.musicQueue.Enqueue(Tuple.Create<string, string>(outFile, video.Title));

                Thread thread = new Thread(() => { _queue.PlayNextMusicToAllVoiceClients(); });
                thread.Start();
            }
        }

        private async Task SkipCommand(CommandEventArgs e)
        {
             if (_queue.currentPlaying != "undefined")
            {
                _queue.skip = true;
                await e.Channel.SendMessage("Skipping song.");
            }
        }

        public async Task JoinVoiceCommand(CommandEventArgs e)
        {
            string[] channel = e.Args[0].Split('"');
            var room = e.Server.FindChannels(channel.FirstOrDefault(), ChannelType.Voice, false);
           
            if (room.Any())
            {
                await _client.Audio().Join(room.First());
                _queue.PlayNextMusicToAllVoiceClients();
            }
            else
            {
                await e.Channel.SendMessage("Could not join room with the name " + channel.FirstOrDefault() + ".");
            }
        }

        public async Task LeaveVoiceCommand(CommandEventArgs e)
        {
            await _client.Audio().Leave(e.Server);
        }
    }
}
