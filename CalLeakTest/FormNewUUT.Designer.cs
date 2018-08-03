namespace CalLeakTest
{
    partial class FormNewUUT
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblOptionNumber = new System.Windows.Forms.Label();
            this.tbSernum = new System.Windows.Forms.TextBox();
            this.lblSerialNumber = new System.Windows.Forms.Label();
            this.lblModelNumber = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.tbOption = new System.Windows.Forms.TextBox();
            this.cbModelNumber = new System.Windows.Forms.ComboBox();
            this.cbTestPort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblOptionNumber
            // 
            this.lblOptionNumber.AutoSize = true;
            this.lblOptionNumber.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOptionNumber.Location = new System.Drawing.Point(46, 111);
            this.lblOptionNumber.Name = "lblOptionNumber";
            this.lblOptionNumber.Size = new System.Drawing.Size(132, 19);
            this.lblOptionNumber.TabIndex = 14;
            this.lblOptionNumber.Text = "Option                  :";
            // 
            // tbSernum
            // 
            this.tbSernum.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.tbSernum.Location = new System.Drawing.Point(186, 66);
            this.tbSernum.Name = "tbSernum";
            this.tbSernum.Size = new System.Drawing.Size(188, 27);
            this.tbSernum.TabIndex = 1;
            // 
            // lblSerialNumber
            // 
            this.lblSerialNumber.AutoSize = true;
            this.lblSerialNumber.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSerialNumber.Location = new System.Drawing.Point(46, 69);
            this.lblSerialNumber.Name = "lblSerialNumber";
            this.lblSerialNumber.Size = new System.Drawing.Size(130, 19);
            this.lblSerialNumber.TabIndex = 9;
            this.lblSerialNumber.Text = "UUT Tracking #    :";
            // 
            // lblModelNumber
            // 
            this.lblModelNumber.AutoSize = true;
            this.lblModelNumber.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblModelNumber.Location = new System.Drawing.Point(47, 26);
            this.lblModelNumber.Name = "lblModelNumber";
            this.lblModelNumber.Size = new System.Drawing.Size(129, 19);
            this.lblModelNumber.TabIndex = 10;
            this.lblModelNumber.Text = "Model                  :";
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(249, 213);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 32);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.btnOk.Location = new System.Drawing.Point(95, 213);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(88, 32);
            this.btnOk.TabIndex = 12;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // tbOption
            // 
            this.tbOption.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.tbOption.Location = new System.Drawing.Point(186, 108);
            this.tbOption.Name = "tbOption";
            this.tbOption.Size = new System.Drawing.Size(188, 27);
            this.tbOption.TabIndex = 2;
            this.tbOption.Text = "NA";
            // 
            // cbModelNumber
            // 
            this.cbModelNumber.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbModelNumber.FormattingEnabled = true;
            this.cbModelNumber.Items.AddRange(new object[] {
            "R2015301"});
            this.cbModelNumber.Location = new System.Drawing.Point(186, 23);
            this.cbModelNumber.Name = "cbModelNumber";
            this.cbModelNumber.Size = new System.Drawing.Size(188, 27);
            this.cbModelNumber.TabIndex = 0;
            this.cbModelNumber.SelectedIndexChanged += new System.EventHandler(this.cbModelNumber_SelectedIndexChanged);
            // 
            // cbTestPort
            // 
            this.cbTestPort.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbTestPort.FormattingEnabled = true;
            this.cbTestPort.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4"});
            this.cbTestPort.Location = new System.Drawing.Point(186, 152);
            this.cbTestPort.Name = "cbTestPort";
            this.cbTestPort.Size = new System.Drawing.Size(188, 27);
            this.cbTestPort.TabIndex = 3;
            this.cbTestPort.Text = "1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(45, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 19);
            this.label1.TabIndex = 16;
            this.label1.Text = "Test Port #            :";
            // 
            // FormNewUUT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 267);
            this.Controls.Add(this.cbTestPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbModelNumber);
            this.Controls.Add(this.tbOption);
            this.Controls.Add(this.lblOptionNumber);
            this.Controls.Add(this.tbSernum);
            this.Controls.Add(this.lblSerialNumber);
            this.Controls.Add(this.lblModelNumber);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Name = "FormNewUUT";
            this.Text = "New UUT";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormNewDut_FormClosing);
            this.Load += new System.EventHandler(this.FormNewUUT_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOptionNumber;
        private System.Windows.Forms.TextBox tbSernum;
        private System.Windows.Forms.Label lblSerialNumber;
        private System.Windows.Forms.Label lblModelNumber;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox tbOption;
        private System.Windows.Forms.ComboBox cbModelNumber;
        private System.Windows.Forms.ComboBox cbTestPort;
        private System.Windows.Forms.Label label1;
    }
}