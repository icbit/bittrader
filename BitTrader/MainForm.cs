using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
//using System.ServiceModel.WebSockets;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace BitTrader
{
    public partial class MainForm : Form
    {
        MtGox exchange;
        WebSocket ws;

        private delegate void UpdateOrderBook(JObject o);

        private void UpdateOrderBookMethod(JObject o)
        {
            // Scan to check if we have such entry already
            int i;

            double price = Int32.Parse(o["depth"]["price_int"].ToString()) / 100000.0f;
            //int price = Int32.Parse(o["depth"]["price_int"].ToString());
            double qty = Double.Parse(o["depth"]["volume_int"].ToString()) / 1e8;

            // Determine column index accroding to bid/ask type
            int cind;
            if (o["depth"]["type_str"].ToString() == "ask")
                cind = 0;
            else
                cind = 2;

            bool found = false;
            for (i = 0; i < dataOrderBook.Rows.Count; i++)
            {
                if (dataOrderBook.Rows[i].Cells[cind].Value != null &&
                    (double)dataOrderBook.Rows[i].Cells[1].Value == price)
                {
                    double newValue = (double)dataOrderBook.Rows[i].Cells[cind].Value + qty;

                    if (newValue > 0)
                    {
                        dataOrderBook.Rows[i].Cells[cind].Value = newValue;
                    }
                    else
                    {
                        // Delete this row
                        dataOrderBook.Rows.Remove(dataOrderBook.Rows[i]);
                    }

                    found = true;
                    break;
                }
            }

            // Add it if it wasn't found
            if (!found && qty > 0)
            {
                int ind = 0;

                if (dataOrderBook.Rows.Count > 0)
                {
                    ind = 0;

                    // This is a bid type of order, so scan bottom right part
                    for (i = 0; i < dataOrderBook.Rows.Count; i++)
                    {
                        // Stop if we encounter "ask" type of orders already
                        //if (dataOrderBook.Rows[i].Cells[0].Value != null) break;

                        if ((double)dataOrderBook.Rows[i].Cells[1].Value < price)
                        {
                            // found it
                            ind = i;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        dataOrderBook.Rows.Insert(ind, 1);
                    else
                        ind = dataOrderBook.Rows.Add();
                }
                else
                {
                    // It's the first row
                    ind = dataOrderBook.Rows.Add();
                }

                dataOrderBook.Rows[ind].Cells[cind].Value = qty; // Quantity
                dataOrderBook.Rows[ind].Cells[1].Value = price; // Price
            }
        }

        private void WebSocketOnData(object sender, string eventdata)
        {
            //try
            {
                // Try to parse JSON crap
                JObject o = JObject.Parse(eventdata);
                string channel = o["channel"].ToString();

                // Don't process subscription messages
                if (o["op"].ToString() == "subscribe")
                    return;

                switch (channel)
                {
                    case "24e67e0d-1cad-4cc0-9e7a-f8523ef460fe":
                        // Order book update
                        //["depth"]["currency", "item", "price_int", "type_str", "volume_int"]

                        dataOrderBook.Invoke(new UpdateOrderBook(UpdateOrderBookMethod), new Object[] {o});
                    break;

                    default:
                        // Ignore other data for now
                    break;
                }
            }
            //catch
            //{
                // Ignore this exception
            //}
        }

        private void WebSocketOnOpen(object sender, EventArgs e)
        {
            int a = 5;
        }

        private void WebSocketOnError(object sender, string s)
        {
            int a = 5;
            Console.WriteLine("[WebSocket] Error  : {0}", s);
        }

        public MainForm()
        {
            InitializeComponent();

            double[] y = new double[4];

            /*for (int i = 0; i < 40; i++)
            {
                y[0] = 1; // low
                y[1] = 4.2; // high
                y[2] = 1.5; // open
                y[3] = 3; // close

                stockChart.Series[0].Points.Add(y);
            }*/

            // Create the exchange object
            exchange = new MtGox();

            /*ws = new WebSocket("ws://websocket.mtgox.com:80/mtgox");
            ws.OnData += new EventHandler<WebSocketEventArgs>(WebSocketOnData);
            ws.OnOpen += new EventHandler<EventArgs>(WebSocketOnOpen);
            ws.Open();*/

            ws = new WebSocket("ws://websocket.mtgox.com:80/mtgox", "none");

            // Set handlers
            ws.OnOpen += new EventHandler(WebSocketOnOpen);
            ws.OnMessage += new MessageEventHandler(WebSocketOnData);
            ws.OnError += new MessageEventHandler(WebSocketOnError);

            ws.Connect();
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
