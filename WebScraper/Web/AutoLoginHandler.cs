using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebScraper.Web.AutoLogin
{
    public interface AutoLoginHandler
    {
        string getName();
        void doLogin(System.Windows.Forms.WebBrowser browser);
        void doLogout(System.Windows.Forms.WebBrowser browser);
        bool isLoggedIn(System.Windows.Forms.WebBrowser browser);
        bool isLoggedOut(System.Windows.Forms.WebBrowser browser);
        bool isLoginRequired();
    }
}
