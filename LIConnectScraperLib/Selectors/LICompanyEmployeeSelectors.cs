using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIConnectScraperLib.Selectors
{
   public class LICompanyEmployeeSelectors
    {
        public static string CompanyEmployeeParentElementCSSSelLoop = "div.search-result__wrapper";
        public static string ParentElementCSSSel = "div.search-result__info";

        public static string EmployeeNameCSSSelIdx = "span.actor-name";
        public static string EmployeeDesignationCSSSelIdx = "p.subline-level-1";
        public static string EmployeeSRDesignationCSSSelIdx = ".search-result__snippets.mt2";
        public static string EmployeeConnectionDegreeCSSSelIdx = "span.distance-badge > span.dist-value";
        public static string EmployeeLocationCSSSelIdx = "p.subline-level-2";

        public static string EmployeeProfileUrlCSSSelIdx = "div.search-result__image-wrapper > a.search-result__result-link.ember-view";
        
        public static string EmployeeImageUrlCSSSelIdx = "figure.search-result__image > img.lazy-image";

        public static string SearchResultCSSSel = "div.search-results__primary-cluster";
        public static string PagerCollectionCSSSel = "li.page-list > ol > li";

        public static string NextPageSelector = "button.next";
    }
}
