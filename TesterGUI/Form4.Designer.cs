namespace TesterGUI
{
    partial class Form4
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
            buttonCreate = new Button();
            buttonLoad = new Button();
            buttonRandomOperations = new Button();
            textBoxNumbers = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(455, 426);
            panel1.TabIndex = 0;
            // 
            // buttonCreate
            // 
            buttonCreate.Location = new Point(548, 95);
            buttonCreate.Name = "buttonCreate";
            buttonCreate.Size = new Size(136, 23);
            buttonCreate.TabIndex = 1;
            buttonCreate.Text = "Create Lin Hash";
            buttonCreate.UseVisualStyleBackColor = true;
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(548, 124);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(136, 23);
            buttonLoad.TabIndex = 2;
            buttonLoad.Text = "Load Lin Hash";
            buttonLoad.UseVisualStyleBackColor = true;
            // 
            // buttonRandomOperations
            // 
            buttonRandomOperations.Location = new Point(548, 153);
            buttonRandomOperations.Name = "buttonRandomOperations";
            buttonRandomOperations.Size = new Size(136, 23);
            buttonRandomOperations.TabIndex = 3;
            buttonRandomOperations.Text = "Random Operations";
            buttonRandomOperations.UseVisualStyleBackColor = true;
            // 
            // textBoxNumbers
            // 
            textBoxNumbers.Location = new Point(548, 182);
            textBoxNumbers.Name = "textBoxNumbers";
            textBoxNumbers.Size = new Size(136, 23);
            textBoxNumbers.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 473);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 5;
            label1.Text = "label1";
            label1.Click += label1_Click;
            // 
            // Form4
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(728, 539);
            Controls.Add(label1);
            Controls.Add(textBoxNumbers);
            Controls.Add(buttonRandomOperations);
            Controls.Add(buttonLoad);
            Controls.Add(buttonCreate);
            Controls.Add(panel1);
            Name = "Form4";
            Text = "Form4";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panel1;
        private Button buttonCreate;
        private Button buttonLoad;
        private Button buttonRandomOperations;
        private TextBox textBoxNumbers;
        private Label label1;
    }
}