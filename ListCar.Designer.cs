namespace Project3
{
    partial class ListCar
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
            this.components = new System.ComponentModel.Container();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.MenuB = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.AddModelB = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.MenuBox = new System.Windows.Forms.GroupBox();
            this.onServer = new System.Windows.Forms.Button();
            this.ExitB = new System.Windows.Forms.Button();
            this.Menu = new System.Windows.Forms.Label();
            this.ShowMenuB = new System.Windows.Forms.Button();
            this.LoadB = new System.Windows.Forms.Button();
            this.SaveB = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.MenuBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.BackgroundColor = System.Drawing.Color.CornflowerBlue;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(0, 55);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(800, 392);
            this.dataGridView1.TabIndex = 0;
            // 
            // MenuB
            // 
            this.MenuB.BackColor = System.Drawing.Color.OliveDrab;
            this.MenuB.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MenuB.Location = new System.Drawing.Point(676, 18);
            this.MenuB.Name = "MenuB";
            this.MenuB.Size = new System.Drawing.Size(118, 32);
            this.MenuB.TabIndex = 1;
            this.MenuB.Text = "Меню";
            this.MenuB.UseVisualStyleBackColor = false;
            this.MenuB.Click += new System.EventHandler(this.MenuB_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.ForeColor = System.Drawing.SystemColors.MenuBar;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Список автомобилей";
            // 
            // AddModelB
            // 
            this.AddModelB.BackColor = System.Drawing.Color.MediumPurple;
            this.AddModelB.Location = new System.Drawing.Point(532, 19);
            this.AddModelB.Name = "AddModelB";
            this.AddModelB.Size = new System.Drawing.Size(118, 32);
            this.AddModelB.TabIndex = 3;
            this.AddModelB.Text = "Добавить марку";
            this.AddModelB.UseVisualStyleBackColor = false;
            this.AddModelB.Click += new System.EventHandler(this.AddModelB_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.pictureBox1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.MenuB);
            this.groupBox1.Controls.Add(this.AddModelB);
            this.groupBox1.Location = new System.Drawing.Point(0, -2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(800, 61);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label2.Location = new System.Drawing.Point(442, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 19);
            this.label2.TabIndex = 5;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(419, 27);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(23, 23);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // MenuBox
            // 
            this.MenuBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.MenuBox.Controls.Add(this.onServer);
            this.MenuBox.Controls.Add(this.ExitB);
            this.MenuBox.Controls.Add(this.Menu);
            this.MenuBox.Controls.Add(this.ShowMenuB);
            this.MenuBox.Controls.Add(this.LoadB);
            this.MenuBox.Controls.Add(this.SaveB);
            this.MenuBox.Location = new System.Drawing.Point(568, 72);
            this.MenuBox.Margin = new System.Windows.Forms.Padding(0);
            this.MenuBox.Name = "MenuBox";
            this.MenuBox.Padding = new System.Windows.Forms.Padding(0);
            this.MenuBox.Size = new System.Drawing.Size(223, 290);
            this.MenuBox.TabIndex = 6;
            this.MenuBox.TabStop = false;
            this.MenuBox.Visible = false;
            // 
            // onServer
            // 
            this.onServer.BackColor = System.Drawing.Color.MediumPurple;
            this.onServer.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.onServer.Location = new System.Drawing.Point(18, 172);
            this.onServer.Name = "onServer";
            this.onServer.Size = new System.Drawing.Size(189, 44);
            this.onServer.TabIndex = 10;
            this.onServer.Text = "Включить сервер";
            this.onServer.UseVisualStyleBackColor = false;
            this.onServer.Click += new System.EventHandler(this.onServer_Click);
            // 
            // ExitB
            // 
            this.ExitB.BackColor = System.Drawing.Color.Crimson;
            this.ExitB.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ExitB.Location = new System.Drawing.Point(18, 200);
            this.ExitB.Name = "ExitB";
            this.ExitB.Size = new System.Drawing.Size(189, 42);
            this.ExitB.TabIndex = 9;
            this.ExitB.Text = "Выход";
            this.ExitB.UseVisualStyleBackColor = false;
            this.ExitB.Visible = false;
            this.ExitB.Click += new System.EventHandler(this.ExitB_Click);
            // 
            // Menu
            // 
            this.Menu.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.Menu.AutoSize = true;
            this.Menu.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Menu.ForeColor = System.Drawing.Color.Crimson;
            this.Menu.Location = new System.Drawing.Point(28, 37);
            this.Menu.Name = "Menu";
            this.Menu.Size = new System.Drawing.Size(175, 31);
            this.Menu.TabIndex = 6;
            this.Menu.Text = "Главное меню";
            this.Menu.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ShowMenuB
            // 
            this.ShowMenuB.BackColor = System.Drawing.Color.OliveDrab;
            this.ShowMenuB.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ShowMenuB.Location = new System.Drawing.Point(18, 108);
            this.ShowMenuB.Name = "ShowMenuB";
            this.ShowMenuB.Size = new System.Drawing.Size(189, 44);
            this.ShowMenuB.TabIndex = 5;
            this.ShowMenuB.Text = "Файл";
            this.ShowMenuB.UseVisualStyleBackColor = false;
            this.ShowMenuB.Click += new System.EventHandler(this.ShowMenuB_Click);
            // 
            // LoadB
            // 
            this.LoadB.BackColor = System.Drawing.Color.OliveDrab;
            this.LoadB.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoadB.Location = new System.Drawing.Point(18, 124);
            this.LoadB.Name = "LoadB";
            this.LoadB.Size = new System.Drawing.Size(189, 42);
            this.LoadB.TabIndex = 8;
            this.LoadB.Text = "Загрузить";
            this.LoadB.UseVisualStyleBackColor = false;
            this.LoadB.Visible = false;
            this.LoadB.Click += new System.EventHandler(this.LoadB_Click);
            // 
            // SaveB
            // 
            this.SaveB.BackColor = System.Drawing.Color.CornflowerBlue;
            this.SaveB.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SaveB.Location = new System.Drawing.Point(18, 49);
            this.SaveB.Name = "SaveB";
            this.SaveB.Size = new System.Drawing.Size(189, 42);
            this.SaveB.TabIndex = 7;
            this.SaveB.Text = "Сохранить список марок";
            this.SaveB.UseVisualStyleBackColor = false;
            this.SaveB.Visible = false;
            this.SaveB.Click += new System.EventHandler(this.SaveB_Click);
            // 
            // ListCar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.CornflowerBlue;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.MenuBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "ListCar";
            this.Text = "ListCar";
            this.Load += new System.EventHandler(this.ListCar_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.MenuBox.ResumeLayout(false);
            this.MenuBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button MenuB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button AddModelB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox MenuBox;
        private System.Windows.Forms.Button ExitB;
        private System.Windows.Forms.Button LoadB;
        private System.Windows.Forms.Button SaveB;
        private System.Windows.Forms.Label Menu;
        private System.Windows.Forms.Button ShowMenuB;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Button onServer;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}