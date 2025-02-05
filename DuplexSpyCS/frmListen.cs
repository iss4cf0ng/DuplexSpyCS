﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmListen : Form
    {
        public string ip;
        public int port;

        public frmListen()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            port = int.Parse(textBox1.Text);
            ActiveForm.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ActiveForm.Close();
        }

        private void frmListen_Load(object sender, EventArgs e)
        {
            textBox1.Text = "5000";
        }
    }
}
