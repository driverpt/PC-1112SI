using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ConfinementDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BackgroundWork()
        {
            Thread.Sleep(10000);
            this.label1.BeginInvoke(new ThreadStart(this.Completion));
        }

        private void Completion()
        {
            label1.Text = "Done!";
        }

        private void doIt_Click(object sender, EventArgs e)
        {
            Console.WriteLine("In doItClick: " + Thread.CurrentThread.ManagedThreadId);
            label1.Text = "Doing It...";
            BackgroundWorker bck = new BackgroundWorker();
            bck.DoWork += new DoWorkEventHandler(bck_DoWork);
            bck.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bck_RunWorkerCompleted);
            bck.RunWorkerAsync();
            //new Thread(() =>
            //               {
            //                   Thread.Sleep(10000);
            //                   this.label1.BeginInvoke(
            //                       new ThreadStart(() => label1.Text = "Done!")
            //                   );
            //               }).Start();
            //new Thread(this.BackgroundWork).Start();
        }

        void bck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("In Completed: " + Thread.CurrentThread.ManagedThreadId);
            label1.Text = "Done!";
        }

        void bck_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("In DoWork: " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(10000);
        }
    }
}
