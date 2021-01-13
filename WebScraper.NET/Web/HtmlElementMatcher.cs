﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WebScraper.Web
{
    public abstract class AbstractHtmlElementMatcher : ElementMatcher<HtmlElement>
    {
        public string Name { get; set; }
        public AbstractHtmlElementMatcher()
        {

        }
        public AbstractHtmlElementMatcher(string name = null)
        {
            this.Name = name;
        }
        public string getName()
        {
            return Name;
        }
        public abstract bool match(HtmlElement element);
    }

    public class TagHtmlElementMatcher : AbstractHtmlElementMatcher
    {
        public string Tag { get; set; }
        public TagHtmlElementMatcher()
            : base()
        {

        }
        public TagHtmlElementMatcher(string name = null, string tag = null)
            : base(name)
        {
            Tag = tag;
        }

        public override bool match(HtmlElement element)
        {
            return (null != Tag && null != element && Tag.Equals(element.TagName, StringComparison.CurrentCultureIgnoreCase));
        }
    }


    public class AttributeHtmlElementMatcher : AbstractHtmlElementMatcher
    {
        public Regex TagValueRegex { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public Dictionary<string, Regex> AttributeRegexs { get; set; }

        public AttributeHtmlElementMatcher()
            : base()
        {

        }
        public AttributeHtmlElementMatcher(string name = null, string attribute = null, string value = null)
            : base(name)
        {
            Attributes = new Dictionary<string, string>();
            Attributes[attribute] = value;
        }
        public AttributeHtmlElementMatcher(string name = null, Regex tagValueRegex = null, Dictionary<string, string> attributes = null, Dictionary<string, Regex> attributeRegexs = null)
            : base(name)
        {
            this.TagValueRegex = tagValueRegex;
            this.Attributes = attributes;
            this.AttributeRegexs = attributeRegexs;
        }

        public override bool match(HtmlElement element)
        {
            bool ret = false;
            bool foundAtr = true;
            if (null != Attributes)
            {
                foreach (string key in Attributes.Keys)
                {
                    string value = element.GetAttribute(key);
                    if (null == value || !value.Equals(Attributes[key]))
                    {
                        foundAtr = false;
                        break;
                    }
                }
            }
            bool foundAtrReg = true;
            if (null != AttributeRegexs)
            {
                foreach (string key in AttributeRegexs.Keys)
                {
                    string value = element.GetAttribute(key);
                    Regex regex = AttributeRegexs[key];
                    if (null == value || !regex.IsMatch(value))
                    {
                        foundAtrReg = false;
                        break;
                    }
                }
            }
            bool foundValue = true;
            if (null != TagValueRegex)
            {
                foundValue = null != element.InnerText && TagValueRegex.IsMatch(element.InnerText);
            }
            if (foundAtr && foundValue && foundAtrReg)
            {
                ret = true;
            }
            return ret;
        }
    }

    public class IdHtmlElementMatcher : AttributeHtmlElementMatcher
    {
        public IdHtmlElementMatcher()
            : base()
        {

        }
        public IdHtmlElementMatcher(string name = null, string value = null)
            : base(name, "id", value)
        {
        }

    }

}
