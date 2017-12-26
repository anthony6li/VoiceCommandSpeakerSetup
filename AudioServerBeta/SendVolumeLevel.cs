using AudioServerBeta.Sources;
using AudioServerBeta.Sources.Audio;
using AudioServerBeta.Sources.Audio.streams;
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

namespace AudioServerBeta
{
    public class SendVolumeLevel
    {
        public AudioServerBetaDemo ParentForm;
        private int audioMode = 0;
        public objectsMicrophone Micobject;
        private WaveIn _waveIn;
        private WaveInProvider _waveProvider;
        private MeteringSampleProvider _meteringProvider;
        private SampleChannel _sampleChannel;
        public IAudioSource AudioSource;
        public IWavePlayer WaveOut;
        private readonly object _lockobject = new object();
        public List<HttpRequest> OutSockets = new List<HttpRequest>();
        public event Delegates.NewDataAvailable DataAvailable;
        public event EventHandler AudioDeviceEnabled, AudioDeviceDisabled, AudioDeviceReConnected;

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

        public SendVolumeLevel(objectsMicrophone om, int mode, AudioServerBetaDemo form)
        {
            Micobject = om;
            audioMode = mode;
            ParentForm = form;
        }

        public void Enable()
        {
            if (audioMode == 0)
            {
                AudioSource = new iSpyServerStream(Micobject.settings.sourcename)
                { RecordingFormat = new WaveFormat(8000, 16, 1) };
                if (AudioSource != null)
                {
                    WaveOut = !string.IsNullOrEmpty(Micobject.settings.deviceout)
                        ? new DirectSoundOut(new Guid(Micobject.settings.deviceout), 100)
                        : new DirectSoundOut(100);
                    AudioSource.AudioFinished -= AudioDeviceAudioFinished;
                    AudioSource.DataAvailable -= AudioDeviceDataAvailable;

                    AudioSource.AudioFinished += AudioDeviceAudioFinished;
                    AudioSource.DataAvailable += AudioDeviceDataAvailable;
                }

                if (!AudioSource.IsRunning)
                {
                    lock (_lockobject)
                    {
                        AudioSource.Start();
                    }
                }
            }
        }

        public void WaveInDataAvailable(object sender, WaveInEventArgs e)
        {
            var sampleBuffer = new float[e.BytesRecorded];
            if (_meteringProvider != null)
            {
                _meteringProvider.Read(sampleBuffer, 0, e.BytesRecorded);

                var enc = new byte[e.Buffer.Length / 2];
                ALawEncoder.ALawEncode(e.Buffer, enc);


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

        public void AudioDeviceAudioFinished(object sender, PlayingFinishedEventArgs e)
        {
            switch (e.ReasonToFinishPlaying)
            {
                case ReasonToFinishPlaying.DeviceLost:
                case ReasonToFinishPlaying.EndOfStreamReached:
                case ReasonToFinishPlaying.VideoSourceError:
                case ReasonToFinishPlaying.StoppedByUser:
                    Disable(false);
                    break;
            }
        }

        public void Disable(bool stopSource = true)
        {
            try
            {

                if (AudioSource != null)
                {
                    AudioSource.AudioFinished -= AudioDeviceAudioFinished;
                    AudioSource.DataAvailable -= AudioDeviceDataAvailable;

                    //if (!IsClone)
                    //{
                        if (stopSource)
                        {
                            AudioSource.Stop();
                            Thread.Sleep(250);
                        }
                    //}
                    //else
                    //{
                    //    int imic;
                    //    if (Int32.TryParse(Micobject.settings.sourcename, out imic))
                    //    {

                    //        //var vl = MainForm.InstanceReference.GetVolumeLevel(imic);
                    //        var vl = this;
                    //        if (vl != null)
                    //        {
                    //            vl.AudioDeviceDisabled -= MicrophoneDisabled;
                    //            vl.AudioDeviceEnabled -= MicrophoneEnabled;
                    //            vl.AudioDeviceReConnected -= MicrophoneReconnected;
                    //        }
                    //    }
                    //}

                }
                
                Listening = false;
                //UpdateFloorplans(false);
                Micobject.settings.active = false;

                AudioDeviceDisabled?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
            }
        }

        public void AudioDeviceDataAvailable(object sender, DataAvailableEventArgs e)
        {
            try
            {
                if (Micobject.settings.needsupdate)
                {
                    Micobject.settings.samples = AudioSource.RecordingFormat.SampleRate;
                    Micobject.settings.channels = AudioSource.RecordingFormat.Channels;
                    Micobject.settings.needsupdate = false;
                }

                OutSockets.RemoveAll(p => p.TcpClient.Client.Connected == false);
                if (OutSockets.Count > 0)
                {
                   

                    byte[] bSrc = e.RawData;
                    int totBytes = bSrc.Length;

                    var bterm = Encoding.ASCII.GetBytes("\r\n");
                }

                DataAvailable?.Invoke(this, new NewDataAvailableArgs((byte[])e.RawData.Clone()));

            }
            catch (Exception ex)
            {
            }
        }
    }
}
