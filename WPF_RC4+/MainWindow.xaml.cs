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
using System.IO;
using System.Security.Cryptography;
using SharpLZW;
using Validator; 

namespace WPF_RC4_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool processStart = false;
        bool checBoxChecked = false;
        string fileExtension;
        static string filenameEnc;
        static string filenameCom;

        public MainWindow()
        {
            InitializeComponent();
            cypherKey.Text = "barabarabealiquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        }

        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                filenameEnc = dlg.FileName;
                chosenFileChypr.Content = "Выбранный файл: " + filenameEnc;
                if (checBoxChecked)
                {
                    chosenFileCompress.Content = "Выбранный файл: " + filenameEnc;
                }

                fileExtension = System.IO.Path.GetExtension(filenameEnc);
            }
        }

        private void ChooseCompressFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            filenameCom = dlg.FileName;
            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                chosenFileCompress.Content = "Выбранный файл: " + filenameCom;
            }
            
            fileExtension = System.IO.Path.GetExtension(filenameCom);
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if(checkBox.IsChecked == true)
            {
                checBoxChecked = true;
            }
            else
            {
                checBoxChecked = false;
            }
        }

        private void CypherKey_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Generate_Key(object sender, RoutedEventArgs e)
        {
            cypherKey.Text = "";
            for (int i = 0; i < 11; i++)
            {
                TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();
                if (i == 10)
                {
                    string sub = Encoding.Default.GetString(tripleDES.Key).Substring(0, 16);
                    cypherKey.Text += sub;
                    break;
                }
                cypherKey.Text += Encoding.Default.GetString(tripleDES.Key);
            }
            MessageBox.Show(cypherKey.Text.Length.ToString());
        }

        private void Encode(object sender, RoutedEventArgs e)
        {
            if (!filenameEnc.fileIsEmpty() && !cypherKey.Text.fileIsEmpty())
            {
                string key = cypherKey.Text;
                string fileToEncode = filenameEnc;
                string encodePath = @"noteEncode.txt";
                File.Delete(encodePath);

                InfoAboutFile file = new InfoAboutFile();

                do
                {
                    getPlainText(file, fileToEncode);

                    string encodedText = rc4plus(file.plainText, key);
                    byte[] justArray = System.Text.Encoding.Default.GetBytes(encodedText);
                    string justStr = System.Text.Encoding.Default.GetString(justArray);
                    string decoded = rc4plus(justStr, key);
                    processText(encodePath, encodedText);
                }
                while (processStart);
            }
        }

        private void Decode(object sender, RoutedEventArgs e)
        {
            string key = cypherKey.Text;
            string fileToDecode = @"noteEncode.txt";
            string decodedPath = @"noteDecode.txt";
            File.Delete(decodedPath);

            InfoAboutFile encodedFile = new InfoAboutFile();

            do
            {
                getPlainText(encodedFile, fileToDecode);
                string decodedText = rc4plus(encodedFile.plainText, key);
                byte[] encB = System.Text.Encoding.Default.GetBytes(decodedText);
                string iif = System.Text.Encoding.Default.GetString(encB);
                processText(decodedPath, decodedText);
            }
            while (processStart);
        }

        private static void processText(string address, string text)
        {
            using (FileStream fstream = new FileStream(address, FileMode.Append))
            {
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                fstream.Write(array, 0, array.Length);
            }
        }//adding text to encrypted/decrypted files by block

        private static string rc4plus(string slovo, string key) //return encoded/decoded text  
        {
            string chiper;
            VarsValue vars = new VarsValue();
            vars = KSA(vars, key);
            chiper = PRGA(vars, slovo);
            return chiper;
        }
        
        private static void getPlainText(InfoAboutFile info, string fileToEncode) // if file size more than 256 bytes, then need read file few times and return part of file
        {
            byte[] array;

            using (FileStream fstream = new FileStream(fileToEncode, FileMode.Open))
            {
                if (fstream.Length > 256)
                {
                    processStart = true;

                    if (256 * (info.cyclicReRead + 1) < fstream.Length)
                    {
                        info.fileSize = 256;
                        array = new byte[256];
                        fstream.Seek(256 * info.cyclicReRead, SeekOrigin.Begin);
                        fstream.Read(array, 0, 256);
                    }
                    else
                    {
                        info.fileSize = fstream.Length - 256 * info.cyclicReRead;
                        array = new byte[info.fileSize];
                        fstream.Seek(256 * info.cyclicReRead, SeekOrigin.Begin);
                        fstream.Read(array, 0, (int)(info.fileSize));
                        processStart = false;
                    }

                    info.cyclicReRead++;
                    info.plainText = System.Text.Encoding.Default.GetString(array);
                }
                else
                {
                    info.fileSize = fstream.Length;
                    array = new byte[info.fileSize];
                    fstream.Read(array, 0, array.Length);
                    info.plainText = System.Text.Encoding.Default.GetString(array);
                }
            }
        }

        private static VarsValue KSA(VarsValue vars, string key) //(KSA) Key Scheduling Algorithm
        {
            for (vars.n = 0, vars.a = "", vars.s = 0; vars.s < 256; vars.s++) vars.i[vars.s] = vars.s;

            for (vars.s = 0; vars.s < 256; vars.s++)
            {
                vars.n = (vars.n + vars.i[vars.s] + Convert.ToInt32(key.ToCharArray(vars.s % key.Length, 1)[0])) % 256;
                vars.r = vars.i[vars.s];
                vars.i[vars.s] = vars.i[vars.n];
                vars.i[vars.n] = vars.r;
            }

            return vars;
        }

        private static string PRGA(VarsValue vars, string slovo) //(PRGA) Pseudo Random Generation Algorithm
        {
            for (vars.s = 0, vars.n = 0, vars.o = 0, vars.r = 0; vars.o < slovo.Length; vars.o++)
            {
                vars.s = (vars.s + 1) % 256;
                vars.r = vars.i[vars.s];
                vars.n = (vars.n + vars.r) % 256;
                vars.r = vars.i[vars.s];
                vars.i[vars.s] = vars.i[vars.n];
                vars.i[vars.n] = vars.r;
                vars.r = vars.i[((vars.s << 5) ^ (vars.n >> 3)) % 256] + vars.i[((vars.n << 5) ^ (vars.s >> 3)) % 256];
                vars.a += Convert.ToChar((slovo.ToCharArray(vars.o, 1)[0])
                    ^ ((vars.i[(vars.i[vars.s] + vars.i[vars.n]) % 256] + vars.i[(vars.r ^ 0xAA) % 256]) ^ vars.i[(vars.n + vars.i[vars.s]) % 256]) % 256);
            }

            return vars.a;
        }
    }
}