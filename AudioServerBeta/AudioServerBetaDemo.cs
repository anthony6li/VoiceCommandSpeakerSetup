using System;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using NAudio.Wave;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text.RegularExpressions;

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
        private List<ListItem> ddlAudioOut = new List<ListItem>();
        private string clientIP = string.Empty;

        Stopwatch sw;
        TimeSpan ts;
        private Timer speakTime;
        private delegate void SetLBTime(string value);

        #region  F2热键处理
        public bool ifF2Press = false;
        public DateTime ifF2PressTime = new DateTime();
        public static bool ifF2PressProsessing = false;
        private Timer _updateTime;
        private Timer checkF2HotKey;
        private delegate void handleF2PressDelegate();
        private delegate void handleF2PressProcessingDelegate();
        private delegate void handleRichTextDelegate(string message);
        #endregion

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
            try
            {
                string regIP = @"^(?:(?:1[0-9][0-9]\.)|(?:2[0-4][0-9]\.)|(?:25[0-5]\.)|(?:[1-9][0-9]\.)|(?:[0-9]\.)){3}(?:(?:1[0-9][0-9])|(?:2[0-4][0-9])|(?:25[0-5])|(?:[1-9][0-9])|(?:[0-9]))$";
                string[] aa = args[0].Substring(args[0].IndexOf("//", StringComparison.Ordinal)).TrimEnd('”').Trim('/').Trim('"').Trim('?').Split('&');
                clientIP = aa[0].Split(':')[1];
                if (!Regex.IsMatch(clientIP, regIP))
                {
                    clientIP = string.Empty;
                }
            }
            catch (Exception e)
            {
                clientIP = string.Empty;
            }
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
                //设置F2热键的ID为100，注册热键
                HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.None, Keys.F2);
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

                //用来记录F2按下的时间
                checkF2HotKey = new Timer(500);
                checkF2HotKey.AutoReset = true;
                checkF2HotKey.Elapsed += CheckF2HotKey_Elapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AudioServerBetaDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            HotKey.UnregisterHotKey(Handle, 100);
            Application.Exit();
        }

        #region 计时器功能   
        private void CheckF2HotKey_Elapsed(object sender, ElapsedEventArgs e)
        {
            handleF2PressProsessingSetFalse();
        }

        /// <summary>
        /// 判定在经过一个Form Timer周期，F2键依然判定为弹起，结束F2长按处理Timer
        /// </summary>
        private void handleF2PressProsessingSetFalse()
        {
            if (this.InvokeRequired)
            {
                handleF2PressProcessingDelegate hf2 = new handleF2PressProcessingDelegate(handleF2PressProsessingSetFalse);
                this.Invoke(hf2);
            }
            else
            {
                if (ifF2Press)
                {
                    TimeSpan ts = DateTime.Now - ifF2PressTime;
                    if (ts.TotalMilliseconds > 100)
                    {
                        AudioServerBetaDemo.ifF2PressProsessing = false;
                        UpdateF2Button(false);
                        checkF2HotKey.Stop();
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:
                            handleF2PressSetTrue();
                            break;
                        default:
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 截获全局热键F2，开启Timer纪录F2长按事件
        /// </summary>
        private void handleF2PressSetTrue()
        {
            //if (!ifF2Press)
            {
                ifF2Press = true;
                AudioServerBetaDemo.ifF2PressProsessing = true;
                UpdateF2Button(true);
                ifF2PressTime = DateTime.Now;
            }
            if (!checkF2HotKey.Enabled)
            {
                checkF2HotKey.Start();
            }
        }

        private void SpeakTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            ts = sw.Elapsed;
            SetLB(string.Format("{0}:{1}:{2}", ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00")));
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
            try
            {
                UpdateBeginButton(isBegin);
                //string str = "192.168.198.1";
                //string str = "10.10.36.28";
                if (!isBegin)
                {
                    isBegin = true;
                    Mic = AddMicrophone();
                    if (ddlAudioOut.Count > 0 && !string.IsNullOrEmpty(clientIP))
                    {
                        Mic.settings.sourcename = clientIP;
                        Mic.settings.active = true;
                        Mic.settings.deviceout = ((ListItem)ddlAudioOut[0]).Value[0];
                        MicVolumeLevel = new SendVolumeLevel(Mic);
                        MicVolumeLevel.AudioMode = 0;
                        MicVolumeLevel.Enable();
                    }
                }
                else
                {
                    isBegin = false;
                    if (MicVolumeLevel != null)
                    {
                        //断开语音传输
                        MicVolumeLevel.Disable();
                    }
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
            catch (Exception be)
            {
                MessageBox.Show(be.Message);
            }
        }

        private void UpdateBeginButton(bool begin)
        {
            try
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
            catch (Exception ubb)
            {
                MessageBox.Show(ubb.Message);
            }

        }

        private void UpdateF2Button(bool isPress)
        {
            try
            {
                if (isPress)
                {
                    this.pl_TalkF2.BackColor = System.Drawing.SystemColors.MenuHighlight;
                    this.lb_TalkF2.ForeColor = System.Drawing.Color.White;
                    this.lb_TalkF2.Text = "发言中...";
                }
                else
                {
                    this.pl_TalkF2.BackColor = System.Drawing.SystemColors.Control;
                    this.lb_TalkF2.ForeColor = System.Drawing.SystemColors.HotTrack;
                    this.lb_TalkF2.Text = "按F2键发言";
                    this.lb_TalkF2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                }
            }
            catch (Exception ufb)
            {
                MessageBox.Show(ufb.Message);
            }
        }
        #endregion

    }
}
