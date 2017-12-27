namespace AudioServerBeta
{
    partial class AudioServerBetaDemo
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AudioServerBetaDemo));
            this.btn_min = new System.Windows.Forms.Button();
            this.btn_closeForm = new System.Windows.Forms.Button();
            this.pl_BeginOver = new System.Windows.Forms.Panel();
            this.lb_BeingOver = new System.Windows.Forms.Label();
            this.pl_TalkF2 = new System.Windows.Forms.Panel();
            this.lb_TalkF2 = new System.Windows.Forms.Label();
            this.lb_Time = new System.Windows.Forms.Label();
            this.pl_CenterCircle = new System.Windows.Forms.Panel();
            this.pl_BeginOver.SuspendLayout();
            this.pl_TalkF2.SuspendLayout();
            this.pl_CenterCircle.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_min
            // 
            this.btn_min.FlatAppearance.BorderSize = 0;
            this.btn_min.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_min.Image = global::AudioServerBeta.Properties.Resources.btn_min;
            this.btn_min.Location = new System.Drawing.Point(569, 1);
            this.btn_min.Name = "btn_min";
            this.btn_min.Size = new System.Drawing.Size(30, 30);
            this.btn_min.TabIndex = 0;
            this.btn_min.UseVisualStyleBackColor = true;
            this.btn_min.Click += new System.EventHandler(this.btn_min_Click);
            // 
            // btn_closeForm
            // 
            this.btn_closeForm.FlatAppearance.BorderSize = 0;
            this.btn_closeForm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_closeForm.Image = global::AudioServerBeta.Properties.Resources.btn_close;
            this.btn_closeForm.Location = new System.Drawing.Point(605, 1);
            this.btn_closeForm.Name = "btn_closeForm";
            this.btn_closeForm.Size = new System.Drawing.Size(30, 30);
            this.btn_closeForm.TabIndex = 0;
            this.btn_closeForm.UseVisualStyleBackColor = true;
            this.btn_closeForm.Click += new System.EventHandler(this.btn_closeForm_Click);
            // 
            // pl_BeginOver
            // 
            this.pl_BeginOver.BackColor = System.Drawing.SystemColors.Control;
            this.pl_BeginOver.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pl_BeginOver.Controls.Add(this.lb_BeingOver);
            this.pl_BeginOver.Location = new System.Drawing.Point(175, 232);
            this.pl_BeginOver.Name = "pl_BeginOver";
            this.pl_BeginOver.Size = new System.Drawing.Size(140, 40);
            this.pl_BeginOver.TabIndex = 2;
            this.pl_BeginOver.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lb_BeingOver_MouseClick);
            // 
            // lb_BeingOver
            // 
            this.lb_BeingOver.AutoSize = true;
            this.lb_BeingOver.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_BeingOver.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lb_BeingOver.Location = new System.Drawing.Point(45, 5);
            this.lb_BeingOver.Name = "lb_BeingOver";
            this.lb_BeingOver.Size = new System.Drawing.Size(52, 27);
            this.lb_BeingOver.TabIndex = 0;
            this.lb_BeingOver.Text = "开始";
            this.lb_BeingOver.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lb_BeingOver.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lb_BeingOver_MouseClick);
            // 
            // pl_TalkF2
            // 
            this.pl_TalkF2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pl_TalkF2.Controls.Add(this.lb_TalkF2);
            this.pl_TalkF2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pl_TalkF2.Location = new System.Drawing.Point(314, 232);
            this.pl_TalkF2.Name = "pl_TalkF2";
            this.pl_TalkF2.Size = new System.Drawing.Size(140, 40);
            this.pl_TalkF2.TabIndex = 2;
            // 
            // lb_TalkF2
            // 
            this.lb_TalkF2.AutoSize = true;
            this.lb_TalkF2.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_TalkF2.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lb_TalkF2.Location = new System.Drawing.Point(2, 5);
            this.lb_TalkF2.Name = "lb_TalkF2";
            this.lb_TalkF2.Size = new System.Drawing.Size(135, 27);
            this.lb_TalkF2.TabIndex = 0;
            this.lb_TalkF2.Text = "按下F2键发言";
            this.lb_TalkF2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_Time
            // 
            this.lb_Time.AutoSize = true;
            this.lb_Time.BackColor = System.Drawing.Color.Transparent;
            this.lb_Time.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_Time.Font = new System.Drawing.Font("微軟正黑體", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lb_Time.ForeColor = System.Drawing.Color.White;
            this.lb_Time.Location = new System.Drawing.Point(42, 79);
            this.lb_Time.Name = "lb_Time";
            this.lb_Time.Size = new System.Drawing.Size(125, 35);
            this.lb_Time.TabIndex = 3;
            this.lb_Time.Text = "00:00:00";
            this.lb_Time.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pl_CenterCircle
            // 
            this.pl_CenterCircle.BackColor = System.Drawing.Color.Transparent;
            this.pl_CenterCircle.BackgroundImage = global::AudioServerBeta.Properties.Resources.jtjht_03;
            this.pl_CenterCircle.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pl_CenterCircle.Controls.Add(this.lb_Time);
            this.pl_CenterCircle.Location = new System.Drawing.Point(217, 28);
            this.pl_CenterCircle.Name = "pl_CenterCircle";
            this.pl_CenterCircle.Size = new System.Drawing.Size(208, 190);
            this.pl_CenterCircle.TabIndex = 4;
            // 
            // AudioServerBetaDemo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::AudioServerBeta.Properties.Resources.yhtfj_02;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(634, 281);
            this.Controls.Add(this.pl_CenterCircle);
            this.Controls.Add(this.pl_TalkF2);
            this.Controls.Add(this.pl_BeginOver);
            this.Controls.Add(this.btn_closeForm);
            this.Controls.Add(this.btn_min);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AudioServerBetaDemo";
            this.Text = "语音指挥";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AudioServerBetaDemo_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.AudioServerBetaDemo_MouseDown);
            this.MouseLeave += new System.EventHandler(this.AudioServerBetaDemo_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.AudioServerBetaDemo_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.AudioServerBetaDemo_MouseUp);
            this.pl_BeginOver.ResumeLayout(false);
            this.pl_BeginOver.PerformLayout();
            this.pl_TalkF2.ResumeLayout(false);
            this.pl_TalkF2.PerformLayout();
            this.pl_CenterCircle.ResumeLayout(false);
            this.pl_CenterCircle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_min;
        private System.Windows.Forms.Button btn_closeForm;
        private System.Windows.Forms.Panel pl_BeginOver;
        private System.Windows.Forms.Label lb_BeingOver;
        private System.Windows.Forms.Panel pl_TalkF2;
        private System.Windows.Forms.Label lb_TalkF2;
        private System.Windows.Forms.Label lb_Time;
        private System.Windows.Forms.Panel pl_CenterCircle;
    }
}

