using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK__CRUSHER
{
    public partial class recap : Form
    {
        private string _keyID;
        private string idCap;
        public delegate void goodReq(string st, string id);  // делегат метода обратного вызова 
        public event goodReq onGoodReq;

        public recap()
        {
            InitializeComponent();
        }

        public void sendReq(string keyID, Image im)
        {
            _keyID = keyID;
            pictureBox1.Image = im;
            im.Save("capcha.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            string s = getRecaptcha();
            listBox1.Items.Add(s);
            if (s.Substring(0, 2) == "OK")
            {
                idCap = s.Substring(3);
                timer1.Enabled = true;
            }
        }


        private string getRecaptcha() // запросить решение капчи
        {
            string filepath = "capcha.jpg";
            string url = "http://rucaptcha.com/in.php";

            string boundary = System.IO.Path.GetRandomFileName();
            StringBuilder header = new StringBuilder();
            header = getPost(boundary);

            header.Append("Content-Disposition: form-data; name='file';");
            header.AppendFormat("filename='{0}'", System.IO.Path.GetFileName(filepath));
            header.AppendLine();
            header.AppendLine("Content-Type: application/octet-stream");
            header.AppendLine();

            Byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header.ToString());
            Byte[] endboundarybytes = System.Text.Encoding.ASCII.GetBytes(Environment.NewLine + "--" + boundary + "--" + Environment.NewLine);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = "Mozilla/4.0";
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.ContentLength = headerbytes.Length + new System.IO.FileInfo(filepath).Length + endboundarybytes.Length;
            req.Method = "POST";

            System.IO.Stream s = req.GetRequestStream();
            s.Write(headerbytes, 0, headerbytes.Length);
            Byte[] filebytes = System.IO.File.ReadAllBytes(filepath);
            s.Write(filebytes, 0, filebytes.Length);
            s.Write(endboundarybytes, 0, endboundarybytes.Length);
            s.Close();
            WebResponse response = req.GetResponse();
            s = response.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(s);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            s.Close();
            listBox1.Items.Add("Капча отправлена на сервер");
            return responseFromServer;
        }

        private System.Text.StringBuilder getPost(string boundary)
        {
            // вернуть тело пост запроса
            StringBuilder header = new StringBuilder();
            header.AppendLine("--" + boundary);
            header.AppendLine("Content-Disposition: form-data; name='regsense'");
            header.AppendLine();
            header.AppendLine("1");
            header.AppendLine("--" + boundary);
            header.AppendLine("Content-Disposition: form-data; name='soft_id'");
            header.AppendLine();
            header.AppendLine("1635");
            header.AppendLine("--" + boundary);
            header.AppendLine("Content-Disposition: form-data; name='method'");
            header.AppendLine();
            header.AppendLine("post");
            header.AppendLine("--" + boundary);
            header.AppendLine("Content-Disposition: form-data; name='key'"); //26df7e59ff8f598db35348e3cb49b98b
            header.AppendLine();
            header.AppendLine(_keyID);
            header.AppendLine("--" + boundary);
            return header;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // проверка ответа от сервиса рукапчи
            timer1.Stop();
            // Возможно, что здесь нужно вставить проверку прерывания запросов по тайм-ауту (количеству раз)
            string s = getRqw().Trim(); // запросить ответ
            listBox1.Items.Add(s);
            if (s.Substring(0, 2) == "OK")
            {
                onGoodReq(s.Substring(3), idCap);
                System.Threading.Thread.Sleep(250); // время на обработку сигнала (скорость интернета...)
                Close();
                return;
            }
            if (s.Trim() == "CAPCHA_NOT_READY")
            {
                timer1.Start();
                return;
            }
            // выход - неудачное общение с сервисом    
            System.Threading.Thread.Sleep(1000); // чтобы успеть прочитать сообщение
            Close();
        }

        private string getRqw()
        {     //get - запрос ответа
            string sURL;
            sURL = "http://rucaptcha.com/res.php?key=" + _keyID + "&action=get&id=" + idCap;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
            request.Method = "GET";
            request.Accept = "text/plain"; // "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            StringBuilder output = new StringBuilder();
            output.Append(reader.ReadToEnd());
            response.Close();
            return output.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
 }
