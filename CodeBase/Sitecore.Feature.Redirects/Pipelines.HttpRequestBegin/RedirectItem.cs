using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Feature.Redirects.Extensions;
using Sitecore.Feature.Redirects.Pipelines.Base;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;
using Sitecore.StringExtensions;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequestBegin
{
    public class RedirectItem : BaseClass
    {
        public RedirectItem()
        {
        }


        protected virtual string GetRedirectUrl(Item redirectItem)
        {
            LinkField item = redirectItem.Fields[Templates.Redirect.Fields.RedirectUrl];
            string str = null;
            if (item != null)
            {
                if (!item.IsInternal || item.TargetItem == null)
                {
                    str = (!item.IsMediaLink || item.TargetItem == null ? item.Url : ((MediaItem)item.TargetItem).GetMediaUrl(null));
                }
                else
                {
                    SiteInfo siteInfo = Context.Site.SiteInfo;
                    UrlOptions defaultOptions = UrlOptions.DefaultOptions;
                    defaultOptions.Site = SiteContextFactory.GetSiteContext(siteInfo.Name);
                    defaultOptions.AlwaysIncludeServerUrl = true;
                    str = string.Concat(LinkManager.GetItemUrl(item.TargetItem, defaultOptions), (string.IsNullOrEmpty(item.QueryString) ? "" : string.Concat("?", item.QueryString)));
                }
            }
            return str;
        }
        protected virtual string GetTargetItemId(Item targetItem)
        {
            LinkField item = targetItem.Fields[Templates.Redirect.Fields.TargetItem];
            string str = string.Empty;
            if (item != null)
            {
                if (item.IsInternal && !string.IsNullOrEmpty(item.Value))
                {
                    str = GetItemIdByPath(item.Value);
                }
            }
            return str;
        }
        public static string GetItemIdByPath(string path)
        {
            var Item = Sitecore.Context.Database.SelectSingleItem(path);
            if (Item != null)
            {
                return Item.ID.ToString();
            }

            return string.Empty;
        }


        protected virtual Redirect GetResolvedMapping(string ItemId)
        {
            var item = GetCache<Dictionary<string, Redirect>>(ResolvedRedirectPrefix);
            if (item == null || !item.ContainsKey(ItemId))
            {
                return null;
            }
            return item[ItemId];
        }

        protected virtual List<Redirect> MappingsMap
        {
            get
            {


                var redirectItems = GetCache<List<Redirect>>(AllRedirectMappingsPrefix);

                if (redirectItems == null)
                {
                    redirectItems = new List<Redirect>();

                    Item item = GetItem(GetRedirectSettingsId);

                    Item item1 = item;
                    if (item1 != null)
                    {
                        Item[] array = (
                            from i in (IEnumerable<Item>)item1.Axes.GetDescendants()
                            where i.InheritsFrom(Templates.Redirect.ID)
                            select i).ToArray<Item>();
                        Array.Sort<Item>(array, new TreeComparer());
                        Item[] itemArray = array;
                        for (int num = 0; num < (int)itemArray.Length; num++)
                        {
                            Item item2 = itemArray[num];
                            var targetId = this.GetTargetItemId(item2);
                            var redirectUrl = this.GetRedirectUrl(item2);
                            if (!string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(redirectUrl))
                            {
                                redirectItems.Add(new Redirect()
                                {
                                    RedirectUrl = redirectUrl,
                                    TargetItemId = targetId
                                });
                            }

                        }
                    }
                    if (this.CacheExpiration > 0)
                    {
                        SetCache(AllRedirectMappingsPrefix, redirectItems);
                    }
                }
                return redirectItems;
            }
        }
        protected virtual Redirect FindMapping(string itemId)
        {
            Redirect redirectMapping = null;
            List<Redirect>.Enumerator enumerator = this.MappingsMap.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {

                    Redirect current = enumerator.Current;
                    if (itemId == current.TargetItemId)
                    {
                        redirectMapping = current;
                        return redirectMapping;
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            return redirectMapping;
        }

        public override void ProcessRequest(HttpRequestArgs args)
        {
            //Check https & www is enabled and apply
            var siteLevel = SiteLevelConfigMapping;

            if (siteLevel != null)
            {
                var current = HttpContext.Current;
              
                if (current != null && siteLevel.RedirectToHttpsAlways)
                {
                    if (!current.Request.IsSecureConnection)
                    {
                        var url = current.Request.Url.AbsoluteUri;
                        if (url.StartsWith("http:"))
                        {
                            url = url = "https" + url.Remove(0, 4);
                            this.Redirect301(current.Response, url);
                        }
                    }
                }
                if (siteLevel.AddWwwPrefix)
                {
                    var url = current.Request.Url.AbsoluteUri;

                    var urlVal = url.Substring(url.IndexOf("//") + 2);
                    if (!urlVal.StartsWith("www."))
                    {
                        url = (url.Substring(0, url.IndexOf("//") + 2)) + "www." + urlVal;
                        this.Redirect301(current.Response, url);
                    }

                }
            }
            
            string itemId = Context.Item.ID.ToString();
            Redirect resolvedMapping = this.GetResolvedMapping(itemId);

            bool flag = resolvedMapping != null;
            if (resolvedMapping == null)
            {
                resolvedMapping = this.FindMapping(itemId);
            }
            if (resolvedMapping != null && !flag)
            {
                var dictionaryitem = GetCache<Dictionary<string, Redirect>>(ResolvedRedirectPrefix)
                                  ?? new Dictionary<string, Redirect>();

                dictionaryitem[itemId] = resolvedMapping;

                SetCache(ResolvedRedirectPrefix, dictionaryitem);
            }
            if (resolvedMapping != null && HttpContext.Current != null)
            {
                if (!resolvedMapping.RedirectUrl.IsNullOrEmpty())
                {
                    HttpContext.Current.Response.Redirect(resolvedMapping.RedirectUrl, true);
                    args.AbortPipeline();
                }
            }
        }

        protected virtual SiteLevelRedirectMapping SiteLevelConfigMapping
        {
            get
            {
                var siteLevelAllMapping = GetCache<Dictionary<string, SiteLevelRedirectMapping>>(SiteLevelAllMappingPrefix);

                if (siteLevelAllMapping == null || siteLevelAllMapping[SiteLevelMappingPrefix] == null)
                {
                    var siteLevelMapping = new SiteLevelRedirectMapping();

                    Item item = GetItem(GetRedirectSettingsId);

                    Item item1 = item;
                    if (item1 != null && item1.Fields != null)
                    {
                        CheckboxField https = item1.Fields[Templates.SiteLevelMap.Fields.RedirectToHttpsAlways];
                        CheckboxField www = item1.Fields[Templates.SiteLevelMap.Fields.AddWwwPrefix];

                        siteLevelMapping = new SiteLevelRedirectMapping()
                        {
                            AddWwwPrefix = www.Checked,
                            RedirectToHttpsAlways = https.Checked
                        };

                        if (siteLevelAllMapping == null)
                            siteLevelAllMapping = new Dictionary<string, SiteLevelRedirectMapping>();
                        siteLevelAllMapping.Add(SiteLevelMappingPrefix, siteLevelMapping);
                    }
                    if (this.CacheExpiration > 0 && siteLevelAllMapping != null)
                    {
                        SetCache(SiteLevelAllMappingPrefix, siteLevelAllMapping);
                    }
                    return siteLevelMapping;
                }

                return siteLevelAllMapping[SiteLevelMappingPrefix];
            }
        }
    }
}