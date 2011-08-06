using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitTrader
{
    class Exchange
    {
        protected double[] balance;

        public string UserName;
        public string Password;

        virtual public void Connect()
        {
        }

        virtual public void Disconnect()
        {
        }

        virtual public void UpdateBalance()
        {
        }

        public double GetBalance(int i)
        {
            return balance[i];
        }
    }
}
