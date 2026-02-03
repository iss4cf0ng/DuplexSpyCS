using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DuplexSpyCS
{
    public partial class frmSSLCert : Form
    {
        public frmSSLCert()
        {
            InitializeComponent();

            Text = "SSL Certificate";
        }

        void fnSetup()
        {
            //Invoke password char.
            checkBox1.Checked = true;
            checkBox1.Checked = false;

            textBox1.Text = "CN=MyTestServer";

            comboBox1.SelectedIndex = 0;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            numericUpDown1.Value = 1;
            numericUpDown1.Minimum = 1;
        }

        private void frmSSLCert_Load(object sender, EventArgs e)
        {
            fnSetup();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string szCertName = textBox1.Text;
            int nLength = int.Parse(comboBox1.Text);
            string szPassword = textBox2.Text;
            int nYear = (int)numericUpDown1.Value;

            using (var rsa = RSA.Create(nLength))
            {
                byte[] abKey = rsa.ExportPkcs8PrivateKey();

                var req = new CertificateRequest(
                    szCertName,
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1
                );

                req.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                        false
                    )
                );

                var cert = req.CreateSelfSigned(
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddYears(nYear)
                );

                byte[] abPFX = cert.Export(X509ContentType.Pfx, szPassword);

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Application.StartupPath;
                sfd.Filter = "PFX file (*.pfx)|*.pfx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, abPFX);

                    if (File.Exists(sfd.FileName))
                        MessageBox.Show("Created certificate: " + sfd.FileName, "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                        MessageBox.Show("Cannot create certificate.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !checkBox1.Checked;
        }
    }
}
