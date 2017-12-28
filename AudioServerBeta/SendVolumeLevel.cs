using AudioServerBeta.Sources;
using AudioServerBeta.Sources.Audio;
using g711audio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace AudioServerBeta
{
    public class SendVolumeLevel
    {
        private int audioMode = 0;
        public objectsMicrophone Micobject;
        private WaveIn _waveIn;
        private int _sampleRate = 8000;
        private int _bitsPerSample = 16;
        private int _channels = 1;
        public IAudioSource AudioSource;
        public IWavePlayer WaveOut;
        private WaveInProvider _waveProvider;
        private MeteringSampleProvider _meteringProvider;
        private SampleChannel _sampleChannel;
        Socket client;
        IPEndPoint ipep;

        private WaveFormat _recordingFormat;

        public WaveFormat RecordingFormat
        {
            get { return _recordingFormat; }
            set
            {
                _recordingFormat = value;
            }
        }

        public int AudioMode
        {
            get
            {
                return audioMode;
            }

            set
            {
                audioMode = value;
            }
        }

        public bool Listening
        {
            get
            {
                if (WaveOut != null && WaveOut.PlaybackState == PlaybackState.Playing)
                    return true;
                return false;
            }
            set
            {
                if (WaveOut != null)
                {
                    if (value && AudioSource != null)
                    {
                        //(creates the waveoutprovider referenced below)
                        AudioSource.Listening = true;

                        WaveOut.Init(AudioSource.WaveOutProvider);
                        //Setting volume not supported on DirectSoundOut, adjust the volume on your WaveProvider instead
                        //WaveOut.Volume = 0;
                        WaveOut.Play();

                    }
                    else
                    {
                        if (AudioSource != null) AudioSource.Listening = false;
                        WaveOut.Stop();

                    }
                }
            }
        }

        public SendVolumeLevel(objectsMicrophone om)
        {
            Micobject = om;
        }

        public void Enable()
        {
            try
            {
                ipep = new IPEndPoint(IPAddress.Parse(Micobject.settings.sourcename), 8092);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(ipep);
            }
            catch (SocketException se)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                _sampleRate = Micobject.settings.samples;
                _bitsPerSample = Micobject.settings.bits;
                _channels = Micobject.settings.channels;

                RecordingFormat = new WaveFormat(_sampleRate, _bitsPerSample, _channels);


                _waveIn = new WaveIn { BufferMilliseconds = 40, DeviceNumber = 0, WaveFormat = RecordingFormat };
                _waveIn.DataAvailable += WaveInDataAvailable;
                _waveIn.RecordingStopped += WaveInRecordingStopped;

                _waveProvider = new WaveInProvider(_waveIn);
                _sampleChannel = new SampleChannel(_waveProvider);

                _meteringProvider = new MeteringSampleProvider(_sampleChannel);
                //_meteringProvider.StreamVolume += _meteringProvider_StreamVolume;

            }
            catch (Exception)
            {
                throw;
            }

            try
            {
                _waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            var sampleBuffer = new float[e.BytesRecorded];
            if (_meteringProvider != null)
            {
                _meteringProvider.Read(sampleBuffer, 0, e.BytesRecorded);

                var enc = new byte[e.Buffer.Length / 2];
                //可以控制是否对语音进行编码，编码之后Client才可以播放出声音
                if (AudioServerBetaDemo.ifF2PressProsessing)
                {
                    ALawEncoder.ALawEncode(e.Buffer, enc);
                }
                try
                {
                    SendVarData(client, enc);
                }
                catch (SocketException se)
                {
                    MessageBox.Show(se.Message);
                }


                //if (mySocket.Connected)
                //{
                //    SendToBrowser(enc, mySocket);
                //}
            }
        }

        private void WaveInRecordingStopped(object sender, EventArgs e)
        {
            Micobject.settings.active = false;
            if (_waveIn != null)
            {
                _waveIn.Dispose();
                _waveIn = null;
            }
        }

        private bool SendToBrowser(Byte[] bSendData, Socket socket)
        {
            try
            {
                if (socket.Connected)
                {
                    int sent = socket.Send(bSendData);
                    if (sent < bSendData.Length)
                    {
                        //Debug.WriteLine("Only sent " + sent + " of " + bSendData.Length);
                    }
                    if (sent == -1)
                        return false;
                    return true;
                }
            }
            catch (Exception e)
            {
                //Debug.WriteLine("Send To Browser Error: " + e.Message);
                //MainForm.LogExceptionToFile(e);
            }
            return false;
        }

        public void Disable()
        {
            try
            {
                if (_waveIn != null)
                {
                    try
                    {
                        _waveIn.StopRecording();
                    }
                    catch
                    {
                    }
                }
                if (client != null)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public int SendVarData(Socket s, byte[] data)
        {
            int total = 0;
            int size = data.Length;
            int dataleft = size;
            int sent;
            byte[] datasize = new byte[4];

            try
            {
                datasize = BitConverter.GetBytes(size);
                sent = s.Send(datasize);

                while (total < size)
                {
                    sent = s.Send(data, total, dataleft, SocketFlags.None);
                    total += sent;
                    dataleft -= sent;
                }

                return total;
            }
            catch
            {
                return 3;

            }
        }
    }
}
