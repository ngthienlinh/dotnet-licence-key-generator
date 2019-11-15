using AppSoftware.LicenceEngine.Common;
using AppSoftware.LicenceEngine.KeyGenerator;
using AppSoftware.LicenceEngine.KeyVerification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Licenser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int KeyLength = 3;
        private Dictionary<string, KeyByteSet[]> Apps { get; set; }
        private Dictionary<string, Dictionary<string, int>> AppUserMap { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Apps = new Dictionary<string, KeyByteSet[]>();
            AppUserMap = new Dictionary<string, Dictionary<string, int>>();
        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtApp.Text) || string.IsNullOrEmpty(txtUser.Text))
            {
                return;
            }

            try
            {
                btnGen.IsEnabled = false;

                var random = new Random();

                if (!Apps.ContainsKey(txtApp.Text))
                {

                    if (txtApp.Text == "CopyTool")
                    {
                        Apps.Add(txtApp.Text, new KeyByteSet[] {
                            new KeyByteSet(1, 177, 99, 0),
                            new KeyByteSet(2, 186, 153, 17),
                            new KeyByteSet(3, 7, 113, 200),
                            //new KeyByteSet(4, 73, 102, 49),
                            //new KeyByteSet(5, 61, 173, 33),
                        });
                    } else
                    {
                        var newKeySet = new KeyByteSet[KeyLength];
                        for (int j = 0; j < KeyLength; j++)
                        {
                            newKeySet[j] = new KeyByteSet(j + 1, (byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));
                        }

                        Apps.Add(txtApp.Text, newKeySet);
                    }
                    AppUserMap.Add(txtApp.Text, new Dictionary<string, int>());

                }

                if (!AppUserMap[txtApp.Text].ContainsKey(txtUser.Text))
                {
                    AppUserMap[txtApp.Text].Add(txtUser.Text, AppUserMap[txtApp.Text].Count);
                }

                var pkvLicenceKey = new PkvLicenceKeyGenerator();

                int seed = AppUserMap[txtApp.Text][txtUser.Text];
                // Generate the key
                string key = pkvLicenceKey.MakeKey(seed, Apps[txtApp.Text]);

                var kbs1 = Apps[txtApp.Text][new Random().Next(0, KeyLength - 1)];
                var kbs2 = Apps[txtApp.Text][new Random().Next(0, KeyLength - 1)];

                // The check project also uses a class called KeyByteSet, but with
                // separate name spacing to achieve single self contained dll

                var keyByteSet1 = new KeyByteSet(kbs1.KeyByteNo, kbs1.KeyByteA, kbs1.KeyByteB, kbs1.KeyByteC); // Change no to test others
                var keyByteSet2 = new KeyByteSet(kbs2.KeyByteNo, kbs2.KeyByteA, kbs2.KeyByteB, kbs2.KeyByteC);

                txtLicense.Text = key;

                var partialKeys = new[] { keyByteSet1, keyByteSet2 };

                CheckKey(key, KeyLength, partialKeys);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Sorry! cannot generate license key!");
            }
            finally
            {
                btnGen.IsEnabled = true;
            }
        }

        private bool CheckKey(string key, int setLength, KeyByteSet[] partialKeys)
        {
            var pkvKeyCheck = new PkvKeyCheck();
            // Check that check sum validation passes

            if (!pkvKeyCheck.CheckKeyChecksum(key, setLength))
            {
                return false;
            }

            // Check using full check method
            if (pkvKeyCheck.CheckKey(key, partialKeys, setLength, null) != PkvLicenceKeyResult.KeyGood)
            {
                return false;
            }

            return true;                        
        }
    }
}
