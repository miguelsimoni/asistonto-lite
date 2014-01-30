using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Win32;
[assembly: AssemblyTitle("asistonto lite")]
[assembly: AssemblyDescription("miguel.simoni@gmail.com")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Miguel.Simoni")]
[assembly: AssemblyProduct("asistonto lite")]
[assembly: AssemblyCopyright("Copyright © Miguel.Simoni 2008")]
[assembly: AssemblyVersion("10.5.27.1530")]
[assembly: AssemblyFileVersion("2.1.0.0")]

namespace asistonto_lite
{
	public class mainForm : Form
	{
        private NotifyIcon notifyIcon = new NotifyIcon();
		private ContextMenu contextMenu = new ContextMenu();
        private string dataFile;

		public mainForm()
		{
			notifyIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico"));
			notifyIcon.Visible = true;
			notifyIcon.Text = Application.ProductName + " " + Application.ProductVersion;
			notifyIcon.ContextMenu = contextMenu;
			notifyIcon.Click += new EventHandler(notifyIcon_Click);
			notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);

            this.ShowInTaskbar = false;
			this.WindowState = FormWindowState.Minimized;
			this.Load += new EventHandler(mainForm_Activated);
			this.Activated += new EventHandler(mainForm_Activated);

            dataFile = Application.StartupPath + @"\data.txt";
            if(!File.Exists(dataFile))
                File.CreateText(dataFile).Close();
		}

		[STAThread]
        static void Main(string[] args) 
		{
            if(args.Length > 0)
            {
                string arg = args[0].ToLower().Trim().Substring(0, 2);
                switch(arg)
                {
                    case "/s":
                        RunOnStartup(true);
                        break;
                    case "/n":
                        RunOnStartup(false);
                        break;
                    default:
                        break;
                }
            }
            string moduleName = Process.GetCurrentProcess().MainModule.ModuleName;
            string processName = Path.GetFileNameWithoutExtension(moduleName);
            Process[] appProcess = Process.GetProcessesByName(processName);
            if(appProcess.Length > 1)
            {
                MessageBox.Show(Application.ProductName + " ya se está ejecutando.", "oops!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.Run(new mainForm());
            }
        }

		private void mainForm_Activated(object sender, System.EventArgs e)
		{
			this.Hide();
		}

		private void notifyIcon_DoubleClick(object sender, EventArgs e)
		{
			Process.Start("notepad", dataFile);
		}

        public void loadData()
        {
			MenuItem menuItem = null;
            contextMenu.MenuItems.Clear();
            if(File.Exists(dataFile))
            {
                StreamReader input = new StreamReader(dataFile, Encoding.Default);
                string item;
                while((item = input.ReadLine()) != null)
				{
					menuItem = null;
                    if(item.IndexOf("|") > 0)
					{
                        string submenu = item.Split('|')[0];
                        foreach(MenuItem mit in contextMenu.MenuItems)
                        {
                            if(mit.Text == submenu)
                            {
                                menuItem = mit;
                            }
                        }
                        if(menuItem == null)
                        {
                            menuItem = new MenuItem(submenu);
                        }
                        item = item.Substring(item.IndexOf("|") + 1);
                        menuItem.MenuItems.Add(new MenuItem(item, new EventHandler(mitGeneric_Click)));
					}
                    else
                    {
					    menuItem = new MenuItem(item, new EventHandler(mitGeneric_Click));
                    }
                    if(!contextMenu.MenuItems.Contains(menuItem))
                        contextMenu.MenuItems.Add(menuItem);
				}
                input.Close();
            }
            contextMenu.MenuItems.Add("-");
            menuItem = new MenuItem("[ editar... ]", new EventHandler(notifyIcon_DoubleClick));
            contextMenu.MenuItems.Add(menuItem);
            menuItem = new MenuItem("[ web... ]", new EventHandler(mitSite_Click));
            contextMenu.MenuItems.Add(menuItem);
            menuItem = new MenuItem("[ salir ]", new EventHandler(mitExit_Click));
            contextMenu.MenuItems.Add(menuItem);
        }

        private void notifyIcon_Click(object sender, EventArgs e)
		{
			loadData();
		}

        private void mitSite_Click(object sender, EventArgs e)
        {
            Process.Start("http://sites.google.com/site/asistonto");
        }

        private void mitExit_Click(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Exit();
        }

        private void mitGeneric_Click(object sender, EventArgs e)
		{
            string value = ((MenuItem)sender).Text;
            if(value.IndexOf("\t") > -1)
                value = value.Split('\t')[1];
            doAction(value);
        }

        private static void doAction(string value)
        {
            string patternUrl = @"^(((https?|ftp)\://)?((\[?(\d{1,3}\.){3}\d{1,3}\]?)|(([-a-zA-Z0-9]+\.)+[a-zA-Z]{2,4}))(\:\d+)?(/[-a-zA-Z0-9._?,'+&amp;%$#=~\\]+)*/?)$";
            string patternMailto = @"^mailto\:\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
            string patternLocalUrl = @"^\\\\\w+$";
            string patternPath = @"^[a-zA-Z]:[\\\w*(\.\w+)+]*$";
            if(Regex.Match(value, patternUrl).Success)
            {
                try
                {
                    Process.Start(value);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Dirección inválida\n" + ex.Message, "oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if(Regex.Match(value, patternMailto).Success)
            {
                try
                {
                    Process.Start(value);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("E-mail inválido\n" + ex.Message, "oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if(Regex.Match(value, patternLocalUrl).Success)
            {
                try
                {
                    Process.Start(value);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Dirección de red inválida\n" + ex.Message, "oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if(Regex.Match(value, patternPath).Success)
            {
                try
                {
                    Process.Start(value);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Dirección local inválida\n" + ex.Message, "oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Clipboard.SetDataObject(value, true);
            }
        }

        private static void RunOnStartup(bool run)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            if(regKey != null)
            {
                try
                {
                    if(run)
                        regKey.SetValue("asistonto_lite", Assembly.GetExecutingAssembly().Location);
                    else
                        regKey.DeleteValue("asistonto_lite");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("No se pudo configurar la aplicación para correr al inicio.\n\n" + ex.Message);
                }
                finally
                {
                    regKey.Close();
                }
            }
        }

	}

}
