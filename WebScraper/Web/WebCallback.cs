using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;


namespace WebScraper.Web
{
    public interface WebCallback
    {
        void callback(Agent agent);
    }
    public class BackgroundInvoke : WebCallback
    {
        public string ElementId { get; set; }
        public string MethodName { get; set; }

        public BackgroundInvoke(string elementId = null, string methodName = "click")
        {
            this.ElementId = elementId;
            this.MethodName = methodName;
        }
        public void callback(Agent agent)
        {
            MethodInvoker delegateCall = delegate
            {
                HtmlElement element = agent.WebBrowser.Document.GetElementById(ElementId);
                if (null != element)
                {
                    element.InvokeMember(MethodName);
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
        }
    }

}
