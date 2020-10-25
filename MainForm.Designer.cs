namespace TurbulenceModeling
{
    partial class MainForm
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
            this._pctbMain = new System.Windows.Forms.PictureBox();
            this._menu = new System.Windows.Forms.MenuStrip();
            this.жизненныйЦиклToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this._pctbMain)).BeginInit();
            this._menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // _pctbMain
            // 
            this._pctbMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._pctbMain.Location = new System.Drawing.Point(12, 27);
            this._pctbMain.Name = "_pctbMain";
            this._pctbMain.Size = new System.Drawing.Size(1019, 515);
            this._pctbMain.TabIndex = 0;
            this._pctbMain.TabStop = false;
            // 
            // _menu
            // 
            this._menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.жизненныйЦиклToolStripMenuItem});
            this._menu.Location = new System.Drawing.Point(0, 0);
            this._menu.Name = "_menu";
            this._menu.Size = new System.Drawing.Size(1043, 24);
            this._menu.TabIndex = 1;
            this._menu.Text = "menuStrip1";
            // 
            // жизненныйЦиклToolStripMenuItem
            // 
            this.жизненныйЦиклToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StartToolStripMenuItem,
            this.StopToolStripMenuItem});
            this.жизненныйЦиклToolStripMenuItem.Name = "жизненныйЦиклToolStripMenuItem";
            this.жизненныйЦиклToolStripMenuItem.Size = new System.Drawing.Size(115, 20);
            this.жизненныйЦиклToolStripMenuItem.Text = "Жизненный цикл";
            // 
            // StartToolStripMenuItem
            // 
            this.StartToolStripMenuItem.Name = "StartToolStripMenuItem";
            this.StartToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.StartToolStripMenuItem.Text = "Запуск";
            this.StartToolStripMenuItem.Click += new System.EventHandler(this.StartToolStripMenuItem_Click);
            // 
            // StopToolStripMenuItem
            // 
            this.StopToolStripMenuItem.Name = "StopToolStripMenuItem";
            this.StopToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.StopToolStripMenuItem.Text = "Стоп";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1043, 554);
            this.Controls.Add(this._pctbMain);
            this.Controls.Add(this._menu);
            this.MainMenuStrip = this._menu;
            this.Name = "MainForm";
            this.Text = "Моделирование турбулентности";
            ((System.ComponentModel.ISupportInitialize)(this._pctbMain)).EndInit();
            this._menu.ResumeLayout(false);
            this._menu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox _pctbMain;
        private System.Windows.Forms.MenuStrip _menu;
        private System.Windows.Forms.ToolStripMenuItem жизненныйЦиклToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem StopToolStripMenuItem;
    }
}

