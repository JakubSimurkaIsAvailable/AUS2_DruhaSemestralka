namespace TesterGUI
{
    partial class Form3
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
            labelBlock = new Label();
            textBoxBlock = new TextBox();
            labelValue = new Label();
            textBoxValue = new TextBox();
            buttonGet = new Button();
            buttonDelete = new Button();
            buttonCancel = new Button();
            SuspendLayout();
            // 
            // labelBlock
            // 
            labelBlock.AutoSize = true;
            labelBlock.Location = new Point(16, 16);
            labelBlock.Name = "labelBlock";
            labelBlock.Size = new Size(70, 15);
            labelBlock.TabIndex = 0;
            labelBlock.Text = "Block index:";
            // 
            // textBoxBlock
            // 
            textBoxBlock.Location = new Point(100, 13);
            textBoxBlock.Name = "textBoxBlock";
            textBoxBlock.Size = new Size(170, 23);
            textBoxBlock.TabIndex = 1;
            // 
            // labelValue
            // 
            labelValue.AutoSize = true;
            labelValue.Location = new Point(16, 52);
            labelValue.Name = "labelValue";
            labelValue.Size = new Size(82, 15);
            labelValue.TabIndex = 2;
            labelValue.Text = "Record value:";
            // 
            // textBoxValue
            // 
            textBoxValue.Location = new Point(100, 49);
            textBoxValue.Name = "textBoxValue";
            textBoxValue.Size = new Size(170, 23);
            textBoxValue.TabIndex = 3;
            // 
            // buttonGet
            // 
            buttonGet.Location = new Point(16, 90);
            buttonGet.Name = "buttonGet";
            buttonGet.Size = new Size(75, 23);
            buttonGet.TabIndex = 4;
            buttonGet.Text = "Get";
            buttonGet.UseVisualStyleBackColor = true;
            buttonGet.Click += ButtonGet_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.Location = new Point(106, 90);
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(75, 23);
            buttonDelete.TabIndex = 5;
            buttonDelete.Text = "Delete";
            buttonDelete.UseVisualStyleBackColor = true;
            buttonDelete.Click += ButtonDelete_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(196, 90);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 6;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            // 
            // Form3
            // 
            AcceptButton = buttonGet;
            CancelButton = buttonCancel;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(290, 130);
            Controls.Add(buttonCancel);
            Controls.Add(buttonDelete);
            Controls.Add(buttonGet);
            Controls.Add(textBoxValue);
            Controls.Add(labelValue);
            Controls.Add(textBoxBlock);
            Controls.Add(labelBlock);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form3";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Get / Delete Record";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelBlock;
        private TextBox textBoxBlock;
        private Label labelValue;
        private TextBox textBoxValue;
        private Button buttonGet;
        private Button buttonDelete;
        private Button buttonCancel;
    }
}