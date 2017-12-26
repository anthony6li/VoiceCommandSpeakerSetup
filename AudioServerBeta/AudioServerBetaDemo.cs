using System;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Timers;
using Timer = System.Timers.Timer;
using System.Diagnostics;

namespace AudioServerBeta
{
    public partial class AudioServerBetaDemo : Form
    {
        bool beginMove = false;
        int currentXPosition;
        int currentYPosition;
        bool isBegin = false;

        string[] arguments = null;
        string ip = string.Empty;
        string name = string.Empty;


        Stopwatch sw;
        TimeSpan ts;
        private Timer speakTime;
        private delegate void SetLBTime(string value);

        public AudioServerBetaDemo(string[] args)
        {
            arguments = args;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
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
        private void pl_BeginOver_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isBegin)
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.MenuHighlight;
                this.lb_BeingOver.ForeColor = System.Drawing.Color.White;
                isBegin = true;
                speakTime.Elapsed += SpeakTime_Elapsed;
                sw.Start();
                speakTime.Start();
            }
            else
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.Control;
                this.lb_BeingOver.ForeColor = System.Drawing.SystemColors.HotTrack;
                isBegin = false;
                speakTime.Stop();
                sw.Stop();
                sw.Reset();
                SetLB(string.Format("00:00:00"));
            }
        }

        private void lb_BeingOver_MouseClick(object sender, MouseEventArgs e)
        {
            if (!isBegin)
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.MenuHighlight;
                this.lb_BeingOver.ForeColor = System.Drawing.Color.White;
                isBegin = true;
                speakTime.Elapsed += SpeakTime_Elapsed;
                sw.Start();
                speakTime.Start();
            }
            else
            {
                this.pl_BeginOver.BackColor = System.Drawing.SystemColors.Control;
                this.lb_BeingOver.ForeColor = System.Drawing.SystemColors.HotTrack;
                isBegin = false;
                speakTime.Stop();
                sw.Stop();
                sw.Reset();
                SetLB(string.Format("00:00:00"));
            }
        }
        #endregion
    }
}
