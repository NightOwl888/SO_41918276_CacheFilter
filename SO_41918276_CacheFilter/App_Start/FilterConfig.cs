using System.Web;
using System.Web.Mvc;
using SO_41918276_CacheFilter.Filters;

namespace SO_41918276_CacheFilter
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyCacheFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}