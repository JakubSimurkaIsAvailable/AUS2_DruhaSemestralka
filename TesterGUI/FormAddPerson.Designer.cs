namespace TesterGUI
{
    partial class FormAddPerson
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
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            dateTimePicker1 = new DateTimePicker();
            labelName = new Label();
            labelSurname = new Label();
            labelBirthDay = new Label();
            buttonAdd = new Button();
            SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 32);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(200, 23);
            textBox1.TabIndex = 0;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(13, 95);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(199, 23);
            textBox2.TabIndex = 1;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 161);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 2;
            // 
            // labelName
            // 
            labelName.AutoSize = true;
            labelName.Location = new Point(13, 14);
            labelName.Name = "labelName";
            labelName.Size = new Size(38, 15);
            labelName.TabIndex = 3;
            labelName.Text = "Meno";
            // 
            // labelSurname
            // 
            labelSurname.AutoSize = true;
            labelSurname.Location = new Point(15, 77);
            labelSurname.Name = "labelSurname";
            labelSurname.Size = new Size(59, 15);
            labelSurname.TabIndex = 4;
            labelSurname.Text = "Priezvisko";
            // 
            // labelBirthDay
            // 
            labelBirthDay.AutoSize = true;
            labelBirthDay.Location = new Point(12, 143);
            labelBirthDay.Name = "labelBirthDay";
            labelBirthDay.Size = new Size(99, 15);
            labelBirthDay.TabIndex = 5;
            labelBirthDay.Text = "Datum narodenia";
            // 
            // buttonAdd
            // 
            buttonAdd.Location = new Point(146, 215);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(75, 23);
            buttonAdd.TabIndex = 6;
            buttonAdd.Text = "Pridaj";
            buttonAdd.UseVisualStyleBackColor = true;
            // 
            // FormAddPerson
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(233, 260);
            Controls.Add(buttonAdd);
            Controls.Add(labelBirthDay);
            Controls.Add(labelSurname);
            Controls.Add(labelName);
            Controls.Add(dateTimePicker1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Name = "FormAddPerson";
            Text = "FormAddPerson";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBox1;
        private TextBox textBox2;
        private DateTimePicker dateTimePicker1;
        private Label labelName;
        private Label labelSurname;
        private Label labelBirthDay;
        private Button buttonAdd;
    }
}