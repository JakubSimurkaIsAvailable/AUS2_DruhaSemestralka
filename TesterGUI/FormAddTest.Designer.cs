namespace TesterGUI
{
    partial class FormAddTest
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
            textBoxPatientUID = new TextBox();
            textBoxTestResult = new TextBox();
            textBox3 = new TextBox();
            textBoxNote = new TextBox();
            dateTimePicker1 = new DateTimePicker();
            labelPatientUID = new Label();
            labelTestResult = new Label();
            labelTestValue = new Label();
            labelTestDate = new Label();
            labelNote = new Label();
            buttonAdd = new Button();
            SuspendLayout();
            // 
            // textBoxPatientUID
            // 
            textBoxPatientUID.Location = new Point(12, 44);
            textBoxPatientUID.Name = "textBoxPatientUID";
            textBoxPatientUID.Size = new Size(215, 23);
            textBoxPatientUID.TabIndex = 0;
            // 
            // textBoxTestResult
            // 
            textBoxTestResult.Location = new Point(12, 104);
            textBoxTestResult.Name = "textBoxTestResult";
            textBoxTestResult.Size = new Size(215, 23);
            textBoxTestResult.TabIndex = 1;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(12, 164);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(215, 23);
            textBox3.TabIndex = 2;
            // 
            // textBoxNote
            // 
            textBoxNote.Location = new Point(12, 294);
            textBoxNote.Name = "textBoxNote";
            textBoxNote.Size = new Size(215, 23);
            textBoxNote.TabIndex = 3;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 229);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(215, 23);
            dateTimePicker1.TabIndex = 4;
            // 
            // labelPatientUID
            // 
            labelPatientUID.AutoSize = true;
            labelPatientUID.Location = new Point(12, 26);
            labelPatientUID.Name = "labelPatientUID";
            labelPatientUID.Size = new Size(129, 15);
            labelPatientUID.TabIndex = 5;
            labelPatientUID.Text = "Unikatne cislo pacienta";
            // 
            // labelTestResult
            // 
            labelTestResult.AutoSize = true;
            labelTestResult.Location = new Point(12, 86);
            labelTestResult.Name = "labelTestResult";
            labelTestResult.Size = new Size(215, 15);
            labelTestResult.TabIndex = 6;
            labelTestResult.Text = "Vysledok testu (1 pozitivny 0 negativny)";
            // 
            // labelTestValue
            // 
            labelTestValue.AutoSize = true;
            labelTestValue.Location = new Point(12, 146);
            labelTestValue.Name = "labelTestValue";
            labelTestValue.Size = new Size(83, 15);
            labelTestValue.TabIndex = 7;
            labelTestValue.Text = "Hodnota testu";
            // 
            // labelTestDate
            // 
            labelTestDate.AutoSize = true;
            labelTestDate.Location = new Point(12, 211);
            labelTestDate.Name = "labelTestDate";
            labelTestDate.Size = new Size(72, 15);
            labelTestDate.TabIndex = 8;
            labelTestDate.Text = "Datum testu";
            // 
            // labelNote
            // 
            labelNote.AutoSize = true;
            labelNote.Location = new Point(12, 276);
            labelNote.Name = "labelNote";
            labelNote.Size = new Size(62, 15);
            labelNote.TabIndex = 9;
            labelNote.Text = "Poznamka";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(167, 343);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 10;
            buttonAdd.Text = "Pridaj";
            buttonAdd.UseVisualStyleBackColor = true;
            // 
            // FormAddTest
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(254, 385);
            Controls.Add(buttonAdd);
            Controls.Add(labelNote);
            Controls.Add(labelTestDate);
            Controls.Add(labelTestValue);
            Controls.Add(labelTestResult);
            Controls.Add(labelPatientUID);
            Controls.Add(dateTimePicker1);
            Controls.Add(textBoxNote);
            Controls.Add(textBox3);
            Controls.Add(textBoxTestResult);
            Controls.Add(textBoxPatientUID);
            Name = "FormAddTest";
            Text = "FormAddTest";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxPatientUID;
        private TextBox textBoxTestResult;
        private TextBox textBox3;
        private TextBox textBoxNote;
        private DateTimePicker dateTimePicker1;
        private Label labelPatientUID;
        private Label labelTestResult;
        private Label labelTestValue;
        private Label labelTestDate;
        private Label labelNote;
        private Button buttonAdd;
    }
}