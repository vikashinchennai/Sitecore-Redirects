using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines.HttpRequest;
using System;
using System.Net;
using System.Web;
using System.Web.Caching;

namespace Sitecore.Feature.Redirects.Pipelines.Base
{
    public class BaseClass : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            if (Context.PageMode.IsExperienceEditor || Context.PageMode.IsPreview)
            {
                return;
            }

            if (Context.Item == null || Context.Database == null)
            {
                return;
            }
            // throw new NotImplementedException();
            ProcessRequest(args);
        }

        public virtual void ProcessRequest(HttpRequestArgs args)
        {

        }

        public int CacheExpiration
        {
            get;set;
        }

        protected string GetRedirectSettingsId
        {
            get
            {
                return  Context.Site.Properties["redirectSettingsId"];
            } 
        }

        protected string DatabaseName
        {
            get { return Context.Database.Name; }
        }

       
        protected string SiteName
        {
            get { return Context.Site.Name; }
        }

        protected string SiteLanguage
        {
            get { return Context.Language.Name; }
        }

        protected Item GetItem(string itemId)
        {
            //var redirectSettingsId = ;
            if (!string.IsNullOrEmpty(itemId) && ID.TryParse(itemId, out ID iD))
            {
                return Context.Database.GetItem(iD);
            }
            return null;
        }

        protected string ResolvedRedirectPrefix
        {
            get
            {
                return string.Format("{0}ResolvedRedirect-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }
        protected string AllRedirectMappingsPrefix
        {
            get
            {
                return string.Format("{0}AllRedirectMappings-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }
        protected string AllMappingsPrefix
        {
            get
            {
                return string.Format("{0}AllMappings-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }

        protected string ResolvedMappingsPrefix
        {
            get
            {
                return string.Format("{0}ResolvedMappings-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }

        protected string SiteLevelMappingPrefix
        {
            get
            {
                return string.Format("{0}SiteLevel-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }

        protected string SiteLevelAllMappingPrefix
        {
            get
            {
                return string.Format("{0}SiteLevelAllMappings-{1}-{2}-{3}", "Sitecore-Redirect-", DatabaseName, SiteName, SiteLanguage);
            }
        }

        protected string EnsureSlashes(string text)
        {
            return StringUtil.EnsurePostfix('/', StringUtil.EnsurePrefix('/', text));
        }

        protected T GetCache<T>(string key)
        {
            return (T)HttpRuntime.Cache[key];
        }

        protected void SetCache(string key, object value)
        {
            Cache cache = HttpRuntime.Cache;
            DateTime utcNow = DateTime.UtcNow;
            cache.Add(key, value, null, utcNow.AddMinutes((double)this.CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
        }

        protected virtual void Redirect301(HttpResponse response, string url)
        {
            HttpCookieCollection httpCookieCollection = new HttpCookieCollection();
            for (int i = 0; i < response.Cookies.Count; i++)
            {
                HttpCookie item = response.Cookies[i];
                if (item != null)
                {
                    httpCookieCollection.Add(item);
                }
            }
            response.Clear();
            for (int j = 0; j < httpCookieCollection.Count; j++)
            {
                HttpCookie httpCookie = httpCookieCollection[j];
                if (httpCookie != null)
                {
                    response.Cookies.Add(httpCookie);
                }
            }

            response.Status = "301 Moved Permanently";
            response.StatusCode = (int)HttpStatusCode.MovedPermanently;
            response.AppendHeader("Location", url); 
            response.Flush();
            response.End();
        }

    }
}