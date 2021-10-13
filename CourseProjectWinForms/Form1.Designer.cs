namespace CourseProjectWinForms
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.encrypt_but = new System.Windows.Forms.Button();
            this.decrypt_but = new System.Windows.Forms.Button();
            this.generateKey_but = new System.Windows.Forms.Button();
            this.keyBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // encrypt_but
            // 
            this.encrypt_but.Location = new System.Drawing.Point(65, 86);
            this.encrypt_but.Name = "encrypt_but";
            this.encrypt_but.Size = new System.Drawing.Size(139, 31);
            this.encrypt_but.TabIndex = 0;
            this.encrypt_but.Text = "Encrypt";
            this.encrypt_but.UseVisualStyleBackColor = true;
            this.encrypt_but.Click += new System.EventHandler(this.encrypt_but_Click);
            // 
            // decrypt_but
            // 
            this.decrypt_but.Location = new System.Drawing.Point(65, 135);
            this.decrypt_but.Name = "decrypt_but";
            this.decrypt_but.Size = new System.Drawing.Size(139, 31);
            this.decrypt_but.TabIndex = 1;
            this.decrypt_but.Text = "Decrypt";
            this.decrypt_but.UseVisualStyleBackColor = true;
            this.decrypt_but.Click += new System.EventHandler(this.decrypt_but_Click);
            // 
            // generateKey_but
            // 
            this.generateKey_but.Location = new System.Drawing.Point(383, 86);
            this.generateKey_but.Name = "generateKey_but";
            this.generateKey_but.Size = new System.Drawing.Size(139, 31);
            this.generateKey_but.TabIndex = 2;
            this.generateKey_but.Text = "GenerateKey";
            this.generateKey_but.UseVisualStyleBackColor = true;
            this.generateKey_but.Click += new System.EventHandler(this.generateKey_but_Click);
            // 
            // keyBox
            // 
            this.keyBox.Location = new System.Drawing.Point(383, 144);
            this.keyBox.Name = "keyBox";
            this.keyBox.Size = new System.Drawing.Size(501, 22);
            this.keyBox.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 480);
            this.Controls.Add(this.keyBox);
            this.Controls.Add(this.generateKey_but);
            this.Controls.Add(this.decrypt_but);
            this.Controls.Add(this.encrypt_but);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button encrypt_but;
        private System.Windows.Forms.Button decrypt_but;
        private System.Windows.Forms.Button generateKey_but;
        private System.Windows.Forms.TextBox keyBox;
    }
}

