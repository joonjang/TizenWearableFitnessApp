using System;
using System.Collections.Generic;
using System.Text;

namespace FitCompanion
{
    public interface IProviderServiceSocket
    {
        void OnError(int p0, string p1, int p2);
        void OnReceive(int p0, byte[] p1);
    }
}
