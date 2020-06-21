using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon.Models
{
    public class ScrapingResult
    {
        public List<LIUserData> UserProfiles { get; set; }
        public bool AreThereMoreRecords { get; set; }
    }
}
