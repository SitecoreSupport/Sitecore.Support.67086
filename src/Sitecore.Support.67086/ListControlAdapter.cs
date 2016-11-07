using Sitecore.Form.Core.Client.Submit;
using Sitecore.Form.Core.Configuration;
using Sitecore.Form.Core.Utility;
using Sitecore.Forms.Core.Data;
using Sitecore.Globalization;
using Sitecore.WFFM.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace Sitecore.Support.Form.UI.Adapters
{
    public class ListControlAdapter : Adapter
    {
        public override IEnumerable<string> AdaptToFriendlyListValues(IFieldItem field, string value, bool returnTexts)
        {
            IEnumerable<string> enumerable = ParametersUtil.XmlToStringArray(value);
            if (field != null & returnTexts)
            {
                Match match = Regex.Match(field.LocalizedParameters, "<items>([^<]*)</items>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string text = HttpUtility.UrlDecode(match.Result("$1"));
                    if (text.StartsWith(StaticSettings.SourceMarker))
                    {
                        text = new QuerySettings("root", text.Substring(StaticSettings.SourceMarker.Length)).ToString();
                    }
                    Language language;
                    //added null check for Context.Request. required when code is executed in a background thread (remote event)
                    if (Context.Request != null && !string.IsNullOrEmpty(Context.Request.QueryString["la"]))
                    {
                        language = Language.Parse(Context.Request.QueryString["la"]);
                    }
                    else if (Context.ContentLanguage != null && Context.ContentLanguage.ToString() != string.Empty)
                    {
                        language = Context.ContentLanguage;
                    }
                    else
                    {
                        language = Context.Language;
                    }
                    NameValueCollection nameValueCollection;
                    using (new LanguageSwitcher(language))
                    {
                        nameValueCollection = QueryManager.Select(QuerySettings.ParseRange(text));
                    }
                    List<string> list = new List<string>();
                    foreach (string current in enumerable)
                    {
                        if (!string.IsNullOrEmpty(nameValueCollection[current]))
                        {
                            list.Add(nameValueCollection[current]);
                        }
                    }
                    return list;
                }
            }
            return enumerable;
        }

        public override string AdaptToFriendlyValue(IFieldItem field, string value)
        {
            return string.Join(", ", new List<string>(this.AdaptToFriendlyListValues(field, value, true)).ToArray());
        }
    }
}
