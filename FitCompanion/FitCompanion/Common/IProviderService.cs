using System;
using System.Collections.Generic;
using System.Text;

namespace FitCompanion
{
    public interface IProviderService
    {
        bool CloseConnection();
        void SendData(string msg);

    }
}
