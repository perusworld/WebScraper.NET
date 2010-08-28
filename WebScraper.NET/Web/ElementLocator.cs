using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;


namespace WebScraper.Web
{
    public interface ElementLocator<T>
    {
        string getName();
        T locate(Agent agent);
    }
    public interface ElementMatcher<T>
    {
        string getName();
        bool match(T element);
    }
    public interface DataExtractor<T, V>
    {
        V extract(T element);
    }
    public enum ElementTarget
    {
        SELF, CHILDREN, ALL_CHILDREN
    }
}
