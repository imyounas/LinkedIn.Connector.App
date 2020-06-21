using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon.Models
{
    public class LIUserData
    {
        public LIUserData()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string ScraperProfileUrl { get; set; }

        public string ProfileUrl { get; set; }
        public string ProfileTitle { get; set; }
        public string ConnectionDegree { get; set; }
        
        public string Image { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }


        public string CurrentJobTitle { get; set; }
        public string CurrentWorkingTitle { get; set; }

        public string Location { get; set; }
        public string Summary { get; set; }


        
        public DateTime ScrapedAt { get; set; }

        public string CurrentCompany { get; set; }
        

        public bool IsSelected { get; set; }

        
    }
}
