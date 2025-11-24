namespace TesterGUI
{
    partial class Form2
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

        private void InitializeComponent()
        {
            labelRecord = new Label();
            textBoxName = new TextBox();
            buttonInsert = new Button();
            buttonCancel = new Button();
            textBoxSurname = new TextBox();
            label1 = new Label();
            label2 = new Label();
            dateTimePicker1 = new DateTimePicker();
            SuspendLayout();
            // 
            // labelRecord
            // 
            labelRecord.AutoSize = true;
            labelRecord.Location = new Point(19, 22);
            labelRecord.Name = "labelRecord";
            labelRecord.Size = new Size(42, 15);
            labelRecord.TabIndex = 0;
            labelRecord.Text = "Name:";
            labelRecord.Click += labelRecord_Click;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(100, 19);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(188, 23);
            textBoxName.TabIndex = 1;
            // 
            // buttonInsert
            // 
            buttonInsert.Location = new Point(115, 125);
            buttonInsert.Name = "buttonInsert";
            buttonInsert.Size = new Size(75, 23);
            buttonInsert.TabIndex = 2;
            buttonInsert.Text = "Insert";
            buttonInsert.UseVisualStyleBackColor = true;
            buttonInsert.Click += ButtonInsert_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(213, 125);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // textBoxSurname
            // 
            textBoxSurname.Location = new Point(100, 48);
            textBoxSurname.Name = "textBoxSurname";
            textBoxSurname.Size = new Size(188, 23);
            textBoxSurname.TabIndex = 5;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(19, 51);
            label1.Name = "label1";
            label1.Size = new Size(57, 15);
            label1.TabIndex = 4;
            label1.Text = "Surname:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(19, 80);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 6;
            label2.Text = "Birth Date:";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(100, 80);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(188, 23);
            dateTimePicker1.TabIndex = 7;
            // 
            // Form2
            // 
            AcceptButton = buttonInsert;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(309, 165);
            Controls.Add(dateTimePicker1);
            Controls.Add(label2);
            Controls.Add(textBoxSurname);
            Controls.Add(label1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonInsert);
            Controls.Add(textBoxName);
            Controls.Add(labelRecord);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form2";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Insert Record";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelRecord;
        private TextBox textBoxName;
        private Button buttonInsert;
        private Button buttonCancel;
        private TextBox textBoxSurname;
        private Label label1;
        private Label label2;
        private DateTimePicker dateTimePicker1;
    }
}