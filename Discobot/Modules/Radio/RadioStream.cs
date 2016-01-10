using Discord.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiscoBot.Modules.Radio
{
    class RadioStream
    {
        // station parameters
        string server = "http://uk1.internet-radio.com:8106";

        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        private BufferedWaveProvider bufferedWaveProvider;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;

        private IAudioClient _client;

        HttpWebRequest request = null; // web request

        public RadioStream(string a_Server)
        {
            //create a radio stream object with the specified shoutcast url.
            server = a_Server;
        }

        public void Start(IAudioClient a_Client)
        {
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;

                _client = a_Client;

                Task.Run(() => { Stream(a_Client); });
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
        }

        private void Stream(IAudioClient client)
        {
            IMp3FrameDecompressor decompressor = null;

            ShoutcastStream shoutStream = new ShoutcastStream(server);
            shoutStream.StreamTitleChanged += ChangedTitle;
            var buffer = new byte[16384 * 4]; // needs to be big enough to hold a decompressed frame

            try
            {
                    do
                    {

                        if (IsBufferNearlyFull)
                        {
                            Console.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(shoutStream);
                            }
                            catch (EndOfStreamException)
                            {
                                fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                decompressor = CreateFrameDecompressor(frame);
                                bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // allow us to get well ahead of ourselves
                            }
                            int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                            bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                        }

                        if (bufferedWaveProvider != null)
                        {
                            var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;

                            if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
                            {
                                playbackState = StreamingPlaybackState.Playing;
                                Task.Run(() => { StartPlay(); });
                            }
                            else if (fullyDownloaded && bufferedSeconds == 0)
                            {
                                Console.WriteLine("Reached end of stream");
                                Stop();
                            }
                        }
                    } while (playbackState != StreamingPlaybackState.Stopped);
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
            }
        }

        private void ChangedTitle(object sender, EventArgs e)
        {
            Disco.Bot.Client.SetGame(((ShoutcastStream)sender).StreamTitle);
        }

        public void Stop()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Stopped;
                Disco.Bot.Client.SetGame(null);
            }
        }

        public bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }


        private void StartPlay()
        {
            try
            {
                    var outFormat = new WaveFormat(48000, 16, 2);
                    using (var resampler = new MediaFoundationResampler(bufferedWaveProvider, outFormat))
                    {
                        resampler.ResamplerQuality = 60;
                        int blocksize = resampler.WaveFormat.AverageBytesPerSecond / 5;
                        byte[] buffer = new byte[blocksize];

                    while (bufferedWaveProvider.BufferedBytes > bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 5 && playbackState != StreamingPlaybackState.Stopped)
                    {
                        resampler.Read(buffer, 0, blocksize);
                        _client.Send(buffer, 0, blocksize);
                    }
                }
            }
            catch (Exception e2)
            {
                Console.Write(e2.ToString());
                return;
            }

            if(playbackState != StreamingPlaybackState.Stopped)
                playbackState = StreamingPlaybackState.Buffering;

            Console.WriteLine(String.Format("Gotta buffer"));          
        }

        public void Pause()
        {
            playbackState = StreamingPlaybackState.Buffering;
        }

        public static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }
    }
}
