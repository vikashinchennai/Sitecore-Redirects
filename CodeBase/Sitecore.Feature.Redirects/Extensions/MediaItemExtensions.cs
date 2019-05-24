using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Resources.Media;
using System.Linq;

namespace Sitecore.Feature.Redirects.Extensions
{
    public static class MediaItemExtensions
    {
        public static string GetMediaUrl(this MediaItem mediaItem, MediaUrlOptions options = null)
        {
            string mediaUrl = MediaManager.GetMediaUrl(mediaItem, options ?? new MediaUrlOptions());
            mediaUrl = (mediaUrl.Contains("://") ? mediaUrl : StringUtil.EnsurePrefix('/', mediaUrl));
            return mediaUrl;
        }
           
        public static bool InheritsFrom(this Item item, ID templateId)
        {
            return item.IsDerived(templateId);
        }

       
    }

    public static class TemplateExtensions
    {
        public static bool IsDerived([NotNull] this Template template, [NotNull] ID templateId)
        {
            return template.ID == templateId || template.GetBaseTemplates().Any(baseTemplate => IsDerived(baseTemplate, templateId));
        }
    }
    public static class ItemExtensions
    {
        public static bool IsDerived([NotNull] this Item item, [NotNull] ID templateId)
        {
            return TemplateManager.GetTemplate(item).IsDerived(templateId);
        }
    }
}
