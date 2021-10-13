using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseProjectWinForms
{   

    public partial class Form1 : Form
    {
        private SerpentCipher serpent;
        

        public Form1()
        {
            InitializeComponent();
            serpent = new SerpentCipher { AlphabetLength = 256 };

            var key = serpent.RandomizeKey();
            // keyBox.Text = key.ToString();

            var vd = new Validation();
            
        }

        private void encrypt_but_Click(object sender, EventArgs e)
        {
            
            //char[] keyyy = keyBox.Text.ToArray<char>();
            //byte[] key=new byte[keyyy.Length];

            
            byte[] key = new byte[] { 230, 160, 122, 182, 157, 94, 110, 236, 153, 221, 116, 127, 220, 152, 145, 107, 194, 29, 168, 193, 100, 254, 159, 147, 244, 141, 15, 140, 139, 111, 43, 225 };
           //  serpent.Encrypt(sourceFile, key, rounds, algMode, encrMode);
            string sourceFile = "D://Deach_3//2_сем//Защита инфы//Курсовой//MyProject//CourseProjectWinForms//text.txt";
            var algMode = Mode.Standard;
            var encrMode = EncryptionMode.CBC;
            int rounds = 32;
            serpent.Encrypt(sourceFile,key,rounds, algMode, encrMode);

        }

        private void decrypt_but_Click(object sender, EventArgs e)
        {
            serpent = new SerpentCipher { AlphabetLength = 256 };
            byte[] key = new byte[] { 230, 160, 122, 182, 157, 94, 110, 236, 153, 221, 116, 127, 220, 152, 145, 107, 194, 29, 168, 193, 100, 254, 159, 147, 244, 141, 15, 140, 139, 111, 43, 225 };
            string sourceFile = "D://Deach_3//2_сем//Защита инфы//Курсовой//MyProject//CourseProjectWinForms//text.serpent";
            var algMode = Mode.Standard;
            var encrMode = EncryptionMode.CBC;
            int rounds = 32;
            serpent.Decrypt(sourceFile,key,rounds, algMode, encrMode);
        }

        private void generateKey_but_Click(object sender, EventArgs e)
        {
            keyBox.Text = string.Empty;
          //  var key = 
           // keyBox.Text = key.ToString();

           
          //  keyBox.Text = vd.GetKeyString(key, KeyMode.Bytes);

        }
    }
}
