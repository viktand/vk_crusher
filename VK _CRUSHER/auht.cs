using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK__CRUSHER
{
    public partial class auht : Form
    {
        public string ACCESS_TOKEN = "";
        public string USER_ID = "";

        public auht()
        {
            InitializeComponent();
        }


        public async void VKAuth(string appID) // авторизация в VK
        {
            string url = "https://oauth.vk.com/authorize?client_id=" + appID + "&scope=9999999" +
                                    "&redirect_uri=http://oauth.vk.com/blank.html&display=touch&response_type=token";
            webBrowser1.Navigate(url);

        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e) // проверка загруженной страницы, поиск ключей
        {
            if (webBrowser1.Url.ToString().IndexOf("https://oauth.vk.com/blank.html") == 0)
            {
                string s = webBrowser1.Url.ToString();
                int i1 = s.IndexOf("access_token")+13;
                int i3 = s.IndexOf("&", i1);
                int i2 = s.IndexOf("user_id");
                ACCESS_TOKEN = s.Substring(i1,i3-i1);
                USER_ID = s.Substring(i2+8);
                this.Close();
            }
        }
    }
}
