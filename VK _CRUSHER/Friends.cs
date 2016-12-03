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
    public partial class Friends : Form
    {
        vkData[] countries;
        vkData[] cities;
        usrs[] fndUsers;
        string prg_ID;
        string memSID;
        int    memIndex;
        bool autoAdd = false;
        private bool flLoad;
        private string searchLine;
        private int userFlag=0;
        private bool captchaFlag = false; // запрос был с капчей
        string idCap;   // id последней капчи
        string mm = "";
        string dd = "";

        public Friends()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            loadCitiesDef();
            textBox3.Text = Properties.Settings.Default.idCap; // "9cd31babf7eb64d2f501248b6bef71db";
            numUD.Value = Properties.Settings.Default.tm;
        }

        void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (checkBox7.Checked) return;
            int dlt= e.Delta / -5;
            if (fndUsers != null && (vScrollBar1.Value + dlt >= 0) && (vScrollBar1.Value + dlt <= vScrollBar1.Maximum))
            {
                for (int i = 0; i < fndUsers.Length; i++)
                {
                    fndUsers[i].pict.Top -= dlt;
                }
                vScrollBar1.Value += dlt;
            }
        }

        private void loadCitiesDef() // загрузка начальных стран
        {
            string ans = "{id: 1, title Россия} id: 2, title Украина} id: 0, title Весь список}";
            getInfom(ref countries, ans, "id", "title");
            loadCombo(ref comboBox1, ref countries);
        }

        private void fndBtnPress(int index)
        {  // обработка нажатия на кнопку дружить/не дружить
            if (index < 0)
            {
                userFlag = index; // кнопка отклонить предложение дружбы
                return;
            }
            status.Text = "Обработка контакта: " + fndUsers[index].first_name + " " + fndUsers[index].last_name;
            memIndex = index;
            if (((fndUsers[index].friend_status == "0") || (fndUsers[index].friend_status == "2"))  && userFlag == 0)  
            {
                int i = addToFriend(fndUsers[index].id);
                if (i == 1)
                {
                    if(!checkBox7.Checked) fndUsers[index].pict.setFriendStatus("1");
                    fndUsers[index].friend_status = "1";
                    status.Text = "Успешная заявка на дружбу с: " + fndUsers[index].first_name + " " + fndUsers[index].last_name;
                }
                if (i == 2)
                {
                    if (!checkBox7.Checked) fndUsers[index].pict.setFriendStatus("3");
                    fndUsers[index].friend_status = "3";
                    status.Text = "Новый друг: " + fndUsers[index].first_name + " " + fndUsers[index].last_name;
                }
                if (i == 4)
                {
                    status.Text = "VK запросил решение капчи";
                    if (autoAdd)
                    {
                        // решение капчи в автоматическом режиме
                        doRucaptcha();
                    }
                    
                    return;
                }
                    return;
            } 
            if ((((fndUsers[index].friend_status == "2")  && userFlag == -1) || 
                (fndUsers[index].friend_status == "1") ||(fndUsers[index].friend_status == "3")) && !autoAdd)
            {   
                int i = offFriends(fndUsers[index].id);
                if (!checkBox7.Checked) fndUsers[index].pict.setFriendStatus("0");
                fndUsers[index].friend_status = "0";
                return;
            }
        }

        private void doRucaptcha() // решение капчи в автоматическом режиме
        {
            recap rcp = new recap();
            while (! flLoad)
            {
                System.Threading.Thread.Sleep(250);
                Application.DoEvents();
            }
            rcp.sendReq(textBox3.Text.Trim(), pictureBox1.Image);
            rcp.onGoodReq += sendCaptcha;
            rcp.ShowDialog();
            rcp.Dispose();
        }

        private void sendCaptcha(string s, string id)
        {
            textBox2.Text = s;
            idCap = id;
            sendCaptcha();
        }

        public void setID(string s)
        {
            prg_ID = s;
        }

        private bool loadCountries() // Загрузка стран
        {
            string url = "https://api.vk.com/method/database.getCountries";
            string pst = "act=a_run_method&al=1&count=500&need_all=1&v=5.60";
            string ans = POST(url, pst);
            getInfom(ref countries, ans, "id", "title");
            loadCombo(ref comboBox1, ref countries);
            return comboBox1.Items.Count > 0;
        }

        private void loadCombo(ref ComboBox cmb, ref vkData[] arr)
        {
            cmb.Items.Clear();
            for (int i=0; i<arr.Length; i++)
            {
                cmb.Items.Add(arr[i].name);
            }
        }

        private void getInfom(ref vkData[] arr, string lin, string v1, string v2) // разобрать строку и сохранить параметры в указанном массиве
        {
            lin = lin.Replace("\"", "");
            int i = lin.IndexOf(v1);
            int c = 0;
            while (i>0)
            {
                int i2 = lin.IndexOf(",", i);
                int j = lin.IndexOf(v2, i);
                int j2 = lin.IndexOf("}", j);
                Array.Resize<vkData>(ref arr, c+1);
                arr[c] = new vkData();
                arr[c].id = lin.Substring(i + 3, i2 - i - 3);
                arr[c].name = lin.Substring(j + 6, j2 - j - 6);
                c++;
                i = lin.IndexOf(v1, j2);
            }
        }

        private static string POST(string Url, string Data) // послать пост запрос и вернуть ответ
        {
            if (Data == null) return "";
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = "application/x-www-form-urlencoded";
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

        class vkData
        {
            public string id = "";
            public string name = "";
        }

        private void Friends_Shown(object sender, EventArgs e)
        {       
        }

        private bool loadCities()
        {
            string url = "https://api.vk.com/method/database.getCities";
            string cntr = countries[comboBox1.SelectedIndex].id.Trim();
            string al = "0";
            if (checkBox9.Checked) al = "1";
            string pst = "act=a_run_method&al=1&count=1000&country_id=" + cntr + "&need_all="+al+"&v=5.60";
            if (textBox5.Text != "") pst += "&q=" + textBox5.Text;
            string ans = POST(url, pst);
            status.Text = "Зарузка списка городов";
            getInfom(ref cities, ans, "id", "title");
            loadCombo(ref comboBox2, ref cities);
            return comboBox2.Items.Count > 0;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = true;
            if (countries[comboBox1.SelectedIndex].id.Trim() == "0")
            {
                status.Text = "Загрузка списка стран";
                Refresh();
                if (loadCountries()) { status.Text = "Список стран загружен успешно"; }
                else { status.Text = "Ошибка загрузки списка стран";}
                return;
            }
            if (loadCities()) { status.Text = "Список городов загружен успешно"; }
            else { status.Text = "Ошибка загрузки сиска городов"; }
        }

        private void button1_Click(object sender, EventArgs e)
        {   // поиск
            clearUsers();
            button6.Visible = true;
            decimal tm = numUD.Value * 4;
            mm = "";
            dd = "";
            autoAdd = true;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            if (checkBox8.Checked)
            {
                progressBar1.Maximum = 12 * 31 + 1;
                for (int m=1; m<13; m++)
                {
                    for (int d=1; d<32; d++)
                    {
                        if (fndUsers != null) status.Text = "Надено: " + fndUsers.Length.ToString();
                        mm = m.ToString();
                        dd = d.ToString();
                        searchUsers();
                        for (int j = 0; j < tm; j++) // pause
                        {
                            System.Threading.Thread.Sleep(250);
                            Application.DoEvents();
                            if (!autoAdd){break;}
                        }
                        progressBar1.Value++;
                        if (!autoAdd) { break; }
                    }
                }
            }
            else
            {
                searchUsers();
            }
            progressBar1.Visible = false;
            int i = 0;
            if (fndUsers != null) i = fndUsers.Length;
            status.Text = "Поиск завершен. Всего найдено: " + i.ToString() + " пользователей VK";
            button6.Visible = false;       
        }

        private void searchUsers()
        {
            string url = "https://api.vk.com/method/users.search";
            string t;
            string str = "access_token=" + prg_ID;
            str += "&country=" + GetCode(ref countries, comboBox1.Text);
            if (comboBox2.Enabled)
            {
                str += "&city=" + GetCode(ref cities, comboBox2.Text);
            } else
            {
                str += "&city=" + textBox4.Text;
            }
            str += "&age_from=" + numericUpDown1.Value.ToString();
            str += "&age_to=" + numericUpDown2.Value.ToString();
            if (checkBox1.Checked) t = "1"; else t = "0";
            str += "&online=" + t;
            if (checkBox2.Checked) t = "1"; else t = "0";
            str += "&has_photo=" + t;
            if (checkBox5.Checked) t = "1"; else t = "0";
            str += "&sort=" + t;
            t = "0";
            if (checkBox4.Checked) t = "1";
            if (checkBox3.Checked) if (t == "1") t = "0"; else t = "2";
            if (textBox1.Text.Trim() != "") str += "&group_id=" + textBox1.Text.Trim();
            str += "&sex=" + t;
            str += "&status=" + comboBox3.SelectedIndex.ToString();
            str += "&offset=" + numericUpDown4.Value.ToString();
            if (mm != "") str += "&birth_month=" + mm;
            if (dd != "") str += "&birth_day="+dd;
            str += "&fields=friend_status,photo_50&count=" + numericUpDown3.Value.ToString() + "&v=5.60";
            searchLine = str;
            string ans = POST(url, str);
            if (ans.IndexOf("captcha_sid") > -1) // ошибка - просит капчу
            {
                captchaFlag = true; ;
                ans = ans.Replace("\"", "");
                ans = ans.Replace("\\", "");
                int i = ans.IndexOf("captcha_img");
                if (i > 0)
                {
                    i = ans.IndexOf("https");
                    int i2 = ans.IndexOf("}}", i);
                    url = ans.Substring(i, i2 - i);
                    ans = ans.Replace("value", "");
                    ans = ans.Replace(":", "");
                    ans = ans.Replace("}", "");
                    ans = ans.Replace("keycap", "");
                    pictureBox1.Image = null;
                    flLoad = false;
                    pictureBox1.ImageLocation = url;
                    button4.Enabled = true;
                    i = ans.IndexOf("captcha_sid") + 11;
                    i2 = ans.IndexOf(",", i);
                    memSID = ans.Substring(i, i2 - i);
                }
            }
            else
            { 
                getInfomUsers(ref fndUsers, ans);
            }
        }

        private string GetCode(ref vkData[] arr, string text)
        {   // вернуть id по тексту
            if (arr == null)
            {
                return "0";
            }
            for (int i=0; i < arr.Length; i++)
            {
                if (arr[i].name == text) return arr[i].id;
            }
            return "0";
        }

        class usrs
        {
            public string id;
            public string first_name;
            public string last_name;
            public string photo_50;
            public string friend_status;
            public userShow pict;
        }

        private void getInfomUsers(ref usrs[] arr, string lin) // разобрать строку и сохранить параметры в указанном массиве
        {
            lin = lin.Replace("\"", "");
            lin = lin.Replace("\\", "");
            int i = lin.IndexOf("id:");
            int c = listBox1.Items.Count;
            while (i > 0)
            {
                Array.Resize<usrs>(ref arr, c + 1);
                arr[c] = new usrs();
                int i2 = lin.IndexOf(",", i);
                int j = lin.IndexOf("first_name", i);
                int j2 = lin.IndexOf(",", j);
                int j3 = lin.IndexOf("last_name", j2);
                int j4 = lin.IndexOf(",", j3);
                int j5 = lin.IndexOf("photo_50", j4);
                int j6 = lin.IndexOf(',', j5);
                if (j6 == -1) j6 = lin.IndexOf('}', j5);
                if (lin.Substring(j6 - 1, 1) == "}") j6--;
                int b = lin.IndexOf("deactivated:banned", j4);
                if ((b > 0) && (b < j5))
                {
                    arr[c].friend_status = "b";
                }
                else
                {
                    int j7 = lin.IndexOf("friend_status", j6);
                    int j8 = lin.IndexOf('}', j7);
                    arr[c].friend_status = lin.Substring(j7 + 14, j8 - j7 - 14);
                }
                arr[c].id = lin.Substring(i + 3, i2 - i - 3);
                arr[c].first_name = lin.Substring(j + 11, j2 - j - 11);
                arr[c].last_name = lin.Substring(j3 + 10, j4 - j3 - 10);
                arr[c].photo_50 = lin.Substring(j5 + 9, j6 - j5 - 9);
                if ((arr[c].friend_status == "1" || arr[c].friend_status == "3") && checkBox6.Checked)
                {
                    c--;
                }
                else
                { 
                    addUserToScreen(c);
                }
                c++;
                i = lin.IndexOf("id:", j4);
            }
            vScrollBar1.Maximum = c * 85 - panel1.Height + 10;
            if (vScrollBar1.Maximum < 0) vScrollBar1.Maximum = panel1.Height + 10;
            vScrollBar1.Value = 0;
            button9.Visible = true;
            button5.Visible = true;

        }

        private void clearUsers() // очистить список юзеров
        {
            if (fndUsers != null)
            {
                for (int i = 0; i < fndUsers.Length; i++)
                {
                    if (fndUsers[i].pict != null)
                    {
                        fndUsers[i].pict.Dispose();
                        panel1.Controls.Remove(fndUsers[i].pict);
                    }
                }
            }
            listBox1.Items.Clear();
            listBox1.Visible = checkBox7.Checked;
        }

        private void addUserToScreen(int index) // создать экземпляр юзера и показать на экране
        {
            if (checkBox7.Checked)
            {
                userToList(index);
                return;

            }
            fndUsers[index].pict = new userShow();
            fndUsers[index].pict.seID(fndUsers[index].id);
            fndUsers[index].pict.setName(fndUsers[index].first_name + " " +fndUsers[index].last_name);
            fndUsers[index].pict.Parent = panel1;
            fndUsers[index].pict.Left = 5;
            fndUsers[index].pict.Top = index * 85;
            fndUsers[index].pict.setPicture(fndUsers[index].photo_50);
            fndUsers[index].pict.buttonText("Добавить в друзья");
            fndUsers[index].pict.setFriendStatus(fndUsers[index].friend_status);
            fndUsers[index].pict.setIndex(index);
            fndUsers[index].pict.onFndPress += fndBtnPress;
        }

        private void userToList(int index)
        {
            string s = fndUsers[index].first_name;
            s += " " + fndUsers[index].last_name;
            s += " статус:" + fndUsers[index].friend_status;
            listBox1.Items.Add(s); 
        }

        private void panel1_Resize(object sender, EventArgs e)
        {
            vScrollBar1.Height = panel1.Height;
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        { // скрллинг юзеров
            if (fndUsers != null && !checkBox7.Checked)
            {
                for (int i = 0; i < fndUsers.Length; i++)
                {
                    fndUsers[i].pict.Top -= (e.NewValue-e.OldValue);
                }
            }
        }

        private void Friends_Resize(object sender, EventArgs e)
        {
            panel1.Height += Height - panel1.Height - 232;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearUsers();
            InputForm ifrm = new InputForm();
            ifrm.onButtonPress += getLink;
            ifrm.ShowDialog();
            ifrm.Dispose();
        }

        private void getLink(string str) // разбр строки из браузера и запрос VK
        {
            int i = str.IndexOf("?c") + 1;
            str = str.Substring(i);
            str = str.Replace("c[", "");
            str = str.Replace("]", "");
            str = "access_token=" + prg_ID + "&" + str;
            str += "&fields=friend_status,photo_50&count=" + numericUpDown3.Value.ToString() + "&v=5.60";
            setSearch(str); // установить параметры поиска из строки
            string url = "https://api.vk.com/method/users.search";
            searchLine = str;
            string ans = POST(url, str);
            getInfomUsers(ref fndUsers, ans);
        }

        private void setSearch(string str) // установить параметры поиска из строки
        {
            int i, j;
            i = str.IndexOf("&country=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                setCountyInCombo(str.Substring(i + 9, j - i - 9));
            }
            i = str.IndexOf("&city=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                textBox4.Text = str.Substring(i + 6, j - i - 6);
            }
            i = str.IndexOf("&age_from=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                numericUpDown1.Value = Int32.Parse(str.Substring(i + 10, j - i - 10));
            }
            i = str.IndexOf("&age_to=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                numericUpDown2.Value = Int32.Parse(str.Substring(i + 8, j - i - 8));
            }
            i = str.IndexOf("&online=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                checkBox1.Checked = (Int32.Parse(str.Substring(i + 8, j - i - 8)) == 1);
            }
            i = str.IndexOf("&has_photo=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                checkBox2.Checked = (Int32.Parse(str.Substring(i + 11, j - i - 11)) == 1);
            }
            i = str.IndexOf("&sort=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                checkBox5.Checked = (Int32.Parse(str.Substring(i + 6, j - i - 6)) == 1);
            }
            i = str.IndexOf("&sex=");
            if (i > 0)
            {
                j = str.IndexOf("&", i + 1);
                switch (str.Substring(i + 6, j - i - 6))
                {
                    case "0":
                        checkBox4.Checked = true;
                        checkBox3.Checked = false;
                        break;
                    case "1":
                        checkBox3.Checked = true;
                        checkBox4.Checked = false;
                        break;
                    case "2":
                        checkBox4.Checked = false;
                        checkBox3.Checked = false;
                        break;
                    default:
                        checkBox4.Checked = false;
                        checkBox3.Checked = false;
                        break;
                }
            }
            i = str.IndexOf("&group_id=");
            if (i > 0)
            {
                j = str.IndexOf("&", i+1);
                textBox1.Text = str.Substring(i + 10, j - i - 10);
            }
            i = str.IndexOf("&status=");
            if (i > 0)
            {
                j = str.IndexOf("&", i+1);
                comboBox3.SelectedIndex = Int32.Parse(str.Substring(i + 8, j - i - 8));
            }
        }

        private void setCountyInCombo(string v) // установить страну
        {
            for (int i=0; i<countries.Length; i++)
            {
                if (countries[i].id == v)
                {
                    comboBox1.SelectedIndex= i;
                    return;
                }
            }
        }

        private int addToFriend(string id) // заявкa на "в друзья" указанному пользователю
        {
            string url = "https://api.vk.com/method/friends.add";
            string str = "access_token=" + prg_ID;
            str += "&user_id=" + id.ToString().Trim();
            str += "&follow=0";
            string ans = POST(url, str);
            if (ans.IndexOf("captcha_sid") > -1) // ошибка - просит капчу
            {
                if (captchaFlag)
                {
                    // Это повторный запрос капчи по этому юзеру, пожаловаться
                    sendErrorCaptcha();
                }
                captchaFlag = true; ;
                ans = ans.Replace("\"", "");
                ans = ans.Replace("\\", "");
                int i = ans.IndexOf("captcha_img");
                if (i > 0)
                {
                    i = ans.IndexOf("https");
                    int i2 = ans.IndexOf("}}", i);
                    url = ans.Substring(i, i2 - i);
                    ans = ans.Replace("value", "");
                    ans = ans.Replace(":", "");
                    ans = ans.Replace("}", "");
                    ans = ans.Replace("keycap", "");
                    pictureBox1.Image = null;
                    flLoad = false;
                    pictureBox1.ImageLocation = url;
                    button4.Enabled = true;
                    i = ans.IndexOf("captcha_sid")+11;
                    i2 = ans.IndexOf(",", i);
                    memSID = ans.Substring(i, i2 - i);
                    return 4; // капча
                }
            } else
            {
                if (ans.IndexOf(":1}") > 0)  return 1; // Ok
                if (ans.IndexOf(":2}") > 0)  return 2; // Ok
            }
            return 0;
        }

        private void sendErrorCaptcha() // жалоба на кпчу
        {
            string sURL;
            sURL = "http://rucaptcha.com/res.php?key=" + textBox3.Text.Trim() + "&action=reportbad&id=" + idCap;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            request.Method = "GET";
            request.Accept = "text/plain"; 
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            status.Text = output.ToString();
        }

        private int offFriends(string id)
        {
            string url = "https://api.vk.com/method/friends.delete";
            string str = "access_token=" + prg_ID;
            str += "&user_id=" + id.ToString().Trim();
            string ans = POST(url, str);
            return 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sendCaptcha();
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                sendCaptcha();
            }
        }

        private void sendCaptcha()
        {
            // ответ на капчу
            button4.Enabled = false;
            pictureBox1.Image = null;
            string s = fndUsers[memIndex].id + "&captcha_sid=" + memSID + "&captcha_key=" + textBox2.Text.ToString();
            textBox2.Text = "";
            if (fndUsers[memIndex].friend_status == "0")
            {
                int i = addToFriend(s);
                if (i == 1)
                {
                    if (!checkBox7.Checked) fndUsers[memIndex].pict.setFriendStatus("1");
                    fndUsers[memIndex].friend_status = "1";
                }
                return;
            }
            else
            {
                int i = offFriends(s);
                if (!checkBox7.Checked)fndUsers[memIndex].pict.setFriendStatus("0");
                fndUsers[memIndex].friend_status = "0";
                return;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // пригласить всех
            if (fndUsers == null) return;
            autoAdd = true;
            button5.Enabled = false;
            button6.Visible = true;
            groupBox1.Enabled = false;
            label11.Text = "Start";
            label11.Visible = true;
            for(int i=0; i< fndUsers.Length; i++)
            {
                if (fndUsers[i].friend_status == "0" || fndUsers[i].friend_status == "2")
                {
                    toScreen(i);
                    captchaFlag = false;
                    fndBtnPress(i);
                    decimal tm = numUD.Value * 4;
                    for (int j = 0; j < tm; j++) // pause
                    {
                        System.Threading.Thread.Sleep(250);
                        Application.DoEvents();
                        if (!autoAdd)
                        {
                            break;
                        }
                    }
                    if (!autoAdd)
                    {
                        break;
                    }
                }
                label11.Text = (i+1).ToString() + " из " + fndUsers.Length.ToString();
            }
            label11.Visible = false;
            autoAdd = false;
            button5.Enabled = true;
            button6.Visible = false;
            groupBox1.Enabled = true;

        }

        private void toScreen(int i)
        {
            // сдвинуть юзеров так, чтобы нужный попал на экран
            if (checkBox7.Checked) return;
           if (fndUsers[i].pict.Top> panel1.Height)
            {
                int d = fndUsers[i].pict.Top - panel1.Height + fndUsers[i].pict.Height;
                for (int j = 0; j < fndUsers.Length; j++)
                {
                    fndUsers[j].pict.Top -= d;
                }
                vScrollBar1.Value += d;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            autoAdd = false;
        }

        private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            flLoad = true;
        }

        private void button7_Click(object sender, EventArgs e) // повторить поиск
        {
            if (searchLine == null) return;
            string url = "https://api.vk.com/method/users.search";
            string ans = POST(url, searchLine);
            getInfomUsers(ref fndUsers, ans);
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.idCap = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void numUD_Leave(object sender, EventArgs e)
        {
            Properties.Settings.Default.tm = numUD.Value;
            Properties.Settings.Default.Save();
        }

        private void button8_Click(object sender, EventArgs e) // загрузить юзеров по списку
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                clearUsers();
                string linResult = "";
                string[] readText = File.ReadAllLines(openFileDialog1.FileName);
                foreach (string s in readText)
                {
                    if (s.Trim() != "")
                    {
                        linResult += "," + checkLine(s.Trim());
                    } 

                }
                if (linResult != "")
                {
                    linResult = linResult.Substring(1);
                    getUser(linResult);
                }
            }
        }

        private string checkLine(string s) // проверка строки файла списка юзеров на валидность
        {
            int i = s.IndexOf(",");
            if (i == -1)
            {
                if (Int32.TryParse(s, out i))
                {
                    return s;
                } else
                {
                    return getUserID(s);
                }
            }
            int j = 0;
            string s2;
            string sr = "";
            while (i>-1)
            {
                s2 = s.Substring(j, i);
                j = i + 1;
                if (Int32.TryParse(s2, out i))
                {
                    sr += s2 + ",";
                } else
                {
                    sr += getUserID(s2) + ",";
                }
                i = s.IndexOf(",", j);
            }
            s2 = s.Substring(j);
            j = i + 1;
            if (Int32.TryParse(s2, out i))
            {
                sr += s2 + ",";
            }
            else
            {
                sr += getUserID(s2) + ",";
            }
            i = s.IndexOf(",", j);
            return sr;
        }

        private void getUser(string s) // найти этих юзеров
        {   // поиск по списку
            string url = "https://api.vk.com/method/users.get";
            string str = "access_token=" + prg_ID;
            str += "&user_ids=" + s;
            str += "&fields=friend_status,photo_50&v=5.60";
            searchLine = str;
            string ans = POST(url, str);
            getInfomUsers(ref fndUsers, ans);
        }

        private string getUserID(string name) // найти ID юзера по его имени
        {
            string url = "https://api.vk.com/method/users.search";
            string str = "access_token=" + prg_ID;
            str += "&q=" + name;
            str += "&v=5.60";
            string ans = POST(url, str);
            ans = ans.Replace("\"", "");
            ans = ans.Replace("\\", "");
            if (ans.IndexOf("count:0") > -1) return "";
            int i = ans.IndexOf("id:");
            int j = ans.IndexOf(",", i);
            str = ans.Substring(i + 3, j - i - 3);
            return str;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            searchGroups sg = new searchGroups();
            sg.setID(prg_ID);
            sg.onGroupFnd += setGroupID;
            sg.ShowDialog();
        }

        private void setGroupID(string s)
        {
            textBox1.Text = s;
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {   // сохранить ИД в файл
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(saveFileDialog1.FileName, true))
                {
                    for (int i = 0; i < fndUsers.Length; i++)
                    {
                        file.WriteLine(fndUsers[i].id);
                    }
                }
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                checkBox7.Checked = true;
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if(!checkBox7.Checked && checkBox8.Checked)
            {
                checkBox7.Checked = true;
            }
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            comboBox2.Enabled = textBox4.Text.Trim().Length == 0;
        }
    }


}
