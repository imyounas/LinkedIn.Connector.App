using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon.Models
{
    public class LIUserConnectRequestReport: LIUserData
    {
        
        public string ConnectMessage { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Errors { get; set; }
        public string Comments { get; set; }
    }
}
