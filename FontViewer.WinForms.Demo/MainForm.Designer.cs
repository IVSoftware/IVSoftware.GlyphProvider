namespace WinformsFontViewerDemo
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayoutPanel = new FlowLayoutPanel();
            comboBoxConfig = new ComboBox();
            SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.BackColor = SystemColors.Control;
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.Location = new Point(2, 35);
            flowLayoutPanel.Margin = new Padding(0);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(474, 157);
            flowLayoutPanel.TabIndex = 0;
            // 
            // comboBoxConfig
            // 
            comboBoxConfig.Dock = DockStyle.Top;
            comboBoxConfig.FormattingEnabled = true;
            comboBoxConfig.Location = new Point(2, 2);
            comboBoxConfig.Margin = new Padding(3, 3, 3, 10);
            comboBoxConfig.Name = "comboBoxConfig";
            comboBoxConfig.Size = new Size(474, 33);
            comboBoxConfig.TabIndex = 1;
            comboBoxConfig.Visible = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Aqua;
            ClientSize = new Size(478, 194);
            Controls.Add(flowLayoutPanel);
            Controls.Add(comboBoxConfig);
            Name = "MainForm";
            Padding = new Padding(2);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel;
        private ComboBox comboBoxConfig;
    }
}
