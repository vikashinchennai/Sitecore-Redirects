using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Feature.Redirects.Repositories;
using System;
using Sitecore.Data;
using System.Linq;
namespace Sitecore.Feature.Redirects.EventHandlers
{
    public class RedirectMapCacheClearer
    {
        public RedirectMapCacheClearer()
        {
        }

        public void ClearCache(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Log.Info("RedirectMapCacheClearer clearing redirect map cache.", this);
            RedirectsRepository.Reset();
            Log.Info("RedirectMapCacheClearer done.", this);
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Item item = Event.ExtractParameter(args, 0) as Item;

            if (item == null)
                return;

            if (IsCustomRedirectItems(item.TemplateID))
                this.ClearCache(sender, args);
        }

        public void OnItemSavedRemote(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            ItemSavedRemoteEventArgs itemSavedRemoteEventArg = args as ItemSavedRemoteEventArgs;
            if (itemSavedRemoteEventArg == null || itemSavedRemoteEventArg.Item == null)
                return;

            if (IsCustomRedirectItems(itemSavedRemoteEventArg.Item.TemplateID))
                this.ClearCache(sender, args);
        }

        private bool IsCustomRedirectItems(ID templateId)
        {
            var customTempates = new ID[] { Templates.RedirectMap.ID, Templates.SiteLevelMap.ID, Templates.Redirect.ID };
            return customTempates.Any(a => a.Equals(templateId));
        }
    }
}