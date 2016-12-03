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
    public partial class InputForm : Form
    {

        public delegate void fndUserButtonPress(string st);  // делегат метода обратного вызова от кнопки 
        public event fndUserButtonPress onButtonPress;

        public InputForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            onButtonPress(textBox1.Text);
            Close();
        }

    }
}
