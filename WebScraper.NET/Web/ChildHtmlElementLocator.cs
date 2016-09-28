using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WebScraper.Web
{
    public interface ChildHtmlElementLocator
    {
        string getName();
        HtmlElement locate(HtmlElement parent);
    }

    public abstract class AbstractChildHtmlElementLocator : ChildHtmlElementLocator
    {
        public string Name { get; set; }
        public AbstractChildHtmlElementLocator()
            : base()
        {

        }
        public AbstractChildHtmlElementLocator(string name = null)
        {
            this.Name = name;
        }
        public string getName()
        {
            return Name;
        }
        public abstract HtmlElement locate(HtmlElement parent);
    }

    public class SimpleChildHtmlElementLocator : AbstractChildHtmlElementLocator
    {
        public ElementMatcher<HtmlElement> Matcher { get; set; }

        public ChildHtmlElementLocator Filter { get; set; }
        public SimpleChildHtmlElementLocator()
            : base()
        {

        }
        public SimpleChildHtmlElementLocator(string name = null, ElementMatcher<HtmlElement> matcher = null, ChildHtmlElementLocator filter = null)
            : base()
        {
            this.Matcher = matcher;
            this.Filter = filter;
        }
        public override HtmlElement locate(HtmlElement parent)
        {
            HtmlElement ret = null;
            if (null != parent)
            {
                HtmlElement toMatch = null;
                foreach (HtmlElement child in parent.All)
                {
                    toMatch = child;
                    if (null != Filter)
                    {
                        toMatch = Filter.locate(child);
                    }
                    if (null != toMatch)
                    {
                        if (null == Matcher || Matcher.match(toMatch))
                        {
                            ret = toMatch;
                            break;
                        }
                    }

                }
            }
            return ret;
        }

    }

}
