using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Redirects.Extensions;
using Sitecore.Feature.Redirects.Pipelines.Base;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Text;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequest
{
    public class RedirectMapResolver : BaseClass
    {
       

        protected virtual List<RedirectMapping> MappingsMap
        {
            get
            {
                RedirectType redirectType;
                var redirectMappings = GetCache<List<RedirectMapping>>(AllMappingsPrefix);
                if (redirectMappings == null)
                {
                    redirectMappings = new List<RedirectMapping>();

                    Item item = GetItem(GetRedirectSettingsId);

                    Item item1 = item;
                    if (item1 != null)
                    {
                        Item[] array = (
                            from i in (IEnumerable<Item>)item1.Axes.GetDescendants()
                            where i.InheritsFrom(Sitecore.Feature.Redirects.Templates.RedirectMap.ID)
                            select i).ToArray<Item>();
                        Array.Sort<Item>(array, new TreeComparer());
                        Item[] itemArray = array;
                        for (int num = 0; num < (int)itemArray.Length; num++)
                        {
                            Item item2 = itemArray[num];
                            if (Enum.TryParse<RedirectType>(item2[Templates.RedirectMap.Fields.RedirectType], out redirectType))
                            {
                                bool flag = MainUtil.GetBool(item2[Templates.RedirectMap.Fields.PreserveQueryString], false);
                                UrlString urlString = new UrlString()
                                {
                                    Query = item2[Templates.RedirectMap.Fields.UrlMapping]
                                };
                                foreach (string key in urlString.Parameters.Keys)
                                {
                                    if (string.IsNullOrEmpty(key))
                                    {
                                        continue;
                                    }
                                    string str = urlString.Parameters[key];
                                    if (string.IsNullOrEmpty(str))
                                    {
                                        continue;
                                    }
                                    string lower = HttpUtility.UrlDecode(key.ToLower(), System.Text.Encoding.UTF8);
                                    bool flag1 = (!lower.StartsWith("^") ? false : lower.EndsWith("$"));
                                    if (!flag1)
                                    {
                                        lower = this.EnsureSlashes(lower);
                                    }
                                    str = HttpUtility.UrlDecode(str.ToLower(), System.Text.Encoding.UTF8);
                                    str = HttpUtility.UrlDecode(str.ToLower(), System.Text.Encoding.UTF8);
                                    str = str.ToLower() ?? string.Empty;
                                    str = str.TrimStart(new char[] { '\u005E' }).TrimEnd(new char[] { '$' });
                                    redirectMappings.Add(new RedirectMapping()
                                    {
                                        RedirectType = redirectType,
                                        PreserveQueryString = flag,
                                        Pattern = lower,
                                        Target = str,
                                        IsRegex = flag1
                                    });
                                }
                            }
                            else
                            {
                                Log.Info(string.Format("Redirect map {0} does not specify redirect type.", item2.Paths.FullPath), this);
                            }
                        }
                    }
                    if (this.CacheExpiration > 0)
                    {
                        SetCache(AllMappingsPrefix, redirectMappings);
                    }
                }
                return redirectMappings;
            }
        }


        public RedirectMapResolver()
        {
        }



        protected virtual RedirectMapping FindMapping(string filePath)
        {
            RedirectMapping redirectMapping = null;
            List<RedirectMapping>.Enumerator enumerator = this.MappingsMap.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    RedirectMapping current = enumerator.Current;
                    if ((current.IsRegex || !(current.Pattern == filePath)) && (!current.IsRegex || !current.Regex.IsMatch(filePath)))
                    {
                        continue;
                    }
                    redirectMapping = current;
                    return redirectMapping;
                }
                return null;
            }
            catch (Exception)
            {

            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            return redirectMapping;
        }

        protected virtual RedirectMapping GetResolvedMapping(string filePath)
        {
            var item = GetCache<Dictionary<string, RedirectMapping>>(ResolvedMappingsPrefix);

            if (item == null || !item.ContainsKey(filePath))
            {
                return null;
            }
            return item[filePath];
        }
        protected virtual string GetTargetUrl(RedirectMapping mapping, string input)
        {
            string target = mapping.Target;
            if (mapping.IsRegex)
            {
                target = mapping.Regex.Replace(input.TrimEnd(new char[] { '/' }), target);
            }
            if (mapping.PreserveQueryString)
            {
                target = string.Concat(target.TrimEnd(new char[] { '/' }), HttpContext.Current.Request.Url.Query);
            }
            if (!string.IsNullOrEmpty(Context.Site.VirtualFolder))
            {
                target = string.Concat(StringUtil.EnsurePostfix('/', Context.Site.VirtualFolder), target.TrimStart(new char[] { '/' }));
            }
            return target;
        }

        protected virtual bool IsFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || WebUtil.IsExternalUrl(filePath))
            {
                return true;
            }
            return File.Exists(HttpContext.Current.Server.MapPath(filePath));
        }


        public override void ProcessRequest(HttpRequestArgs args)
        {
            //Item item = Context.Item;
            
            if (args == null || args.HttpContext == null)
                return;

            var context = args.HttpContext;
            if (Context.Site == null || this.IsFile(Context.Request.FilePath))
            {
                return;
            }

            string str = this.EnsureSlashes(Context.Request.FilePath.ToLower());



            RedirectMapping resolvedMapping = this.GetResolvedMapping(str);

            bool flag = resolvedMapping != null;
            if (resolvedMapping == null)
            {
                resolvedMapping = this.FindMapping(str);
            }


            if (resolvedMapping != null && !flag)
            {
                var item = GetCache<Dictionary<string, RedirectMapping>>(ResolvedMappingsPrefix)
                                     ?? new Dictionary<string, RedirectMapping>();

                item[str] = resolvedMapping;

                SetCache(ResolvedMappingsPrefix, item);
            }


            if (resolvedMapping != null && HttpContext.Current != null)
            {
                string targetUrl = this.GetTargetUrl(resolvedMapping, str);
                if (resolvedMapping.RedirectType == RedirectType.Redirect301)
                {
                    this.Redirect301(HttpContext.Current.Response, targetUrl);
                }
                if (resolvedMapping.RedirectType == RedirectType.Redirect302)
                {
                    HttpContext.Current.Response.Redirect(targetUrl, true);
                }
                if (resolvedMapping.RedirectType == RedirectType.ServerTransfer)
                {
                    HttpContext.Current.Server.TransferRequest(targetUrl);
                }
            }
        }

     }
}