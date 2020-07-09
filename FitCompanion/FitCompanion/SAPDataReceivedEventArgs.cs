using System;
using System.Collections.Generic;
using System.Text;

namespace FitCompanion
{
    class SAPDataReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public SAPDataReceivedEventArgs(string msg)
        {
            Message = msg;
        }
    }
}
