using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WebScraper.Web
{
    public interface HtmlElementDataExtractor<V> : DataExtractor<HtmlElement, V>
    {
    }
    public abstract class AbstractHtmlElementDataExtractor<V> : HtmlElementDataExtractor<V>
    {
        public string Part { get; set; }
        public AbstractHtmlElementDataExtractor()
        {

        }
        public AbstractHtmlElementDataExtractor(string part = null)
        {
            this.Part = part;
        }
        public string getString(HtmlElement element)
        {
            string ret = null;
            if (null != element)
            {
                if (":outerhtml".Equals(Part))
                {
                    ret = element.OuterHtml;
                }
                else if (":outertext".Equals(Part))
                {
                    ret = element.OuterText;
                }
                else if (":innerhtml".Equals(Part))
                {
                    ret = element.InnerHtml;
                }
                else if (":innertext".Equals(Part))
                {
                    ret = element.InnerText;
                }
                else
                {
                    ret = element.GetAttribute(Part);
                }
            }
            return ret;
        }
        public abstract V extract(HtmlElement element);
    }
    public abstract class AbstractUrlHtmlElementDataExtractor<V> : HtmlElementDataExtractor<V>
    {
        public AbstractUrlHtmlElementDataExtractor()
        {

        }
        public Uri getUrl(HtmlElement element)
        {
            Uri ret = null;
            if (null != element)
            {
                ret = element.Document.Window.Url;
            }
            return ret;
        }
        public abstract V extract(HtmlElement element);
    }
    public abstract class AbstractCookieHtmlElementDataExtractor<V> : HtmlElementDataExtractor<V>
    {
        public AbstractCookieHtmlElementDataExtractor()
        {

        }
        public String getCookie(HtmlElement element)
        {
            String ret = null;
            if (null != element)
            {
                ret = element.Document.Cookie;
            }
            return ret;
        }
        public abstract V extract(HtmlElement element);
    }

    public class StringHtmlElementDataExtractor : AbstractHtmlElementDataExtractor<string>
    {
        public StringHtmlElementDataExtractor()
            : base()
        {

        }
        public StringHtmlElementDataExtractor(string part = ":innertext")
            : base(part: part)
        {
        }
        public override string extract(HtmlElement element)
        {
            return getString(element);
        }

    }

    public class BooleanHtmlElementDataExtractor : AbstractHtmlElementDataExtractor<Boolean>
    {
        public BooleanHtmlElementDataExtractor()
            : base()
        {

        }
        public BooleanHtmlElementDataExtractor(string part = ":innertext")
            : base(part: part)
        {
        }
        public override Boolean extract(HtmlElement element)
        {
            Boolean ret = false;
            String text = getString(element);
            ret = (null != text && !text.Trim().ToLower().Equals("false"));
            return ret;
        }

    }

    public class BooleanUrlHtmlElementDataExtractor : AbstractUrlHtmlElementDataExtractor<Boolean>
    {
        public Regex Matcher { get; set; }
        public Boolean ShouldMatch { get; set; }
        public BooleanUrlHtmlElementDataExtractor()
            : base()
        {
            ShouldMatch = true;
        }
        public BooleanUrlHtmlElementDataExtractor(Regex matcher = null, Boolean shouldMatch = true)
            : base()
        {
            this.Matcher = matcher;
            this.ShouldMatch = shouldMatch;
        }
        public override Boolean extract(HtmlElement element)
        {
            Boolean ret = false;
            Uri url = getUrl(element);
            if (null != Matcher)
            {
                ret = Matcher.IsMatch(url.ToString());
                if (!ShouldMatch)
                {
                    ret = !ret;
                }
            }
            return ret;
        }

    }

    public class BooleanCookieHtmlElementDataExtractor : AbstractCookieHtmlElementDataExtractor<Boolean>
    {
        public Regex Matcher { get; set; }
        public Boolean ShouldMatch { get; set; }
        public BooleanCookieHtmlElementDataExtractor()
            : base()
        {
            ShouldMatch = true;
        }
        public BooleanCookieHtmlElementDataExtractor(Regex matcher = null, Boolean shouldMatch = true)
            : base()
        {
            this.Matcher = matcher;
            this.ShouldMatch = shouldMatch;
        }
        public override Boolean extract(HtmlElement element)
        {
            Boolean ret = false;
            String cookie = getCookie(element);
            if (null != Matcher)
            {
                ret = Matcher.IsMatch(cookie);
                if (!ShouldMatch)
                {
                    ret = !ret;
                }
            }
            return ret;
        }

    }

    public class ListHtmlElementDataExtractor<V> : AbstractHtmlElementDataExtractor<List<V>>
    {
        public ElementMatcher<HtmlElement> Matcher { get; set; }
        public HtmlElementDataExtractor<V> Extractor { get; set; }
        public ElementTarget Target { get; set; }
        public ListHtmlElementDataExtractor()
            : base()
        {

        }
        public ListHtmlElementDataExtractor(ElementMatcher<HtmlElement> matcher = null, ElementTarget target = ElementTarget.SELF, HtmlElementDataExtractor<V> extractor = null)
        {
            Matcher = matcher;
            Target = target;
            Extractor = extractor;
        }
        public override List<V> extract(HtmlElement element)
        {
            List<V> ret = new List<V>();
            if (Target.Equals(ElementTarget.SELF))
            {
                if (null == Matcher || Matcher.match(element))
                {
                    ret.Add(Extractor.extract(element));
                }
            }
            else
            {
                foreach (HtmlElement childNode in Target.Equals(ElementTarget.ALL_CHILDREN) ? element.All : element.Children)
                {
                    if (null != Matcher && !Matcher.match(childNode))
                    {
                        continue;
                    }
                    ret.Add(Extractor.extract(childNode));
                }
            }
            return ret;
        }

    }

}
