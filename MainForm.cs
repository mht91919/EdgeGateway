using CefSharp;
using CefSharp.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace EdgeGateway
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RestService _rest;

        private void MainForm_Load(object sender, EventArgs e)
        {
            _rest = RestService.CreateInstance();


            _rest.CloseHandle = HideFrom;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if(RestService.FormStytle == 1)
                this.FormBorderStyle= FormBorderStyle.Sizable;
            else
                this.FormBorderStyle= FormBorderStyle.None;

            //var path = "http://127.0.0.1:99/index.html";

            var path = "http://127.0.0.1:83/index.html";
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lanhu_huabanbeifen5\\index.html");

            chromiumWebBrowser1.LoadUrl(path);

        }



        private void HideFrom()
        {
            this.DialogResult = DialogResult.OK;
            //this.Invoke(new Action(() =>
            //{
            //    Form2 form2 = new Form2();
            //    form2.Show();

            //    this.Hide();
            //}));
        }
    }
}
