namespace TesterGUI
{
    partial class FormApp
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
            panel1 = new Panel();
            menuStrip1 = new MenuStrip();
            dataToolStripMenuItem = new ToolStripMenuItem();
            createFilesToolStripMenuItem = new ToolStripMenuItem();
            loadFilesToolStripMenuItem = new ToolStripMenuItem();
            textBoxPatientMain = new TextBox();
            textBoxPatientOverflow = new TextBox();
            textBoxPCRMain = new TextBox();
            textBoxPCROverflow = new TextBox();
            panel2 = new Panel();
            label3 = new Label();
            label4 = new Label();
            panel3 = new Panel();
            label2 = new Label();
            label1 = new Label();
            labelPatient = new Label();
            labelPCR = new Label();
            buttonInsertTest = new Button();
            closeFilesToolStripMenuItem = new ToolStripMenuItem();
            buttonGetPatient = new Button();
            buttonGetTest = new Button();
            buttonInsertPatient = new Button();
            buttonEditPatient = new Button();
            buttonEditTest = new Button();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            menuStrip1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Location = new Point(12, 39);
            panel1.Name = "panel1";
            panel1.Size = new Size(473, 399);
            panel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { dataToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(985, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // dataToolStripMenuItem
            // 
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { createFilesToolStripMenuItem, loadFilesToolStripMenuItem, closeFilesToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(43, 20);
            dataToolStripMenuItem.Text = "Data";
            // 
            // createFilesToolStripMenuItem
            // 
            createFilesToolStripMenuItem.Name = "createFilesToolStripMenuItem";
            createFilesToolStripMenuItem.Size = new Size(180, 22);
            createFilesToolStripMenuItem.Text = "Create Files";
            // 
            // loadFilesToolStripMenuItem
            // 
            loadFilesToolStripMenuItem.Name = "loadFilesToolStripMenuItem";
            loadFilesToolStripMenuItem.Size = new Size(180, 22);
            loadFilesToolStripMenuItem.Text = "Load Files";
            // 
            // textBoxPatientMain
            // 
            textBoxPatientMain.Location = new Point(3, 29);
            textBoxPatientMain.Name = "textBoxPatientMain";
            textBoxPatientMain.Size = new Size(210, 23);
            textBoxPatientMain.TabIndex = 2;
            // 
            // textBoxPatientOverflow
            // 
            textBoxPatientOverflow.Location = new Point(3, 88);
            textBoxPatientOverflow.Name = "textBoxPatientOverflow";
            textBoxPatientOverflow.Size = new Size(210, 23);
            textBoxPatientOverflow.TabIndex = 3;
            // 
            // textBoxPCRMain
            // 
            textBoxPCRMain.Location = new Point(3, 29);
            textBoxPCRMain.Name = "textBoxPCRMain";
            textBoxPCRMain.Size = new Size(210, 23);
            textBoxPCRMain.TabIndex = 4;
            // 
            // textBoxPCROverflow
            // 
            textBoxPCROverflow.Location = new Point(3, 87);
            textBoxPCROverflow.Name = "textBoxPCROverflow";
            textBoxPCROverflow.Size = new Size(210, 23);
            textBoxPCROverflow.TabIndex = 5;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ControlDark;
            panel2.Controls.Add(label3);
            panel2.Controls.Add(textBoxPCRMain);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(textBoxPCROverflow);
            panel2.Location = new Point(744, 61);
            panel2.Name = "panel2";
            panel2.Size = new Size(216, 139);
            panel2.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(15, 70);
            label3.Name = "label3";
            label3.Size = new Size(180, 15);
            label3.TabIndex = 13;
            label3.Text = "Velkost blocku v overflow subore";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(14, 11);
            label4.Name = "label4";
            label4.Size = new Size(181, 15);
            label4.TabIndex = 12;
            label4.Text = "Velkost blocku v hlavnom subore";
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ControlDark;
            panel3.Controls.Add(label2);
            panel3.Controls.Add(textBoxPatientMain);
            panel3.Controls.Add(label1);
            panel3.Controls.Add(textBoxPatientOverflow);
            panel3.Location = new Point(503, 61);
            panel3.Name = "panel3";
            panel3.Size = new Size(216, 139);
            panel3.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(17, 70);
            label2.Name = "label2";
            label2.Size = new Size(180, 15);
            label2.TabIndex = 11;
            label2.Text = "Velkost blocku v overflow subore";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 11);
            label1.Name = "label1";
            label1.Size = new Size(181, 15);
            label1.TabIndex = 10;
            label1.Text = "Velkost blocku v hlavnom subore";
            // 
            // labelPatient
            // 
            labelPatient.AutoSize = true;
            labelPatient.Location = new Point(503, 37);
            labelPatient.Name = "labelPatient";
            labelPatient.Size = new Size(120, 15);
            labelPatient.TabIndex = 8;
            labelPatient.Text = "Nastavenia pacientov";
            // 
            // labelPCR
            // 
            labelPCR.AutoSize = true;
            labelPCR.Location = new Point(744, 43);
            labelPCR.Name = "labelPCR";
            labelPCR.Size = new Size(125, 15);
            labelPCR.TabIndex = 9;
            labelPCR.Text = "Nastavenia PCR testov";
            // 
            // buttonInsertTest
            // 
            buttonInsertTest.Location = new Point(520, 248);
            buttonInsertTest.Name = "buttonInsertTest";
            buttonInsertTest.Size = new Size(75, 23);
            buttonInsertTest.TabIndex = 10;
            buttonInsertTest.Text = "PCR test";
            buttonInsertTest.UseVisualStyleBackColor = true;
            // 
            // closeFilesToolStripMenuItem
            // 
            closeFilesToolStripMenuItem.Name = "closeFilesToolStripMenuItem";
            closeFilesToolStripMenuItem.Size = new Size(180, 22);
            closeFilesToolStripMenuItem.Text = "Close Files";
            // 
            // buttonGetPatient
            // 
            buttonGetPatient.Location = new Point(519, 304);
            buttonGetPatient.Name = "buttonGetPatient";
            buttonGetPatient.Size = new Size(75, 23);
            buttonGetPatient.TabIndex = 11;
            buttonGetPatient.Text = "Osoba";
            buttonGetPatient.UseVisualStyleBackColor = true;
            // 
            // buttonGetTest
            // 
            buttonGetTest.Location = new Point(520, 361);
            buttonGetTest.Name = "buttonGetTest";
            buttonGetTest.Size = new Size(75, 23);
            buttonGetTest.TabIndex = 12;
            buttonGetTest.Text = "PCR Test";
            buttonGetTest.UseVisualStyleBackColor = true;
            // 
            // buttonInsertPatient
            // 
            buttonInsertPatient.Location = new Point(681, 248);
            buttonInsertPatient.Name = "buttonInsertPatient";
            buttonInsertPatient.Size = new Size(75, 23);
            buttonInsertPatient.TabIndex = 13;
            buttonInsertPatient.Text = "Osoba";
            buttonInsertPatient.UseVisualStyleBackColor = true;
            // 
            // buttonEditPatient
            // 
            buttonEditPatient.Location = new Point(681, 304);
            buttonEditPatient.Name = "buttonEditPatient";
            buttonEditPatient.Size = new Size(75, 23);
            buttonEditPatient.TabIndex = 14;
            buttonEditPatient.Text = "Osoba";
            buttonEditPatient.UseVisualStyleBackColor = true;
            // 
            // buttonEditTest
            // 
            buttonEditTest.Location = new Point(681, 361);
            buttonEditTest.Name = "buttonEditTest";
            buttonEditTest.Size = new Size(75, 23);
            buttonEditTest.TabIndex = 15;
            buttonEditTest.Text = "PCR Test";
            buttonEditTest.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(523, 225);
            label5.Name = "label5";
            label5.Size = new Size(154, 15);
            label5.TabIndex = 16;
            label5.Text = "Vlozenie vysledku PCR testu";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(524, 284);
            label6.Name = "label6";
            label6.Size = new Size(100, 15);
            label6.TabIndex = 17;
            label6.Text = "Vyhladanie osoby";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(525, 341);
            label7.Name = "label7";
            label7.Size = new Size(119, 15);
            label7.TabIndex = 18;
            label7.Text = "Vyhladanie PCR testu";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(693, 225);
            label8.Name = "label8";
            label8.Size = new Size(86, 15);
            label8.TabIndex = 19;
            label8.Text = "Vlozenie osoby";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(685, 283);
            label9.Name = "label9";
            label9.Size = new Size(83, 15);
            label9.TabIndex = 20;
            label9.Text = "Editacia osoby";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(684, 341);
            label10.Name = "label10";
            label10.Size = new Size(102, 15);
            label10.TabIndex = 21;
            label10.Text = "Editacia PCR testu";
            // 
            // FormApp
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(985, 477);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(buttonEditTest);
            Controls.Add(buttonEditPatient);
            Controls.Add(buttonInsertPatient);
            Controls.Add(buttonGetTest);
            Controls.Add(buttonGetPatient);
            Controls.Add(buttonInsertTest);
            Controls.Add(labelPCR);
            Controls.Add(labelPatient);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "FormApp";
            Text = "FormApp";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem dataToolStripMenuItem;
        private ToolStripMenuItem createFilesToolStripMenuItem;
        private ToolStripMenuItem loadFilesToolStripMenuItem;
        private TextBox textBoxPatientMain;
        private TextBox textBoxPatientOverflow;
        private TextBox textBoxPCRMain;
        private TextBox textBoxPCROverflow;
        private Panel panel2;
        private Panel panel3;
        private Label labelPatient;
        private Label labelPCR;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private ToolStripMenuItem closeFilesToolStripMenuItem;
        private Button buttonInsertTest;
        private Button buttonGetPatient;
        private Button buttonGetTest;
        private Button buttonInsertPatient;
        private Button buttonEditPatient;
        private Button buttonEditTest;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
    }
}