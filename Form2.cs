using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdgeGateway
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        private void Form2_Shown(object sender, EventArgs e)
        {
            if (RestService.FormStytle == 1)

            {
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
            }

            LoadPage();

            //RestService.CreateInstance().Start();
        }

        private void LoadPage()
        {
            //延迟加载页面
            Task.Delay(5000);

            var path = "http://127.0.0.1:99/index.html";
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lanhu_huabanbeifen5\\index.html");

            chromiumWebBrowser1.LoadUrl(path);

        }
    }
}
