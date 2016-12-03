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
    public partial class searchGroups : Form
    {
        private string _id;
        string[] grp;
        private bool ok = false;

        public delegate void searchResult(string st);  // делегат метода обратного вызова от кнопки 
        public event searchResult onGroupFnd;

        public searchGroups()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // найти группу
            searchGr();
        }

        public void setID(string id)
        {
            _id = id;
        }

        private void searchGr()
        {
            string url = "https://api.vk.com/method/groups.search";
            string str = "access_token=" + _id;
            str += "&q=" + textBox1.Text.Trim();
            str += "&count=1000&v=5.60";
            string ans = POST(url, str);
            ok = false;
            getInfomGroups(ans);
        }

        private void getInfomGroups(string ans) // "распаковать" ответ
        {
            listBox1.Items.Clear();
            if (ans.IndexOf("error") > -1)
            {
                listBox1.Items.Add("Ответ VK: Ошибка в запросе");
                return;
            }
            ans = ans.Replace("\"", "");
            ans = ans.Replace("\\", "");
            if (ans.IndexOf("count:0,") > -1)
            {
                listBox1.Items.Add("Ничего не найдено");
                return;
            }
            int c = 0;
            int i = ans.IndexOf("id:");
            while (i>-1)
            {
                Array.Resize<string>(ref grp, c + 1);
                int i2 = ans.IndexOf(",", i);
                grp[c++] = ans.Substring(i + 3, i2 - i - 3);
                int i3 = ans.IndexOf("name:", i2);
                int i4 = ans.IndexOf(",", i3);
                listBox1.Items.Add(ans.Substring(i3 + 5, i4 - i3 - 5));
                i = ans.IndexOf("id:", i4);
            }
            if (c > 0) ok = true;
        }

        private static string POST(string Url, string Data) // послать пост запрос и вернуть ответ
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded1";
            byte[] sentData = Encoding.GetEncoding("utf-8").GetBytes(Data);
            req.ContentLength = sentData.Length;
            System.IO.Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            sendStream.Close();
            System.Net.WebResponse res = req.GetResponse();
            System.IO.Stream ReceiveStream = res.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(ReceiveStream, Encoding.UTF8);
            //Кодировка указывается в зависимости от кодировки ответа сервера
            Char[] read = new Char[256];
            int count = sr.Read(read, 0, 256);
            string Out = String.Empty;
            while (count > 0)
            {
                String str = new String(read, 0, count);
                Out += str;
                count = sr.Read(read, 0, 256);
            }
            return Out;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                searchGr();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            toMainForm();
        }

        private void toMainForm() // вернуть значение 
        {
            string f = "";
            if (ok)
            {
                f = grp[listBox1.SelectedIndex];
            }
            onGroupFnd(f);
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            toMainForm();
        }
    }
}
