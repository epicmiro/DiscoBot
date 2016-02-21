using Discord;
using Discord.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoBot.Modules.Music
{
    public class MusicQueue
    {
        public Queue<Tuple<string, string>> musicQueue;
        public string currentPlaying;
        public bool skip;

        public MusicQueue()
        {
            musicQueue = new Queue<Tuple<string, string>>();
            currentPlaying = "undefined";
            skip = false;
        }

        public void PlayNextMusicToAllVoiceClients()
        {
            if (!musicQueue.Any() || currentPlaying != "undefined")
                return;

            string playingSong = musicQueue.First().Item1;

            lock (currentPlaying)
                currentPlaying = musicQueue.First().Item1;

            List<Thread> threads = new List<Thread>();


            foreach (Server server in Disco.Bot.Client.Servers)
            {
                IAudioClient voiceClient = Disco.Bot.Client.GetService<AudioService>().GetClient(server);

                if (voiceClient != null)
                {
                    Thread thread = new Thread(() => PlayMusic(currentPlaying, voiceClient));
                    threads.Add(thread);
                    thread.Start();

                    Disco.Bot.Client.SetGame(musicQueue.First().Item2);
                }
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            //delete the file since we dont need it anymore.
            try
            {
                File.Delete(currentPlaying);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }

            //stop skipping
            if (skip)
                skip = false;

            //remove item from queue.
            musicQueue.Dequeue();
            currentPlaying = "undefined";

            Disco.Bot.Client.SetGame(null);
            PlayNextMusicToAllVoiceClients();
        }

        public void PlayMusic(string sample, IAudioClient voiceClient)
        {
            using (WaveFileReader pcm = new WaveFileReader(sample))
            {
                int blocksize = pcm.WaveFormat.AverageBytesPerSecond / 5;
                byte[] buffer = new byte[blocksize];
                int offset = 0;

                    try
                    {
                        while (offset < pcm.Length / blocksize && !skip)
                        {
                            if (currentPlaying != sample)
                            {

                            }

                            offset++;
                            pcm.Read(buffer, 0, blocksize);

                            voiceClient.Send(buffer, 0, blocksize);
                            //voiceClient.Wait();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.ToString());
                    }
            }
            return;
        }
    }
}
