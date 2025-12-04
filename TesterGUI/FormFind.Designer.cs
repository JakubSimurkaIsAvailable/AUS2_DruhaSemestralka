namespace TesterGUI
{
    partial class FormFind
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBoxIdentify = new TextBox();
            labelUnique = new Label();
            buttonFind = new Button();
            SuspendLayout();
            // 
            // textBoxIdentify
            // 
            textBoxIdentify.Location = new Point(12, 28);
            textBoxIdentify.Name = "textBoxIdentify";
            textBoxIdentify.Size = new Size(152, 23);
            textBoxIdentify.TabIndex = 0;
            // 
            // labelUnique
            // 
            labelUnique.AutoSize = true;
            labelUnique.Location = new Point(12, 10);
            labelUnique.Name = "labelUnique";
            labelUnique.Size = new Size(152, 15);
            labelUnique.TabIndex = 1;
            labelUnique.Text = "Unikatne identifikacne cislo";
            // 
            // buttonFind
            // 
            buttonFind.Location = new Point(92, 82);
            buttonFind.Name = "buttonFind";
            buttonFind.Size = new Size(75, 23);
            buttonFind.TabIndex = 2;
            buttonFind.Text = "Najdi";
            buttonFind.UseVisualStyleBackColor = true;
            // 
            // FormFind
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(179, 117);
            Controls.Add(buttonFind);
            Controls.Add(labelUnique);
            Controls.Add(textBoxIdentify);
            Name = "FormFind";
            Text = "FormFind";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxIdentify;
        private Label labelUnique;
        private Button buttonFind;
    }
}