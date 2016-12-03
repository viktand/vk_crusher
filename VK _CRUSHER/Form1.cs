using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK__CRUSHER
{
    public partial class Form1 : Form
    {
        private auht authForm;
        public static string appKey = "czQ5ThZqFuqujxTXEDUz";
        public static string appID = "5726642";
        string ACCESS_TOKEN = "";
        string USER_ID = "";

        public Form1()
        {
            InitializeComponent();
        }
        
       public void VKAuth() // авторизация в VK
        {
            authForm = new auht();
            authForm.VKAuth(appID);
            authForm.ShowDialog();
            ACCESS_TOKEN = authForm.ACCESS_TOKEN;
            USER_ID = authForm.USER_ID;
        }

        private void Form1_Shown(object sender, EventArgs e) // первое открытие формы программы
        {
            status.Text = "Загрузка страницы авторизации...";
            this.Refresh();
            VKAuth(); // авторизация
            status.Text = "Успешная авторизация";
            this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Friends fFrm;
            fFrm = new Friends();
            fFrm.setID(ACCESS_TOKEN);
            Hide();
            fFrm.ShowDialog();
            Show();
        }
    }
}
