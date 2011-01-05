using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WebScraper.Web
{
    public class AutoLoginAgent : Agent
    {
        public bool LoginRequired { get; set; }

        public List<WebAction> LoginActions { get; set; }

        public List<WebAction> LogoutActions { get; set; }

        public ExtractWebAction<Boolean> LoginCheckAction { get; set; }

        public AutoLoginAgent()
        {
            LoginRequired = true;
        }

        public void doLogin()
        {
            doActions(LoginActions);
        }
        public void doLogout()
        {
            doActions(LogoutActions);
        }
        public bool isLoggedIn()
        {
            bool ret = false;
            if (null == LoginCheckAction)
            {
                //if the login check is not present then it is a free resource
                ret = true;
            } else {
                LoginCheckAction.doAction(this);
                ret = LoginCheckAction.ExtractedData;
            }
            return ret;
        }
        public bool isLoggedOut()
        {
            return !isLoggedIn();
        }
    }
}
