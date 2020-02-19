using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Security;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace failu_sirfavimas
{
    public partial class Form1 : Form
    {
        biblioteka b = new biblioteka();
        ManualResetEvent resetEvent = new ManualResetEvent(true);
        public string slaptazodis = "domantas";
        public string[] uzkoduotosReiksmes = new string[100];
        public bool Bdecrypt = true;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
			//textBox1.Text = @"C:\Users\doman\Kodavimas\failai";
            if (File.Exists(textBox1.Text + @"\md5hash78547.txt"))
            {
                b.DecryptFile(textBox1.Text + @"\md5hash78547.txt", slaptazodis);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(() => compress(textBox1.Text, "compress"));
            th.IsBackground = true;
            th.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread th3 = new Thread(decrypt);
            th3.IsBackground = true;
            th3.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = b.catalogBrowser();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            resetEvent.Reset();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            resetEvent.Set();
        }

        public void compress(string text, string comp)
        {
            try
            {
                if (text == "")
                {
                    MessageBox.Show("Pirma nurodykite katalogą!");
                }
                else
                {
                    if (!File.Exists(textBox1.Text + @"\md5hash78547.txt"))
                    {
                        string[] dirs = Directory.GetDirectories(text, "*");
                        if (dirs.Length != 0)
                        {
                            int i = 0;
                            while (i < dirs.Length)
                            {

                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    label1.Text = "Archyvuojama: ";
                                    label2.Text = (((i + 1) * 50) / dirs.Length).ToString() + "%";
                                });
                                this.BeginInvoke((MethodInvoker)delegate
                                {
                                    progressBar1.Value = ((i + 1) * 50) / dirs.Length;
                                });
                                if(comp == "compress")
                                {
                                    ZipFile.CreateFromDirectory(dirs[i], dirs[i] + ".zip", CompressionLevel.Fastest, true);
                                    Directory.Delete(dirs[i], true);
                                }

                                i++;
                                Thread.Sleep(1000);
                                resetEvent.WaitOne();
                            }
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                progressBar1.Value = 50;
                                label2.Text = "50%";
                            });
                            MessageBox.Show("Archyvavimas baigtas!");

                            Thread th2 = new Thread(endecrypt);
                            th2.IsBackground = true;
                            th2.Start();
                        }
                        else
                        {
                            Thread th2 = new Thread(endecrypt);
                            th2.IsBackground = true;
                            th2.Start();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failas jau užkoduotas");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida skaičiuojant katalogus! " + ex);
            }
        }

        public void endecrypt()
        {
            try
            {
                string[] dirs = Directory.GetFiles(textBox1.Text);                
                using (StreamWriter outputFile = new StreamWriter(textBox1.Text + @"\md5hash78547.txt"))
                {
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        Console.WriteLine(dirs[i]);
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            label1.Text = "Šifruojama: ";
                            label2.Text = (50 + (((i + 1) * 50) / dirs.Length)).ToString() + "%";
                        });
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                        //progressBar1.Minimum = 50;
                        progressBar1.Value = (50 + ((i + 1) * 50) / dirs.Length);
                        });
                        if (dirs[i] != textBox1.Text + @"\md5hash78547.txt")
                        {
                            uzkoduotosReiksmes[i] = b.EncryptFile(dirs[i], slaptazodis);
                            outputFile.WriteLine(uzkoduotosReiksmes[i]);
                           // outputFile.WriteLine(b.EncryptFile(dirs[i], slaptazodis));
                        }
                        else                            
                            MessageBox.Show("Apsauga!");

                        Thread.Sleep(1000);
                        resetEvent.WaitOne();
                    }
                }
                this.BeginInvoke((MethodInvoker)delegate
                {
                    progressBar1.Value = 100;
                    label2.Text = "100%";
                });
                MessageBox.Show("Šifravimas baigtas!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida koduojant katalogus! " + ex);
            }
        }

        public void decrypt()
        {
            try
            {
                Bdecrypt = true;
                string[] dirs = Directory.GetFiles(textBox1.Text);
                string[] dirs1 = Directory.GetDirectories(textBox1.Text, "*");
                string[] lines = File.ReadAllLines(textBox1.Text + @"\md5hash78547.txt");
                string sub;
                string bezip;
                byte[] bytesToBeEncrypted;
                string md5paswordas;
         
                if (File.Exists(textBox1.Text + @"\md5hash78547.txt") && Bdecrypt == true)
                {
                    for (int i = 0; i < dirs.Length - 1; i++)
                    {
                        bytesToBeEncrypted = File.ReadAllBytes(dirs[i]);                        
                        md5paswordas = b.CalculateMD5Hash(bytesToBeEncrypted);
                        Console.WriteLine(dirs[i]);
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                label1.Text = "Atkoduojama: ";
                                label2.Text = (((i + 1) * 100) / dirs.Length - 1).ToString() + "%";
                            });
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                            //progressBar1.Minimum = 50;
                            progressBar1.Value = (((i + 1) * 100) / dirs.Length - 1);
                            });
                            if (dirs[i] != textBox1.Text + @"\md5hash78547.txt")
                            {
                                if (md5paswordas == lines[i])
                                {
                                    b.DecryptFile(dirs[i], slaptazodis);
                                    sub = dirs[i].Substring(dirs[i].Length - 4, 4);                                   
                                    if (sub == ".zip")
                                    {
                                        bezip = dirs[i].Substring(0, dirs[i].Length - 4);
                                        ZipFile.ExtractToDirectory(dirs[i], bezip);
                                        File.Delete(dirs[i]);
                                    }

                                }                                    
                                else
                                    MessageBox.Show("Failo kurio užsifruota reiksmė lygi:" + dirs[i] + " nesutampa");
                            }
                            else
                            {
                                for (int j = i; j < dirs.Length - 1; j++)
                                    dirs[j] = dirs[j + 1];
                                i--;
                            }                        
                            // MessageBox.Show("Apsauga!");
                        Thread.Sleep(1000);
                        resetEvent.WaitOne();
                    }
                    File.Delete(textBox1.Text + @"\md5hash78547.txt");
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        progressBar1.Value = 100;
                        label2.Text = "100%";
                    });
                    MessageBox.Show("Atkodavimas baigtas!");
                }
                else
                {
                    MessageBox.Show("Failas neužšifruotas: error 5");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida atkoduojant katalogus! " + ex);
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*DialogResult dialog = MessageBox.Show("Do you really want to close the program?", "SomeTitle", MessageBoxButtons.YesNo);
            if (dialog == DialogResult.Yes)
            {
                if (File.Exists(textBox1.Text + @"\md5hash78547.txt"))
                {
                    b.EncryptFile(textBox1.Text + @"\md5hash78547.txt", slaptazodis);
                }
                Application.Exit();
            }
            else if (dialog == DialogResult.No)
            {
               e.Cancel = true;
            }*/
            if (File.Exists(textBox1.Text + @"\md5hash78547.txt"))
            {
                b.EncryptFile(textBox1.Text + @"\md5hash78547.txt", slaptazodis);
            }
        }

        public void close()
        {

        }
    }
}
