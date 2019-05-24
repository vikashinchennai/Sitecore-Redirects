using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;


namespace Sitecore.Foundation.Fields.ContentEditor.Controls
{
    /// <summary>
    /// Contains some method which are copied from Sitecore.Kernel.dll 8.1.0 rev. 160519.
    /// HACK: check this class implementation whenever do a Sitecore update.
    /// </summary>
    public class CustomNameValueList : NameValue
    {
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");

            if (Sitecore.Context.ClientPage.IsEvent)
            {
                this.LoadValue();
            }
            else
            {
                this.BuildControl();
            }
        }

        /// <summary>
        /// Copied without invalid character validation.
        /// </summary>
        protected new void ParameterChange()
        {
            var clientPage = Sitecore.Context.ClientPage;
            if (clientPage.ClientRequest.Source == StringUtil.GetString(clientPage.ServerProperties[this.ID + "_LastParameterID"]) && !string.IsNullOrEmpty(clientPage.ClientRequest.Form[clientPage.ClientRequest.Source]))
            {
                var emptyRow = this.BuildParameterKeyValue(string.Empty, string.Empty);
                clientPage.ClientResponse.Insert(this.ID, "beforeEnd", emptyRow);
            }

            clientPage.ClientResponse.SetReturnValue(true);
        }

        /// <summary>
        /// It calls the copied method with the name value collection from the request form.
        /// </summary>
        private void LoadValue()
        {
            var page = HttpContext.Current.Handler as Page;
            this.LoadValue(page == null ? new NameValueCollection() : page.Request.Form);
        }

        /// <summary>
        /// Copied without replacing the '-' with '_' and with URL encode.
        /// </summary>
        public void LoadValue(NameValueCollection formCollection)
        {
            if (this.ReadOnly || this.Disabled)
            {
                return;
            }

            var collection = formCollection ?? new NameValueCollection();
            var urlString = new UrlString();
            foreach (string key in collection.Keys)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith(this.ID + "_Param", StringComparison.InvariantCulture) && !key.EndsWith("_value", StringComparison.InvariantCulture))
                {
                    var resultKey = collection[key];
                    var resultValue = collection[key + "_value"];
                    if (!string.IsNullOrEmpty(resultKey))
                    {
                        urlString[HttpUtility.UrlEncode(resultKey)] = HttpUtility.UrlEncode(resultValue) ?? string.Empty;
                    }
                }
            }

            this.Value = urlString.ToString();
        }

        /// <summary>
        /// Copied with URL encode.
        /// </summary>
        public void BuildControl()
        {
            var urlString = new UrlString(this.Value);
            foreach (var key in urlString.Parameters.Keys.Cast<string>().Where(key => key.Length > 0))
            {
                this.Controls.Add(new LiteralControl(this.BuildParameterKeyValue(HttpUtility.UrlDecode(key), HttpUtility.UrlDecode(urlString.Parameters[key]))));
            }

            this.Controls.Add(new LiteralControl(this.BuildParameterKeyValue(string.Empty, string.Empty)));
        }

        /// <summary>
        /// Copied without any changes. 
        /// </summary>
        private string BuildParameterKeyValue(string key, string value)
        {
            Assert.ArgumentNotNull(key, "key");
            Assert.ArgumentNotNull(value, "value");
            var uniqueId = GetUniqueID(this.ID + "_Param");
            Sitecore.Context.ClientPage.ServerProperties[this.ID + "_LastParameterID"] = uniqueId;
            var clientEvent = Sitecore.Context.ClientPage.GetClientEvent(this.ID + ".ParameterChange");
            var isReadonly = this.ReadOnly ? " readonly=\"readonly\"" : string.Empty;
            var isDisabled = this.Disabled ? " disabled=\"disabled\"" : string.Empty;
            var isVertical = this.IsVertical ? "</tr><tr>" : string.Empty;
            return
                string.Format(
                    "<table width=\"100%\" class='scAdditionalParameters'><tr><td>{0}</td>{2}<td width=\"100%\">{1}</td></tr></table>",
                    string.Format(
                        "<input id=\"{0}\" name=\"{1}\" type=\"text\"{2}{3} style=\"{6}\" value=\"{4}\" onchange=\"{5}\"/>",
                        uniqueId, uniqueId, isReadonly, isDisabled, StringUtil.EscapeQuote(key), clientEvent, this.NameStyle),
                    this.GetValueHtmlControl(uniqueId, StringUtil.EscapeQuote(HttpUtility.UrlDecode(value))), isVertical);
        }
    }
}