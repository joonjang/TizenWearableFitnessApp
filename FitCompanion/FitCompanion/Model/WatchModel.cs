using System;
using System.Collections.Generic;
using System.Text;

namespace FitCompanion.Model
{
    public class WatchModel
    {
        public string Week { get; set; }
        public string Day { get; set; }
        public List<string> Sets { get; set; }
        public List<string> Workouts { get; set; }
    }
}
