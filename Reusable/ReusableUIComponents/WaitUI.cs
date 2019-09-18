using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableUIComponents.Dialogs;
using Timer = System.Windows.Forms.Timer;

namespace ReusableUIComponents
{
    public sealed partial class WaitUI : Form
    {
        private readonly string _caption;
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource;
        Timer timer = new Timer();

        public WaitUI(string caption, Task task, CancellationTokenSource cancellationTokenSource)
        {
            _caption = caption;
            _task = task;
            _cancellationTokenSource = cancellationTokenSource;
            InitializeComponent();

            label1.Text = caption;
            this.SetClientSizeCore(pictureBox1.Width + label1.PreferredWidth + button1.Width + 10, button1.Height);

            button1.Left = label1.Right;
            timer.Interval = 500;
            timer.Start();
            timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_task.IsCompleted)
            {
                timer.Stop();
                this.Close();
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
            button1.Enabled = false;
        }
    }
}
