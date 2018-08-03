using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CalLeakTest
{
    public partial class TestBlocked : Form
    {
        public string msg { get; set; }
        public TestBlocked()
        {
            InitializeComponent();
        }

        private void TestEquipmentPrompt_Load(object sender, EventArgs e)
        {
            this.CenterToParent();
            this.lblEqList.Text = msg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
