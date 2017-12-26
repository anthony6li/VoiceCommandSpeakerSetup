using System;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using NAudio.Wave;
using System.Collections.Generic;

namespace AudioServerBeta
{
    public partial class AudioServerBetaDemo : Form
    {
        bool beginMove = false;
        int currentXPosition;
        int currentYPosition;
        bool isBegin = false;

        public SendVolumeLevel MicVolumeLevel;
        public objectsMicrophone Mic;
        string[] arguments = null;
        string ip = string.Empty;
        string name = string.Empty;
        private List<ListItem> ddlAudioOut = new List<ListItem>();


        Stopwatch sw;
        TimeSpan ts;
        private Timer speakTime;
        private delegate void SetLBTime(string value);

        private struct ListItem
        {
            internal string Name;
            internal string[] Value;
            public override string ToString()
            {
                return Name;
            }
            public ListItem(string Name, string[] Value) { this.Name = Name; this.Value = Value; }
        }

        public AudioServerBetaDemo(string[] args)
        {
            arguments = args;
            InitializeComponent();
        }
        

        private objectsMicrophone AddMicrophone()
        {
            objectsMicrophone Mic = new objectsMicrophone
            {
                alerts = new objectsMicrophoneAlerts(),
                detector = new objectsMicrophoneDetector(),
                notifications = new objectsMicrophoneNotifications(),
                recorder = new objectsMicrophoneRecorder(),
                schedule = new objectsMicrophoneSchedule
                {
                    entries
                                                    =
                                                    new objectsMicrophoneScheduleEntry
                                                    [
                                                    0
                                                    ]
                }
            };
            Mic.settings = new objectsMicrophoneSettings();

            Mic.id

 = 1;
            //om.directory = RandomString(5);
            Mic.x = 0;
            Mic.y = 0;
            Mic.width = 160;
            Mic.height = 40;
            Mic.name

 = "MIC";
            Mic.description = "";
            Mic.newrecordingcount = 0;

            int port = 257;
            //foreach (objectsMicrophone om2 in Microphones)
            //{
            //    if (om2.port > port)
            //        port = om2.port + 1;
            //}
            Mic.port = port;

            Mic.settings.typeindex = 0;
            // if (audioSourceIndex == 2)
            //   om.settings.typeindex = 1;
            Mic.settings.deletewav = true;
            Mic.settings.buffer = 4;
            Mic.settings.samples = 8000;
            Mic.settings.bits = 16;
            Mic.settings.channels = 1;
            Mic.settings.decompress = true;
            Mic.settings.active = false;
            Mic.settings.notifyondisconnect = false;

            Mic.detector.sensitivity = 60;
            Mic.detector.nosoundinterval = 30;
            Mic.detector.soundinterval = 0;
            Mic.detector.recordondetect = true;

            Mic.alerts.mode = "sound";
            Mic.alerts.minimuminterval = 60;
            Mic.alerts.executefile = "";
            Mic.alerts.active = false;
            Mic.alerts.alertoptions = "false,false";

            Mic.recorder.inactiverecord = 5;
            Mic.recorder.maxrecordtime = 900;

            Mic.notifications.sendemail = false;
            Mic.notifications.sendsms = false;

            Mic.schedule.active = false;
            return Mic;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Mic = AddMicrophone();

                //取得输出设备
                int i = 0, j = 0;
                var d = DirectSoundOut.Devices;
                if (d != null)
                {
                    foreach (var dev in d)
                    {
                        ddlAudioOut.Add(new ListItem(dev.Description, new string[] { dev.Guid.ToString() }));
                    }
                }

                sw = new Stopwatch();
                speakTime = new Timer(1000);
                speakTime.AutoReset = true;
                //string[] aa = arguments[0].Substring(arguments[0].IndexOf("//", StringComparison.Ordinal)).TrimEnd('”').Trim('/').Trim('"').Trim('?').Split('&');
                //ip = aa[0].Split(':')[1];
                //name = aa[1].Split(':')[1];
                //name = HttpUtility.UrlDecode(name, Encoding.UTF8);
            }
            catch (Exception ex)
            {
            }
        }

        #region 计时器功能   
        private void SpeakTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            ts = sw.Elapsed;
            SetLB(string.Format("{0}:{1}:{2}",ts.Hours.ToString("00"),ts.Minutes.ToString("00"), ts.Seconds.ToString("00")));
        }

        private void SetLB(string value)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetLBTime(SetLB), value);
            }
            else
            {
                this.lb_Time.Text = value;
            }
        }
        #endregion

        #region 窗体隐藏标题栏后的移动问题,最小化和关闭按钮
        private void btn_min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_closeForm_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void AudioServerBetaDemo_MouseDown(object sender, MouseEventArgs e)
        {
            //将鼠标坐标赋给窗体左上角坐标  
            beginMove = true;
            currentXPosition = MousePosition.X;
            currentYPosition = MousePosition.Y;
            this.Refresh();
        }

        private void AudioServerBetaDemo_MouseLeave(object sender, EventArgs e)
        {
            //设置初始状态  
            currentXPosition = 0;
            currentYPosition = 0;
            beginMove = false;
        }

        private void AudioServerBetaDemo_MouseMove(object sender, MouseEventArgs e)
        {
            if (beginMove)
            {
                //根据鼠标X坐标确定窗体X坐标  
                this.Left += MousePosition.X - currentXPosition;
                //根据鼠标Y坐标确定窗体Y坐标  
                this.Top += MousePosition.Y - currentYPosition;
                currentXPosition = MousePosition.X;
                currentYPosition = MousePosition.Y;
            }
        }

        private void AudioServerBetaDemo_MouseUp(object sender, MouseEventArgs e)
        {
            beginMove = false;
        }
        #endregion

        #region 开始指挥和结束指挥
        /// <summary>
        /// 开始结束的Label和Panel均绑定此事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lb_BeingOver_MouseClick(object sender, MouseEventArgs e)
        {
            UpdateBeginButton(isBegin);
            //string str = "http://192.168.198.1:8092";
            string str = "http://10.10.36.28:8092";
            if (!isBegin)
            {
                if (ddlAudioOut.Count>0 && !string.IsNullOrEmpty(str))
                {

                }
                //开始语音传输
                Mic = AddMicrophone();
                Mic.settings.sourcename = str;
                Mic.settings.active = true;
                Mic.settings.deviceout = ((ListItem)ddlAudioOut[0]).Value[0];
                MicVolumeLevel = new SendVolumeLevel(Mic, 0, this);
                MicVolumeLevel.AudioMode = 0;
                MicVolumeLevel.Enable();
                isBegin = true;
            }
            else
            {
                //断开语音传输
                MicVolumeLevel.Disable();
                isBegin = false;
            }
        }

        private void UpdateBeginButton(bool begin)
        {
            if (!begin)
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.MenuHighlight;
                this.lb_BeingOver.ForeColor = System.Drawing.Color.White;
                this.lb_BeingOver.Text = "结束";
                speakTime.Elapsed += SpeakTime_Elapsed;
                sw.Start();
                speakTime.Start();
            }
            else
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.Control;
                this.lb_BeingOver.ForeColor = System.Drawing.SystemColors.HotTrack;
                this.lb_BeingOver.Text = "开始";
                speakTime.Stop();
                sw.Stop();
                sw.Reset();
                SetLB(string.Format("00:00:00"));
            }
        }
        #endregion
        
    }
}
