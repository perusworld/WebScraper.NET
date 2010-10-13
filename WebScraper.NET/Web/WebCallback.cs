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
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }

        public BackgroundInvoke(string elementId = null, string methodName = "click", string attributeName = null, string attributeValue = null)
        {
            this.ElementId = elementId;
            this.MethodName = methodName;
            this.AttributeName = attributeName;
            this.AttributeValue = attributeValue;
        }
        public void callback(Agent agent)
        {
            MethodInvoker delegateCall = delegate
            {
                HtmlElement element = agent.WebBrowser.Document.GetElementById(ElementId);
                if (null != element)
                {
                    if (null != AttributeName)
                    {
                        element.SetAttribute(AttributeName, AttributeValue);
                    }
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
    public class BlockingCallback : WebCallback
    {
        public void callback(Agent agent)
        {
            Thread.Sleep(10000);
        }
    }
    public class SendKeysCallback : WebCallback
    {
        public String SendKey { get; set; }
        public SendKeysCallback(string sendKey = null)
        {
            SendKey = sendKey;
        }

        public void callback(Agent agent)
        {
            agent.WebBrowser.Focus();
            SendKeys.SendWait(SendKey);
        }
    }
}
