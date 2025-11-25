namespace TesterGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panelHeap = new Panel();
            label1 = new Label();
            buttonAddRecord = new Button();
            buttonRemoveRecord = new Button();
            buttonGetRecord = new Button();
            buttonRandomOperations = new Button();
            labelInfo = new Label();
            buttonLoad = new Button();
            label3 = new Label();
            label4 = new Label();
            buttonCreate = new Button();
            SuspendLayout();
            // 
            // panelHeap
            // 
            panelHeap.AutoScroll = true;
            panelHeap.BackColor = Color.White;
            panelHeap.Location = new Point(12, 35);
            panelHeap.Name = "panelHeap";
            panelHeap.Size = new Size(518, 403);
            panelHeap.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(53, 15);
            label1.TabIndex = 0;
            label1.Text = "HeapFile";
            // 
            // buttonAddRecord
            // 
            buttonAddRecord.Location = new Point(560, 99);
            buttonAddRecord.Name = "buttonAddRecord";
            buttonAddRecord.Size = new Size(126, 23);
            buttonAddRecord.TabIndex = 3;
            buttonAddRecord.Text = "Add Record";
            buttonAddRecord.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveRecord
            // 
            buttonRemoveRecord.Location = new Point(560, 128);
            buttonRemoveRecord.Name = "buttonRemoveRecord";
            buttonRemoveRecord.Size = new Size(126, 23);
            buttonRemoveRecord.TabIndex = 4;
            buttonRemoveRecord.Text = "Remove Record";
            buttonRemoveRecord.UseVisualStyleBackColor = true;
            // 
            // buttonGetRecord
            // 
            buttonGetRecord.Location = new Point(560, 157);
            buttonGetRecord.Name = "buttonGetRecord";
            buttonGetRecord.Size = new Size(126, 23);
            buttonGetRecord.TabIndex = 5;
            buttonGetRecord.Text = "Get Record";
            buttonGetRecord.UseVisualStyleBackColor = true;
            // 
            // buttonRandomOperations
            // 
            buttonRandomOperations.Location = new Point(560, 186);
            buttonRandomOperations.Name = "buttonRandomOperations";
            buttonRandomOperations.Size = new Size(126, 23);
            buttonRandomOperations.TabIndex = 6;
            buttonRandomOperations.Text = "Random Operations";
            buttonRandomOperations.UseVisualStyleBackColor = true;
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(558, 249);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(28, 15);
            labelInfo.TabIndex = 7;
            labelInfo.Text = "Text";
            // 
            // buttonLoad
            // 
            buttonLoad.Location = new Point(560, 66);
            buttonLoad.Name = "buttonLoad";
            buttonLoad.Size = new Size(126, 23);
            buttonLoad.TabIndex = 8;
            buttonLoad.Text = "Load File";
            buttonLoad.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(560, 281);
            label3.Name = "label3";
            label3.Size = new Size(38, 15);
            label3.TabIndex = 9;
            label3.Text = "label3";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(560, 305);
            label4.Name = "label4";
            label4.Size = new Size(38, 15);
            label4.TabIndex = 10;
            label4.Text = "label4";
            // 
            // buttonCreate
            // 
            buttonCreate.Location = new Point(560, 35);
            buttonCreate.Name = "buttonCreate";
            buttonCreate.Size = new Size(126, 23);
            buttonCreate.TabIndex = 11;
            buttonCreate.Text = "Create File";
            buttonCreate.UseVisualStyleBackColor = true;
            buttonCreate.Click += buttonCreate_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(899, 450);
            Controls.Add(buttonCreate);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(buttonLoad);
            Controls.Add(labelInfo);
            Controls.Add(buttonRandomOperations);
            Controls.Add(buttonGetRecord);
            Controls.Add(buttonRemoveRecord);
            Controls.Add(buttonAddRecord);
            Controls.Add(label1);
            Controls.Add(panelHeap);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel panelHeap;
        private Label label1;
        private Button buttonAddRecord;
        private Button buttonRemoveRecord;
        private Button buttonGetRecord;
        private Button buttonRandomOperations;
        private Label labelInfo;
        private Button buttonLoad;
        private Label label3;
        private Label label4;
        private Button buttonCreate;
    }
}
