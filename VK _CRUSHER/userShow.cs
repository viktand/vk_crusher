using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VK__CRUSHER
{
    public partial class userShow : UserControl
    {
        int myIndex;
        public delegate void fndUserButtonPress(int index);  // делегат метода обратного вызова от кнопки юзера (дружить или нет)
        public event fndUserButtonPress onFndPress;

        public userShow()
        {
            InitializeComponent();
        }

        public void setIndex(int i)
        {
            myIndex = i;
        }

        public void setName(string nm)
        {
            label1.Text = nm;
        }

        public void seID(string id)
        {
            label2.Text = id;
        }

        public void buttonText(string tx)
        {
            button1.Text = tx;
        }

        public void setPicture(string url)
        {
            pictureBox1.ImageLocation = url;
        }

        public void setFriendStatus(string s)
        {
            pictureBox2.Visible = false;
            button2.Visible = false;
            switch (s)
            {
                case "0":
                    label3.Text = "Не друг";
                    button1.Text = "Добавить в друзья";
                    break;
                case "1":
                    label3.Text = "Напр. заявка на дружбу";
                    button1.Text = "Отписаться";
                    pictureBox2.Visible = true;
                    break;
                case "2":
                    label3.Text = "Получ.заявка на дружбу";
                    button1.Text = "Добавить в друзья";
                    button2.Visible = true;
                    break;
                case "3":
                    label3.Text = "Друг";
                    button1.Text = "Отписаться";
                    pictureBox2.Visible = true;
                    break;
                case "b":
                    label3.Text = "Юзер забанен";
                    button1.Visible = false;
                    break;
                default:
                    label3.Text = "?";
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // нажатие на кноку
            onFndPress(myIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            onFndPress(-1);
            onFndPress(myIndex);
        }
    }
}
