using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SafePlanet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean passwordClipboard = false;
        private PasswordGenerator passwordGen;
        private PasswordManager pwMGR;

        public MainWindow(string file, string pw)
        {
            InitializeComponent();

            pwMGR = new PasswordManager();
            pwMGR.pwfile = file;

            if (System.IO.File.Exists(file))
            {
                Boolean endloop = false;
                do
                {

                    pwMGR.MasterPassword = Utilities.GetSHA256Hash(pw);
                    pwMGR.loadPasswordlist();
                    if (pwMGR.passwordList.Count == 0)
                    {
                        pw = "";
                        if (Utilities.InputBox("Password safe", "Your password:", ref pw) != System.Windows.Forms.DialogResult.OK)
                        {
                            endloop = true;
                            pw = "";
                        }
                    }
                    else
                    {
                        endloop = true;
                    }
                }
                while (endloop == false);
            }
            else
            {
                pw = "";
                if (Utilities.InputBox("Password safe", "Your password:", ref pw) != System.Windows.Forms.DialogResult.OK)
                {
                    pw = "";
                }
                else
                {
                    pwMGR.MasterPassword = Utilities.GetSHA256Hash(pw);
                }
            }

            if (pw == "")
            {
                this.Close();
            }

            
            ListCollectionView collection = new ListCollectionView(pwMGR.passwordList);
            collection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            dataGrid1.ItemsSource = collection;

            updateDatagrid();
            

        }

        private void updateDatagrid()
        {
            ((ListCollectionView)dataGrid1.ItemsSource).Refresh();
            dataGrid1.Items.Refresh();
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                base.OnSourceInitialized(e);
                HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
                NativeCode.SetWindowDisplayAffinity(source.Handle, 1);
            }
            catch
            {
                MessageBox.Show("Initialisation faild");
            }
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pwMGR.add("", "", "", "", null, 0);
                updateDatagrid();
            }
            catch { }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (PasswordElement el in dataGrid1.SelectedItems)
                {
                    pwMGR.remove(el);
                }

                updateDatagrid();
            }
            catch
            { }
        }


        private void savePasswords()
        {

            try { Directory.CreateDirectory("backup\\"); }
            catch { }
            try { System.IO.File.Delete("backup\\" + pwMGR.pwfile); }
            catch { }
            try { System.IO.File.Move(pwMGR.pwfile, "backup\\" + pwMGR.pwfile); }
            catch { }
            try { System.IO.File.Move(pwMGR.pwfile + ".part", pwMGR.pwfile); }
            catch { }

            pwMGR.pwfile = DateTime.Now.ToShortTimeString() + "." +DateTime.Now.Second + "_" + DateTime.Now.ToShortDateString() + ".safeplanet";
            pwMGR.pwfile = pwMGR.pwfile.Replace(":", ".");

            pwMGR.savePasswordlist();

        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                savePasswords();
            }
            catch
            {
                MessageBox.Show("Save failed!");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (passwordGen != null)
            {
                passwordGen.Close();
            }

            try
            {
                if (pwMGR.isSaved() == false)
                {
                    string message = "Do you want to save?";
                    string caption = "Save settings?";
                    System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
                    System.Windows.Forms.DialogResult result;
                    result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        //pwMGR.savePasswordlist();
                        savePasswords();
                        this.Close();
                    }
                }
            }
            catch { }

            if (passwordClipboard == true)
            {
                Clipboard.SetText("");
            }
           

        }

        private void dataGrid1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                pwMGR.changedData();
            }
            catch { }
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string url = ((PasswordElement)dataGrid1.SelectedItem).URL;
                if (url != "")
                {
                    if (url.Substring(0, 4) != "http")
                    {
                        url = "http://" + url;
                    }
                    System.Diagnostics.Process.Start(url);
                }
            }
            catch { }
            

        }



        private void button5_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string pw = ((PasswordElement)dataGrid1.SelectedItem).getPassword();
                Clipboard.SetText(pw);
                pw = "";
                passwordClipboard = true;
            }
            catch { }
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pw = ((PasswordElement)dataGrid1.SelectedItem).getPassword();
                if (pw != "")
                {
                    Utilities.ShowBox("Your Password:", "Password:", ref pw);
                }
                pw = "";
            }
            catch { }
        }

        private void button7_Click(object sender, RoutedEventArgs e)
        {
            if (passwordGen != null)
            {
                passwordGen.Close();
            }
            passwordGen = new PasswordGenerator();
            passwordGen.Show();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string file = Utilities.SaveFileDialog();
            if (file.Length > 0)
            {
                string encpw = "";
                Utilities.InputBox("Create backup of the secure key!", "Please use a password for the backup:", ref encpw);
                if (pwMGR.exportSecureKey(file, encpw) == false)
                    System.Windows.Forms.MessageBox.Show("Please save your database first!", "Error", System.Windows.Forms.MessageBoxButtons.OK);
            }

        }

        private void button5_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string pw = ((PasswordElement)dataGrid1.SelectedItem).getPassword();
                pw = "µ" + Utilities.TextEncode(pw, Utilities.TextEncode_REPLACESTR, Utilities.TextEncode_SEARCHSTR);
                Clipboard.SetText(pw);
                pw = "";
                passwordClipboard = true;
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)//To get a data changed event when closing the window
        {
            button3.Focus();
        }

    }
}
