﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;  
using WeifenLuo.WinFormsUI.Docking;

namespace RosDBG
{
    [DebugControl, BuildAtStartup]
    public partial class ReactOSWeb : ToolWindow
    {
        public ReactOSWeb()
        {
            InitializeComponent();
            BrowserView.CanGoBackChanged += BrowserView_CanGoBackChanged;
            BrowserView.CanGoForwardChanged += BrowserView_CanGoForwardChanged;
            BrowserView.GoHome();
            BrowserView.ScriptErrorsSuppressed = true;
        }

        public ReactOSWeb(string Title, string URL)
        {
            InitializeComponent();
            this.Text = Title;
            BrowserView.CanGoBackChanged += BrowserView_CanGoBackChanged;
            BrowserView.CanGoForwardChanged += BrowserView_CanGoForwardChanged;
            BrowserView.Navigate(URL);
            BrowserView.ScriptErrorsSuppressed = true;
        }

        private void BrowserView_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            AddressInput.Text = e.Url.ToString();
        }

        private void navPaste_Click(object sender, EventArgs e)
        {
            BrowserView.Navigate("http://www.reactos.org/paste");
        }

        private void navHome_Click(object sender, EventArgs e)
        {
            BrowserView.GoHome(); 
        }

        private void navBack_Click(object sender, EventArgs e)
        {
            BrowserView.GoBack();  
        }

        private void navForward_Click(object sender, EventArgs e)
        {
            BrowserView.GoForward();
        }

        private void navReload_Click(object sender, EventArgs e)
        {
            BrowserView.Refresh();  
        }

        private void BrowserView_CanGoBackChanged(object sender, EventArgs e)
        {
            navBack.Enabled = BrowserView.CanGoBack;   
        }

        private void BrowserView_CanGoForwardChanged(object sender, EventArgs e)
        {
            navForward.Enabled = BrowserView.CanGoForward;
        }
        
        private void AddressInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                BrowserView.Navigate(((ToolStripComboBox)sender).Text);
        }

        private void BugzillaInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                btnGoJIRA_Click(null, null);
        }

        private bool pIsCmdEnabled(string Cmd)
        {
            if (BrowserView.Document != null)
            {
                IHTMLDocument2 doc2 = (mshtml.IHTMLDocument2)BrowserView.Document.DomDocument;
                if (doc2 != null)
                    return doc2.queryCommandEnabled(Cmd);
            }
            return false;
        }

        public override bool IsCmdEnabled(Commands Cmd)
        {
            switch (Cmd)
            {
                case Commands.Copy:
                    return pIsCmdEnabled("copy");
                case Commands.Save:
                case Commands.SaveAs:
                case Commands.Print:
                    return true;
            }
            return false;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BrowserView.Document != null)
            {
                IHTMLDocument2 htmlDocument = (mshtml.IHTMLDocument2)BrowserView.Document.DomDocument;
                htmlDocument.execCommand("copy", true, null);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BrowserView.Document != null)
            {
                IHTMLDocument2 htmlDocument = (mshtml.IHTMLDocument2)BrowserView.Document.DomDocument;
                htmlDocument.execCommand("selectall", true, null);
            }
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem.Enabled = IsCmdEnabled(Commands.Copy);
        }

        public override void Save(string FileName)
        {
            SaveAs(FileName);
        }

        public override void SaveAs(string FileName)
        {
            if (BrowserView.Document != null)
            {
                IHTMLDocument2 htmlDocument = (mshtml.IHTMLDocument2)BrowserView.Document.DomDocument;
                htmlDocument.execCommand("saveas", true, FileName);
            }
        }

        public override string GetDocumentName()
        {
            return BrowserView.Url.GetComponents(UriComponents.Host, UriFormat.UriEscaped).ToString() + ".html";
        }

        public override void Print(bool ShowDialog)
        {
            if (BrowserView.Document != null)
            {
                IHTMLDocument2 htmlDocument = (mshtml.IHTMLDocument2)BrowserView.Document.DomDocument;
                htmlDocument.execCommand("print", ShowDialog, null);
            }
        }

        private string GetJiraURL()
        {
            string url = "https://jira.reactos.org/browse/";
            string raw = tbJiraInput.Text;

            if (string.IsNullOrEmpty(raw))
                return null;

            int resultTemp = 0;
            if(int.TryParse(raw[0].ToString(), out resultTemp))
            {
                // Starts with a number, so add "CORE-" to the start
                raw = raw.Insert(0, "CORE-");
            }

            url += raw;

            return url;
        }

        private void btnGoJIRA_Click(object sender, EventArgs e)
        {
            string url = GetJiraURL();

            if (url == null)
                MessageBox.Show("Wrong JIRA issue");
            else
                BrowserView.Navigate(url);
        }

        private void btnExternalBrowser_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(BrowserView.Url.ToString());
        }
    }
}
