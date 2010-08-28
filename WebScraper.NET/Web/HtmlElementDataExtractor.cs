using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebScraper.Web
{
    public interface HtmlElementDataExtractor<V> : DataExtractor<HtmlElement, V>
    {
    }
    public abstract class AbstractHtmlElementDataExtractor<V> : HtmlElementDataExtractor<V>
    {
        public AbstractHtmlElementDataExtractor()
        {

        }
        public abstract V extract(HtmlElement element);
    }

    public class StringHtmlElementDataExtractor : AbstractHtmlElementDataExtractor<string>
    {
        public string Part { get; set; }
        public StringHtmlElementDataExtractor()
            : base()
        {

        }
        public StringHtmlElementDataExtractor(string part = ":innertext")
        {
            Part = part;
        }
        public override string extract(HtmlElement element)
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
