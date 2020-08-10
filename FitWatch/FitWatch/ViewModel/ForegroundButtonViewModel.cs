using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace FitWatch.ViewModel
{
    class ForegroundButtonViewModel
    {
        public ICommand StopForegroundCommand { get; }
        public ForegroundButtonViewModel()
        {
            StopForegroundCommand = new Command(StopRunningForegroundMethod);
        }

        void StopRunningForegroundMethod()
        {
            MessagingCenter.Send<ForegroundButtonViewModel>(this, "StopForeground");
        }
    }
}
