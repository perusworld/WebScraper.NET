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
        public SimpleChildHtmlElementLocator()
            : base()
        {

        }
        public SimpleChildHtmlElementLocator(string name = null, ElementMatcher<HtmlElement> matcher = null)
            : base()
        {
            this.Matcher = matcher;
        }
        public override HtmlElement locate(HtmlElement parent)
        {
            HtmlElement ret = null;
            if (null != parent)
            {
                foreach (HtmlElement child in parent.All)
                {
                    if (Matcher.match(child))
                    {
                        ret = child;
                        break;
                    }
                }
            }
            return ret;
        }

    }

}
