using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FitWatch
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppShell : CircularShell
    {
        public AppShell()
        {
            InitializeComponent();
          
        }
    }
}