using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QRCodeCreator
{
    public class SelectedCellEventArgs : EventArgs
    {
        public string SelectedValue { get; private set; }

        public SelectedCellEventArgs(string value)
        {
            this.SelectedValue = value;
        }
    }
}
