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
using System.Diagnostics;

namespace WPF_RC4_
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static long sizeOfSourceMess;
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

                string root = @"D:\RealEncAndCom";
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }
                File.Delete(@"D:\RealEncAndCom\EncFile" + fileExtension);
                File.Copy(filenameEnc, @"D:\RealEncAndCom\EncFile" + fileExtension);
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

            string root = @"D:\RealEncAndCom";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            File.Delete(@"D:\RealEncAndCom\ComFile" + fileExtension);
            File.Copy(filenameEnc, @"D:\RealEncAndCom\ComFile" + fileExtension);
        }
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if(checkBox.IsChecked == true)
            {
                checBoxChecked = true;
                filenameCom = filenameEnc;
                if(filenameCom == null)
                {
                    chosenFileCompress.Content = "Файл для сжатия/распаковки не выбран" + filenameCom;
                }
                else
                {
                    chosenFileCompress.Content = "Выбранный файл: " + filenameCom;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.IsChecked == false)
            {
                checBoxChecked = false;
                filenameCom = null;

                if((string)chosenFileCompress.Content == "Файл для сжатия/распаковки не выбран.")
                {
                    chosenFileCompress.Content = "Файл для сжатия/распаковки не выбран.";
                }
            }
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
                string encodePath = @"D:\RealEncAndCom\noteEncode.txt";
                InfoAboutFile file = new InfoAboutFile();

                File.Delete(encodePath);
                getPlainText(file, fileToEncode, true);
                string encodedText = rc4plus(file.plainText, key);
                processText(encodePath, encodedText, true);

                encLabel1.Content = "Файл был зашифрован!";
            }
            else
            {
                encLabel1.Content = "Файл не был зашифрован";
            }
        }

        private void Decode(object sender, RoutedEventArgs e)
        {
            if (!filenameEnc.fileIsEmpty() && !cypherKey.Text.fileIsEmpty())
            {
                string key = cypherKey.Text;
                string fileToDecode = @"D:\RealEncAndCom\noteEncode.txt";
                string decodedPath = @"D:\RealEncAndCom\noteDecode" + fileExtension; 
                InfoAboutFile encodedFile = new InfoAboutFile();

                File.Delete(decodedPath);
                getPlainText(encodedFile, fileToDecode, false);
                string decodedText = rc4plus(encodedFile.plainText, key);
                decodedText = decodedText.Substring(0, (int)sizeOfSourceMess);
                processText(decodedPath, decodedText, false);
                decLabel1.Content = "Файл был расшифрован!";
            }
            else
            {
                decLabel1.Content = "Файл не был расшифрован!";
            }
        }

        private static void processText(string address, string text, bool procType)
        {
            if (procType)
            {
                using (StreamWriter fstream = new StreamWriter(address, false, Encoding.UTF8))
                {
                    fstream.Write(text);
                }
            }
            else
            {
                using (StreamWriter fstream = new StreamWriter(address, false, Encoding.Default))
                {
                    fstream.Write(text);
                }
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

        private static void getPlainText(InfoAboutFile info, string fileToEncode, bool procType) // if file size more than 256 bytes, then need read file few times and return part of file
        {
            char[] array;
            if (procType)
            {
                using (StreamReader fstream = new StreamReader(fileToEncode, Encoding.Default))
                {
                    Console.WriteLine("read File to encode");
                    Console.WriteLine(fstream.BaseStream.Length);
                    sizeOfSourceMess = fstream.BaseStream.Length;
                    array = new char[fstream.BaseStream.Length];
                    fstream.Read(array, 0, array.Length);
                    info.plainText = new string(array);
                }
            }
            else
            {
                using (StreamReader fstream = new StreamReader(fileToEncode, Encoding.UTF8))
                {
                    Console.WriteLine("read File to decode");
                    Console.WriteLine(fstream.BaseStream.Length);
                    array = new char[fstream.BaseStream.Length];
                    fstream.Read(array, 0, array.Length);
                    info.plainText = new string(array);
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

        private void Compress(object sender, RoutedEventArgs e)
        {
            if (!filenameCom.fileIsEmpty())
            {
                string noteCompressed = @"D:\RealEncAndCom\noteCompressed.txt";
                File.Delete(noteCompressed);
                ANSI ascii = new ANSI();
                LZWEncoder encoder = new LZWEncoder();

                string text = File.ReadAllText(filenameCom, System.Text.Encoding.Default);
                ascii.WriteToFile();
                byte[] b = encoder.EncodeToByteList(text);
                File.WriteAllBytes(noteCompressed, b);
                comLabel1.Content = "Файл был сжат!";
            }
            else
            {
                comLabel1.Content = "Файл не был сжат!";
            }
        }

        private void Decompress(object sender, RoutedEventArgs e)
        {
            if (!filenameCom.fileIsEmpty())
            {
                string noteCompressed = @"D:\RealEncAndCom\noteCompressed.txt";
                string noteDecompressed = @"D:\RealEncAndCom\noteDecompressed" + fileExtension;

                File.Delete(noteDecompressed);
                LZWDecoder decoder = new LZWDecoder();

                byte[] bo = File.ReadAllBytes(noteCompressed);
                string decodedOutput = decoder.DecodeFromCodes(bo);
                File.WriteAllText(noteDecompressed, decodedOutput, System.Text.Encoding.Default);
                decomLabel1.Content = "Файл был распакован!";
            }
            else
            {
                decomLabel1.Content = "Файл не был распокван!";
            }
        }

        private void encShowFile(object sender, RoutedEventArgs e)
        {
            string path = @"D:\RealEncAndCom\noteEncode.txt";
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }
        private void decShowFile(object sender, RoutedEventArgs e)
        {
            string path = @"D:\RealEncAndCom\noteDecode" + fileExtension;
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }
        private void comShowFile(object sender, RoutedEventArgs e)
        {
            string path = @"D:\RealEncAndCom\noteCompressed.txt";
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }
        private void decomShowFile(object sender, RoutedEventArgs e)
        {
            string path = @"D:\RealEncAndCom\noteDecompressed" + fileExtension;
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }
    }
}