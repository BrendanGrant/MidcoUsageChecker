using System;
using System.Collections.Generic;
using System.Text;

namespace MidcoUsageChecker
{
    internal class SimpleLoginRadiusResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public DateTime expires_in { get; set; }
    }
}
