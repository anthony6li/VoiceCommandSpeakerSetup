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
using Anthony.Logger;
using System.Reflection;

namespace AudioServerBeta
{
    public class SendVolumeLevel
    {
        private static ARLogger logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
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
                logger.Info("与对方服务器{0}通信连接成功。",Micobject.settings.sourcename);
            }
            catch (SocketException se)
            {
                logger.Error("Socket异常:",se.Message);
            }
            catch (Exception ex)
            {
                logger.Error("通信异常",ex.Message);
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
            catch (Exception exc)
            {
                logger.Error(exc.Message); 
            }

            try
            {
                _waveIn.StartRecording();
                logger.Info("开始接收语音信号。");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
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
                    logger.Error("发送语音流出现异常。Exception:{0}",se.Message);
                }
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

        public void Disable()
        {
            try
            {
                if (_waveIn != null)
                {
                    try
                    {
                        _waveIn.StopRecording();
                        logger.Info("关闭语音接收。");
                    }
                    catch(Exception exc)
                    {
                        logger.Info("关闭语音接收出现异常。Exception：{0}",exc.Message);
                    }
                }
                if (client != null)
                {
                    logger.Info("关闭语音接收Socket套接字。");
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("Disable 异常：{0}",ex.Message);
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
