using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Websocket.Client;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using PumpAndDump.Shared;

namespace PumpAndDump.Windows
{
    public partial class Form1 : Form
    {
        bool isSubscribe = true;
        MainProcess _mainProcess = new MainProcess();

        private delegate void UpdateTextDel(string text);
        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateText(string text)
        {
            textBox1.Text += text;
            textBox1.Update();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string data;
            if (!isSubscribe)
            {
                await _mainProcess.StartProcess();
            }
            else
            {
                await _mainProcess.EndProcess();
            }
            SetSubscribe();
        }

        private void SetSubscribe()
        {
            isSubscribe = !isSubscribe;
            this.button1.Text = isSubscribe ? "Subscribe" : "Unsubscribe";
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            UpdateTextDel utDel = UpdateText;
            await _mainProcess.LoadProcess(o => textBox1.Invoke(utDel, o));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _mainProcess.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (dataGridView1 == null) return;
            dataGridView1.DataSource = _mainProcess.CompareMarketDic.Select(o => o.Value).OrderByDescending(o => o.DifferencePercentage).ToList();
            dataGridView1.Columns["CheckingTime"].DefaultCellStyle.Format = "G";
            dataGridView1.Update();
        }
    }
}
