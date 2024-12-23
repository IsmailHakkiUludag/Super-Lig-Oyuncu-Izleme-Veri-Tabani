namespace superlig
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnEkle;
        private System.Windows.Forms.Button btnDuzenle;
        private System.Windows.Forms.Button btnSil;
        private System.Windows.Forms.Button btnArama;
        private System.Windows.Forms.TextBox txtAra;
        private System.Windows.Forms.DataGridView dataGrid;

        /// <summary>
        /// Kullanılan kaynakları temizlemek için override metod.
        /// </summary>
        /// <param name="disposing">Yönetilen kaynakları serbest bırakmak için true, aksi takdirde false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Formun bileşenlerini başlatmak için gerekli kodlar.
        /// </summary>
        private void InitializeComponent()
        {
            btnEkle = new Button();
            btnDuzenle = new Button();
            btnSil = new Button();
            txtAra = new TextBox();
            btnArama = new Button();
            dataGrid = new DataGridView();
            cmbMenu = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            SuspendLayout();
            // 
            // btnEkle
            // 
            btnEkle.Location = new Point(378, 395);
            btnEkle.Name = "btnEkle";
            btnEkle.Size = new Size(120, 40);
            btnEkle.TabIndex = 6;
            btnEkle.Text = "Ekle";
            // 
            // btnDuzenle
            // 
            btnDuzenle.Location = new Point(630, 395);
            btnDuzenle.Name = "btnDuzenle";
            btnDuzenle.Size = new Size(120, 40);
            btnDuzenle.TabIndex = 7;
            btnDuzenle.Text = "Düzenle";
            // 
            // btnSil
            // 
            btnSil.Location = new Point(504, 395);
            btnSil.Name = "btnSil";
            btnSil.Size = new Size(120, 40);
            btnSil.TabIndex = 8;
            btnSil.Text = "Sil";
            // 
            // txtAra
            // 
            txtAra.Location = new Point(115, 14);
            txtAra.Name = "txtAra";
            txtAra.Size = new Size(300, 27);
            txtAra.TabIndex = 9;
            // 
            // btnArama
            // 
            btnArama.Location = new Point(421, 14);
            btnArama.Name = "btnArama";
            btnArama.Size = new Size(100, 30);
            btnArama.TabIndex = 10;
            btnArama.Text = "Ara";
            // 
            // dataGrid
            // 
            dataGrid.ColumnHeadersHeight = 29;
            dataGrid.Location = new Point(50, 50);
            dataGrid.Name = "dataGrid";
            dataGrid.RowHeadersWidth = 51;
            dataGrid.Size = new Size(700, 330);
            dataGrid.TabIndex = 11;
            // 
            // cmbMenu
            // 
            cmbMenu.FormattingEnabled = true;
            cmbMenu.Location = new Point(527, 16);
            cmbMenu.Name = "cmbMenu";
            cmbMenu.Size = new Size(151, 28);
            cmbMenu.TabIndex = 14;
            // 
            // Form1
            // 
            ClientSize = new Size(960, 520);
            Controls.Add(cmbMenu);
            Controls.Add(btnEkle);
            Controls.Add(btnDuzenle);
            Controls.Add(btnSil);
            Controls.Add(txtAra);
            Controls.Add(btnArama);
            Controls.Add(dataGrid);
            Name = "Form1";
            Text = "Süper Lig Yönetim Sistemi";
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
        private ComboBox cmbMenu;
    }
}
