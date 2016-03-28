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
using System.Windows.Shapes;
using System.Windows.Interop;

namespace SafePlanet
{
    /// <summary>
    /// Interaktionslogik für PasswordGenerator.xaml
    /// </summary>
    public partial class PasswordGenerator : Window
    {
        public PasswordGenerator()
        {
            InitializeComponent();
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

        private void updatePassword()
        {
            try
            {
                int len = Convert.ToInt32(PWLen.Text);
                string chars = allowedChars.Text;
                Password.Text = Utilities.RandomString(len, chars);
            }
            catch { }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            updatePassword();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            updatePassword();
        }
    }
}
