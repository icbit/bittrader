using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;

namespace BitTrader
{
    public partial class MainForm : Form
    {
        MtGox exchange;

        public MainForm()
        {
            InitializeComponent();

            double[] y = new double[4];

            for (int i = 0; i < 40; i++)
            {
                y[0] = 1; // low
                y[1] = 4.2; // high
                y[2] = 1.5; // open
                y[3] = 3; // close

                stockChart.Series[0].Points.Add(y);
            }

            exchange = new MtGox();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            exchange.UserName = txtUsr.Text;
            exchange.Password = txtPass.Text;
            exchange.Connect();
            exchange.UpdateBalance();
            lblusd.Text = String.Format("{0:USD:0.00000000}", exchange.GetBalance(1));
            lblbtc.Text = String.Format("{0:BTC:0.00000000}", exchange.GetBalance(0));
        }
    }
}
