using LICommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LICommon
{
    public class LISearchURLGenerator
    {

        public static string URLGenerator(SearchCriteria sc, int appPageNo, int batchSize, out int liPageNo)
        {
            liPageNo = appPageNo;

            StringBuilder sb = null;


            sb = new StringBuilder("https://www.linkedin.com/search/results/people/?keywords=");

            if (!string.IsNullOrWhiteSpace(sc.Company))
            {
                sb.Append(sc.Company);
                sb.Append(" ");
            }

            if (!string.IsNullOrWhiteSpace(sc.Keyword))
            {
                
                sb.Append(sc.Keyword);
                sb.Append(" ");
            }

            if (!string.IsNullOrWhiteSpace(sc.City))
            {
                sb.Append(sc.City);
                sb.Append(" ");
            }

            if (!string.IsNullOrWhiteSpace(sc.State))
            {
                sb.Append(sc.State);
                sb.Append(" ");
            }

            sb.Length--;

            sb.Append("&origin=GLOBAL_SEARCH_HEADER");


            if (appPageNo > 1)
            {
                double d = batchSize * (appPageNo - 1);
                int np = (int)Math.Ceiling(d / 10.0);
                liPageNo = np + 1;
                sb.Append($"&page={liPageNo}");

            }

            return sb.ToString();
        }



    }
}
