using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SO_41918276_CacheFilter.Filters
{
    public class MyCacheFilter : IActionFilter
    {
        /// <summary>
        /// The cache key that is used to store/retrieve your default values.
        /// </summary>
        private static string MY_DEFAULTS_CACHE_KEY = "MY_DEFAULTS";

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Do nothing
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cache = filterContext.HttpContext.Cache;

            // This method is called for each request. We check to ensure the cache
            // is initialized, and if not, load the values into it.
            IDictionary<string, string> defaults = cache[MY_DEFAULTS_CACHE_KEY] as IDictionary<string, string>;
            if (defaults == null)
            {
                // The value doesn't exist in the cache, load it
                defaults = GetDefaults();

                // Store the defaults in the cache
                cache.Insert(
                    MY_DEFAULTS_CACHE_KEY,
                    defaults,
                    null,
                    DateTime.Now.AddHours(1), // Cache for exactly 1 hour from now
                    System.Web.Caching.Cache.NoSlidingExpiration);
            }

            // Caching work is done, now return the result to the view. We can
            // do that by storing it in the request cache.
            filterContext.HttpContext.SetMyDefaults(defaults);
        }

        private IDictionary<string, string> GetDefaults()
        {
            // You weren't specific about where your defaults data is coming from
            // or even what data type it is, but you can load it from anywhere in this method
            // and return any data type. The type returned should either by readonly or thread safe.
            var defaults = new Dictionary<string, string>
            {
                { "value1", "testing" },
                { "value2", "hello world" },
                { "value3", "this works" }
            };


            // IMPORTANT: Cached data is shared throughout the application. You should make
            // sure the data structure that holds is readonly so it cannot be updated.
            // Alternatively, you could make it a thread-safe dictionary (such as ConcurrentDictionary),
            // so it can be updated and the updates will be shared between all users.
            // I am showing a readonly dictionary because it is the safest and simplest way.
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(defaults);
        }
    }
}

/// <summary>
/// Extensions for convenience of using the request cache in views and filters.
/// Note this is placed in the global namespace so you don't have to import it in your views.
/// </summary>
public static class HttpContextBaseExtensions
{
    /// <summary>
    /// The key that is used to store your context values in the current request cache.
    /// The request cache is simply used here to transfer the cached data to the view.
    /// The difference between the request cache (HttpContext.Items) and HttpContext.Cache is that HttpContext.Items
    /// is immediately released at the end of the request. HttpContext.Cache is stored (in RAM) for the length of
    /// the timeout (or alternatively, using a sliding expiration that keeps it alive for X time after 
    /// the most recent request for it).
    /// 
    /// Note that by using a reference type
    /// this is very efficient. We aren't storing a copy of the data in the request cache, we
    /// are simply storing a pointer to the same object that exists in the cache.
    /// </summary>
    internal static string MY_DEFAULTS_KEY = "MY_DEFAULTS";


    /// <summary>
    /// This is a convenience method so we don't need to scatter the reference to the request cache key
    /// all over the application. It also makes our cache type safe.
    /// </summary>
    public static string GetMyDefault(this HttpContextBase context, string defaultKey)
    {
        // Get the defaults from the request cache.
        IDictionary<string, string> defaults = context.Items[MY_DEFAULTS_KEY] as IDictionary<string, string>;

        // Get the specific value out of the cache that was requested.
        // TryGetValue() is used to prevent an exception from being thrown if the key doesn't
        // exist. In that case, the result will be null
        string result = null;
        defaults.TryGetValue(defaultKey, out result);

        return result ?? String.Empty;
    }

    /// <summary>
    /// This is a convenience method so we don't need to scatter the reference to the request cache key
    /// all over the application. It also makes our cache type safe.
    /// </summary>
    internal static void SetMyDefaults(this HttpContextBase context, IDictionary<string, string> defaults)
    {
        context.Items[MY_DEFAULTS_KEY] = defaults;
    }
}