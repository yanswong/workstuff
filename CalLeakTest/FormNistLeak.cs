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
    public partial class FormNistLeak : Form
    {
        public FormNistLeak()
        {
            InitializeComponent();
        }

        private void FormNistLeak_Load(object sender, EventArgs e)
        {

        }

        private void TxtBoxLeakNumberLeave(object sender, EventArgs e)
        {
            try
            {
                TextBox txtBox = (TextBox)sender;
                txtBox.Text = txtBox.Text.ToUpper();
            }
            catch (Exception)
            {
            }
        }

        private void TxtBoxKeypressed(object sender, KeyPressEventArgs e)
        {
            try
            {

            }
            catch (Exception)
            {
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {

        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
