using System;
using System.Collections.Generic;
using System.Text;

namespace MidcoUsageChecker
{
    public class DataUsage
    {
        public List<string> Labels { get; set; }
        public List<double> Downstream { get; set; }
        public List<double> Upstream { get; set; }
    }
}
