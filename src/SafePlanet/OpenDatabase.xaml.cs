using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SafePlanet
{

    public class DatabaseView
    {
        private string file;
        public string File
        {
            get { return file; }
            set { file = value; }
        }

        private string id;
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        private bool secKey;
        public bool SecureKey
        {
            get { return secKey; }
            set { secKey = value; }
        }

    }


    /// <summary>
    /// Interaktionslogik für OpenDatabase.xaml
    /// </summary>
    public partial class OpenDatabase : Window
    {
        private string mode = "LOAD";
        private string[] files;

        public OpenDatabase()
        {
            InitializeComponent();
            #if DEBUG
            #else
                 Utilities.secureProcess();
            #endif

            string exePath = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;
            System.IO.Directory.SetCurrentDirectory(exePath);

            DateTime online = Utilities.GetNetworkTime();
            DateTime system = DateTime.Now;
            TimeSpan dif = (online - system);
            if (Math.Abs(dif.TotalHours) > 2)
            {
                MessageBox.Show("Warning! Your system time seems to be wrong?");
            }

            reloadView();

        }

        private void reloadView()
        {

            string exePath = new Uri(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).LocalPath;


            files = System.IO.Directory.GetFiles(exePath, "*.safeplanet");

            if (files.Length == 0)//No file found, create a new one
            {
                this.Hide();
                
                try
                {
                    new MainWindow("data.safeplanet", "").Show();
                }
                catch { }
                try
                {
                    this.Close();
                }
                catch { }
            }
            else
            {
                List<DatabaseView> view = new List<DatabaseView>();
                string oldid = "";
                int error = 0;
                foreach (string file in files)
                {
                    DatabaseView el = new DatabaseView();
                    el.File = System.IO.Path.GetFileName(file);
                    byte[] id = PasswordManager.FromFile_GetID(file);
                    el.ID = System.BitConverter.ToString(id);
                    if (oldid != "" && oldid != el.ID)
                        error = 1;
                    oldid = el.ID;
                    el.SecureKey = (PasswordManager.getSecureKey_FromID(id).Length > 0);
                    if (error == 0 && el.SecureKey == false)
                        error = 2;
                    view.Add(el);
                }
                DatabaseViewControl.ItemsSource = view;
                DatabaseViewControl.IsReadOnly = true;
                DatabaseViewControl.Items.Refresh();


                if (error == 1)
                {
                    ButtonLoad.Content = "Incompatible";
                    ButtonLoad.IsEnabled = false;
                }
                else if (error == 2)
                {
                    ButtonLoad.Content = "Add SecKey";
                    ButtonLoad.IsEnabled = false;
                }
                else if (view.Count > 1)
                {
                    ButtonLoad.Content = "Sync";
                    mode = "SYNC";
                }
                else
                {
                    ButtonLoad.Content = "Load";
                    ButtonLoad.IsEnabled = true;
                    mode = "LOAD";
                }

            }
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

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            PasswordManager pwMGR = new PasswordManager();
            DatabaseView el = (DatabaseView)DatabaseViewControl.SelectedItem;
            //if (el == null && DatabaseViewControl.Items.Count == 1)
            //{
                el = (DatabaseView)DatabaseViewControl.Items[0];
            //}
            if (el != null)
            {
                pwMGR.pwfile = el.File;
                pwMGR.MasterPassword = Utilities.GetSHA256Hash(realPassword);
                pwMGR.loadPasswordlist();
            }
            if (pwMGR.passwordList.Count > 0)
            {
                if (mode == "LOAD")
                {
                    this.Hide();
                    try
                    {
                        MainWindow main = new MainWindow(el.File, realPassword);
                        main.Show();
                    }
                    catch { }
                    this.Close();
                }
                else if (mode == "SYNC")
                {
                    PasswordManager.syncFiles(files, realPassword);
                    reloadView();
                }
            }else
            {
                realPassword = "";
                fakePassword = "";
                MasterPasswordControl.Password = "";
                MasterPasswordControl.Focus();
            }

        }

        private void ImportSecureKeyControl_Click(object sender, RoutedEventArgs e)
        {
            string file = Utilities.OpenFileDialog();
            if (file.Length > 0 && File.Exists(file))
            {
                string encpw = "";
                bool needsPW = false;

                FileStream stream = null;
                try
                {
                    stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None);
                    byte[] id = new byte[16];
                    stream.Read(id, 0, 16);
                    byte[] salt = new byte[16];
                    stream.Read(salt, 0, 16);
                    foreach (byte b in salt)
                    {
                        if (b != 0)
                            needsPW = true;
                    }
                }
                catch { }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }


                if (needsPW)Utilities.InputBox("Load backup of the secure key!", "Password:", ref encpw);
                PasswordManager.importSecureKey(file, encpw);
            }
            reloadView();
        }

        private SendInputClass.ScanCodeShort[] randomKey = {           
            SendInputClass.ScanCodeShort.KEY_0,
            SendInputClass.ScanCodeShort.KEY_1,
            SendInputClass.ScanCodeShort.KEY_2,
            SendInputClass.ScanCodeShort.KEY_3,
            SendInputClass.ScanCodeShort.KEY_4,
            SendInputClass.ScanCodeShort.KEY_5,
            SendInputClass.ScanCodeShort.KEY_6,
            SendInputClass.ScanCodeShort.KEY_7,
            SendInputClass.ScanCodeShort.KEY_8,
            SendInputClass.ScanCodeShort.KEY_9,
            SendInputClass.ScanCodeShort.KEY_A,
            SendInputClass.ScanCodeShort.KEY_B,
            SendInputClass.ScanCodeShort.KEY_C,
            SendInputClass.ScanCodeShort.KEY_D,
            SendInputClass.ScanCodeShort.KEY_E,
            SendInputClass.ScanCodeShort.KEY_F,
            SendInputClass.ScanCodeShort.KEY_G,
            SendInputClass.ScanCodeShort.KEY_H,
            SendInputClass.ScanCodeShort.KEY_I,
            SendInputClass.ScanCodeShort.KEY_J,
            SendInputClass.ScanCodeShort.KEY_K,
            SendInputClass.ScanCodeShort.KEY_L,
            SendInputClass.ScanCodeShort.KEY_M,
            SendInputClass.ScanCodeShort.KEY_N,
            SendInputClass.ScanCodeShort.KEY_O,
            SendInputClass.ScanCodeShort.KEY_P,
            SendInputClass.ScanCodeShort.KEY_Q,
            SendInputClass.ScanCodeShort.KEY_R,
            SendInputClass.ScanCodeShort.KEY_S,
            SendInputClass.ScanCodeShort.KEY_T,
            SendInputClass.ScanCodeShort.KEY_U,
            SendInputClass.ScanCodeShort.KEY_V,
            SendInputClass.ScanCodeShort.KEY_W,
            SendInputClass.ScanCodeShort.KEY_X,
            SendInputClass.ScanCodeShort.KEY_Y,
            SendInputClass.ScanCodeShort.KEY_Z
            };
        private string realPassword = "";
        private string fakePassword = "";
        private Random rnd = new Random();
        private void MasterPasswordControl_PasswordChanged(object sender, RoutedEventArgs e)
        {
            string currentPassword = MasterPasswordControl.Password;
            if (currentPassword.Length == fakePassword.Length)
            {

            }
            else if (currentPassword.Length < fakePassword.Length)//User deleted some char
            {
                realPassword = realPassword.Substring(0, currentPassword.Length);
                fakePassword = fakePassword.Substring(0, currentPassword.Length);
            }
            else if (currentPassword.Length > fakePassword.Length)//User added some char
            {
                realPassword = realPassword + currentPassword.Substring(currentPassword.Length - 1, 1);
                if (rnd.Next(100) > 25)
                {
                    MasterPasswordControl.Password = currentPassword.Substring(0, currentPassword.Length - 1);
                    fakePassword = fakePassword + "*";
                    SendInputClass.SendInputWithAPI(randomKey[rnd.Next(randomKey.Length - 1)]);
                    MasterPasswordControl.GetType().GetMethod("Select", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(MasterPasswordControl, new object[] { 1000, 0 });
                }
                else
                {
                    fakePassword = fakePassword + "-";
                }
            }

            //System.Diagnostics.Debug.Print("Fake: " + fakePassword);
            //System.Diagnostics.Debug.Print("Real: " + realPassword);

        }





    }
}
