using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;

namespace WebScraper.Web
{
    public interface WebValidator
    {
        bool validate(Agent agent);
    }
    public abstract class AbstractWebValidator : WebValidator
    {
        public bool validate(Agent agent)
        {
            bool ret = false;
            MethodInvoker delegateCall = delegate
            {
                ret = internalValidate(agent);
            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
            return ret;
        }
        public abstract bool internalValidate(Agent agent);

    }
    public class TitleWebValidator : AbstractWebValidator
    {

        public string Title { get; set; }
        public Regex TitleRegex { get; set; }
        public TitleWebValidator()
        {

        }
        public TitleWebValidator(string title = null, Regex titleRegex = null)
        {
            this.Title = title;
            this.TitleRegex = titleRegex;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            string title = agent.WebBrowser.Document.Title;
            if (null != Title)
            {
                ret = title.Equals(Title);
            }
            if (null != TitleRegex)
            {
                ret = TitleRegex.IsMatch(title);
            }
            return ret;
        }

    }
    public class ValueCheckValidator : AbstractWebValidator
    {
        public HtmlElementLocator Locator { get; set; }
        public string AttributeName { get; set; }
        public string Value { get; set; }
        public Regex ValueRegex { get; set; }
        public ValueCheckValidator(HtmlElementLocator locator = null, string attributeName = null, string value = null, Regex valueRegex = null)
        {
            this.Locator = locator;
            this.AttributeName = attributeName;
            this.Value = value;
            this.ValueRegex = valueRegex;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            if (null != Locator)
            {
                HtmlElement element = Locator.locate(agent);
                if (null != element)
                {
                    if (null == AttributeName)
                    {
                        if (null == ValueRegex)
                        {
                            ret = element.InnerText.Equals(Value);
                        }
                        else
                        {
                            ret = ValueRegex.IsMatch(element.InnerText);
                        }
                    }
                    else
                    {
                        String value = element.GetAttribute(AttributeName);
                        if (null != value)
                        {
                            if (null == ValueRegex)
                            {
                                ret = value.Equals(Value);
                            }
                            else
                            {
                                ret = ValueRegex.IsMatch(value);
                            }
                        }

                    }
                }
            }
            return ret;
        }
    }

    public class BackgroundStyleCheckValidator : AbstractWebValidator
    {
        public HtmlElementLocator Locator { get; set; }
        public string StyleValue { get; set; }
        public BackgroundStyleCheckValidator(HtmlElementLocator locator = null, string styleValue = null)
        {
            this.Locator = locator;
            this.StyleValue = styleValue;
        }
        public override bool internalValidate(Agent agent)
        {
            bool ret = false;
            MethodInvoker delegateCall = delegate
            {
                if (null != Locator)
                {
                    HtmlElement element = Locator.locate(agent);
                    if (null != element)
                    {
                        String value = element.Style;
                        if (null == StyleValue && null == value)
                        {
                            ret = true;
                        }
                        else
                        {
                            ret = value.Equals(StyleValue);
                        }
                    }
                }
            };
            if (agent.WebBrowser.InvokeRequired)
            {
                agent.WebBrowser.Invoke(delegateCall);
            }
            else
            {
                delegateCall();
            }
            return ret;
        }
    }


}
