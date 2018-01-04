using System;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Diagnostics;
using System.Reflection;
using NAudio.Wave;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using Anthony.Logger;
using System.Threading;

namespace VoiceCommandSpeaker
{
    public partial class FrmVoiceSpeaker : Form
    {
        private static ARLogger logger = ARLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        bool beginMove = false;
        int currentXPosition;
        int currentYPosition;
        bool isBegin = false;
        private delegate void ChildThreadExceptionHandler(string message);
        private event ChildThreadExceptionHandler ChildThreadException;

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
        public DateTime ifF2FirstPressTime = new DateTime();
        public static bool ifF2PressProsessing = false;
        private Timer checkF2HotKey;
        private delegate void handleHotKey(bool isRegisterHotKey);
        private delegate void handleF2PressProcessingDelegate();
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

        public FrmVoiceSpeaker(string[] args)
        {
            //Form运行在屏幕右下角逻辑
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Width * 2 - 35;
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Height;
            Point p = new Point(x, y);
            this.PointToScreen(p);
            this.Location = p;

            try
            {
                if (args.Count() == 0)
                {
                    string IOFREX_Msg = "未指定目的端IP，程序无法正常运行";
                    throw new IndexOutOfRangeException(IOFREX_Msg);
                }
                string regIP = @"^(?:(?:1[0-9][0-9]\.)|(?:2[0-4][0-9]\.)|(?:25[0-5]\.)|(?:[1-9][0-9]\.)|(?:[0-9]\.)){3}(?:(?:1[0-9][0-9])|(?:2[0-4][0-9])|(?:25[0-5])|(?:[1-9][0-9])|(?:[0-9]))$";
                string[] aa = args[0].Substring(args[0].IndexOf("//", StringComparison.Ordinal)).TrimEnd('”').Trim('/').Trim('"').Trim('?').Split('&');
                clientIP = aa[0].Split(':')[1];
                if (!Regex.IsMatch(clientIP, regIP))
                {
                    clientIP = string.Empty;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                logger.Error("程序未接收正确的IP，退出程序。Exception:{0}", ex.Message);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
                this.Close();
            }
            catch (Exception e)
            {
                logger.Error("程序接收的参数处理异常，参数置空。Exception:{0}", e.Message);
                clientIP = string.Empty;
            }
            InitializeComponent();
            logger.Info("程序加载完毕");
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
            logger.Info(string.Format("AddMicrophone Sucessful"));
            return Mic;
        }

        protected virtual void OnChildThreadException(string message)
        {
            if (ChildThreadException != null)
            {
                ChildThreadException(message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Mic = AddMicrophone();

                //取得输出设备
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
                logger.Error(ex.Message);
            }
        }

        private void VoiceCommandSpeakerDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            logger.Info("取消注册系统热键F2键。");
            HotKey.UnregisterHotKey(Handle, 100);
            if (speakTime != null)
            {
                speakTime.Stop();
            }
            if (sw != null)
            {
                sw.Stop();
            }
            logger.Info("停止计时器。");
            if (MicVolumeLevel != null)
            {
                //断开语音传输
                MicVolumeLevel.Disable();
            }
            Application.Exit();
            logger.Info("程序彻底退出。");
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
                if (FrmVoiceSpeaker.ifF2PressProsessing)
                {
                    TimeSpan ts = DateTime.Now - ifF2PressTime;
                    if (ts.TotalMilliseconds > 500)
                    {
                        logger.Info("本次按键间隔为：{0}分 {1}秒 {2}毫秒。", ts.Minutes, ts.Seconds, ts.Milliseconds);
                        FrmVoiceSpeaker.ifF2PressProsessing = false;
                        //ifF2Press = false;
                        UpdateF2Button(false);
                        checkF2HotKey.Stop();
                        TimeSpan tts = DateTime.Now - ifF2FirstPressTime;
                        logger.Info("松开F2热键，停止发送语音。更新Button样式为白底蓝字。");
                        logger.Info("本次按键持续时间为：{0}分 {1}秒 {2}毫秒。",tts.Minutes,tts.Seconds,tts.Milliseconds);
                        HotKey.UnregisterHotKey(Handle, 100);
                        //UpdateBeginButton(isBegin);
                        logger.Info("松开F2热键之后，取消注册热键。");
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
            ifF2PressTime = DateTime.Now;
            logger.Warn("触发一次F2！" + ifF2PressTime.ToString());
            logger.Info(string.Format("VoiceCommandSpeakerDemo.ifF2PressProsessing is {0}", FrmVoiceSpeaker.ifF2PressProsessing));
            if (!FrmVoiceSpeaker.ifF2PressProsessing)
            {
                //ifF2Press = true;
                FrmVoiceSpeaker.ifF2PressProsessing = true;
                UpdateF2Button(true);
            }
            if (!checkF2HotKey.Enabled)
            {
                logger.Warn("开启F2按下事件的计时器！" + ifF2PressTime.ToString());
                checkF2HotKey.Start();
                ifF2FirstPressTime = ifF2PressTime;
                logger.Info("按下F2热键，发送语音流。更新Button样式为蓝底白字。");
            }
        }

        private void SpeakTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            ts = sw.Elapsed;
            SetLB(string.Format("{0}:{1}:{2}", ts.Hours.ToString("00"), ts.Minutes.ToString("00"), ts.Seconds.ToString("00")));
        }

        private void SetRegisterHotKey(bool isRegisterHotKey)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new handleHotKey(SetRegisterHotKey), isRegisterHotKey);
            }
            else
            {
                if (isRegisterHotKey)
                {
                    logger.Info("注册系统热键F2键。");
                    HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.None, Keys.F2);
                }
                else
                {
                    HotKey.UnregisterHotKey(Handle, 100);
                    UpdateBeginButton(isBegin);
                    logger.Info("松开F2热键之后，取消注册热键。");
                }
            }
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
            logger.Info("响应最小化按钮操作。");
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_closeForm_Click(object sender, EventArgs e)
        {
            logger.Info("响应最小化按钮操作。");
            Application.Exit();
        }

        private void VoiceCommandSpeakerDemo_MouseDown(object sender, MouseEventArgs e)
        {
            //将鼠标坐标赋给窗体左上角坐标  
            beginMove = true;
            currentXPosition = MousePosition.X;
            currentYPosition = MousePosition.Y;
            this.Refresh();
        }

        private void VoiceCommandSpeakerDemo_MouseLeave(object sender, EventArgs e)
        {
            //设置初始状态  
            currentXPosition = 0;
            currentYPosition = 0;
            beginMove = false;
        }

        private void VoiceCommandSpeakerDemo_MouseMove(object sender, MouseEventArgs e)
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

        private void VoiceCommandSpeakerDemo_MouseUp(object sender, MouseEventArgs e)
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
            Thread th = null;
            try
            {
                UpdateBeginButton(isBegin);
                if (!isBegin)
                {
                    //设置F2热键的ID为100，注册热键
                    //logger.Info("注册系统热键F2键。");
                    //HotKey.RegisterHotKey(Handle, 100, HotKey.KeyModifiers.None, Keys.F2);
                    isBegin = true;
                    Mic = AddMicrophone();
                    //if (ddlAudioOut.Count > 0 && !string.IsNullOrEmpty(clientIP))
                    //{
                    //    Mic.settings.sourcename = clientIP;
                    //    Mic.settings.active = true;
                    //    Mic.settings.deviceout = ((ListItem)ddlAudioOut[0]).Value[0];
                    //    MicVolumeLevel = new SendVolumeLevel(Mic);
                    //    MicVolumeLevel.AudioMode = 0;
                    //    MicVolumeLevel.Enable();
                    //}
                    ChildThreadException = null;
                    ChildThreadException += new ChildThreadExceptionHandler(Import_ChildThreadException);
                    th = new Thread(new System.Threading.ThreadStart(MicVolumeLevelEnable));
                    th.IsBackground = true;
                    th.Start();

                }
                else
                {
                    //logger.Info("取消注册系统热键F2键。");
                    //HotKey.UnregisterHotKey(Handle, 100);
                    isBegin = false;
                    if (MicVolumeLevel != null)
                    {
                        //断开语音传输
                        MicVolumeLevel.Disable();
                    }
                    if (th != null)
                    {
                        th.Abort();
                    }

                    speakTime.Stop();
                    sw.Stop();
                    sw.Reset();
                    logger.Info("结束指挥！！！计时器停止。");
                }
            }
            catch (SocketException)
            {
            }
            catch (Exception be)
            {
                logger.Error("指挥操作出现异常。Exception：{0}", be.Message);
            }
        }

        private void Import_ChildThreadException(string message)
        {
            //lb_BeingOver_MouseClick(new object(),null);
            UpdateBeginButton(isBegin);
            if (MicVolumeLevel != null)
            {
                //断开语音传输
                MicVolumeLevel.Disable();
            }
            SetLB("00:00:00");
            speakTime.Stop();
            sw.Stop();
            sw.Reset();
            logger.Info("处理子线程异常！结束指挥！！！计时器停止。");
        }

        private void MicVolumeLevelEnable()
        {
            try
            {
                if (ddlAudioOut.Count > 0 && !string.IsNullOrEmpty(clientIP))
                {
                    SetLB("连接中……");
                    Mic.settings.sourcename = clientIP;
                    Mic.settings.active = true;
                    Mic.settings.deviceout = ((ListItem)ddlAudioOut[0]).Value[0];
                    MicVolumeLevel = new SendVolumeLevel(Mic);
                    MicVolumeLevel.AudioMode = 0;

                    MicVolumeLevel.Enable();
                }
                SetRegisterHotKey(true);
                speakTime.Elapsed += SpeakTime_Elapsed;
                sw.Start();
                speakTime.Start();
                logger.Info("开始指挥！！！计时器开始。");
            }
            catch (SocketException se)
            {
                SetRegisterHotKey(false);
                //SpeakTime_Elapsed(speakTime, new ElapsedEventArgs());
                OnChildThreadException(se.Message);
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
                    logger.Info("更新开始结束Button样式为蓝底白字。");
                }
                else
                {
                    this.pl_BeginOver.BackColor = System.Drawing.SystemColors.Control;
                    this.lb_BeingOver.ForeColor = System.Drawing.SystemColors.HotTrack;
                    this.lb_BeingOver.Text = "开始";
                    SetLB(string.Format("00:00:00"));
                    logger.Info("更新开始结束Button样式为白底蓝字。");
                }
            }
            catch (Exception ubb)
            {
                logger.Error("更新开始结束Button样式。", ubb.Message);
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
                logger.Warn(string.Format("更新F2Button状态出现异常。",ufb.Message) );
            }
        }
        #endregion

    }
}
