using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Npgsql;

namespace superlig
{
    public partial class Form1 : Form
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=0416;Database=superlıg";

            private Dictionary<string, string[]> tableColumns = new Dictionary<string, string[]>
    {
        { "Oyuncu", new[] { "Adı", "Soyadı", "Pozisyon", "Takım" } },
        { "Takim", new[] { "takim_id", "takim_adi",  "kısaltma", "sehir","kurulus_tarihi" } },
        { "TeknikDirektor", new[] { "tdadı", "takım" } },
        { "Hakem", new[] { "hakemadı",  "tecrübeyılı" } },
        { "Baskan", new[] { "baskanadı", "takım" } },
        { "Stadyum", new[] { "Ad", "kapasite", "takım" } },
        { "Taraftar Grubu", new[] { "grupadı", "takımadı" } },
        { "Kisi", new[] { "kisi_id","Ad","Soyad","dogum_tarihi" } },
        { "Goller", new[] { "oyuncu", "maçtarihi", "takım" } },
        { "Ceza", new[] { "oyuncuadı", "maçtarihi", "cezasüresi" } },
        { "Transferler", new[] { "oyuncu", "eskitakım", "yenitakım" } },
        { "İstatistik", new[] { "oyuncuadı", "goller", "asistler", "kartlar" } },
        { "Kart", new[] { "oyuncu", "karttürü", "maçtarihi" } }
     };

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            btnEkle.Click += (s, e) => AddRecord();
            btnDuzenle.Click += (s, e) => EditRecord();
            btnSil.Click += (s, e) => DeleteRecord();
            btnArama.Click += BtnAra_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGrid.DataSource = null;
            cmbMenu.Items.AddRange(tableColumns.Keys.ToArray());
        }

        private void ExecuteQueryAndBindData(string query, params NpgsqlParameter[] parameters)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    foreach (var parameter in parameters)
                    {
                        cmd.Parameters.Add(parameter);
                    }

                    var dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());
                    dataGrid.DataSource = dataTable;
                }
            }
        }

        private void BtnAra_Click(object sender, EventArgs e)
        {
            string keyword = txtAra.Text.Trim();
            string selectedTable = cmbMenu.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedTable))
            {
                MessageBox.Show("Lütfen bir tablo seçin.");
                return;
            }

            string query = "";
            NpgsqlParameter keywordParam = new NpgsqlParameter("@keyword", "%" + keyword + "%");

            switch (selectedTable)
            {
                case "Oyuncu":
                    query = @"SELECT 
                        o.oyuncu_id AS OyuncuID,
                        k.ad AS Adı,
                        k.soyad AS Soyadı,
                        p.pozisyon_adi AS Pozisyon,
                        t.takim_adi AS Takım
                    FROM oyuncu o
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    JOIN pozisyon p ON o.pozisyon_id = p.pozisyon_id
                    JOIN takim t ON o.takim_id = t.takim_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "TeknikDirektor":
                    query = @"SELECT 
                        td.td_id AS TDID,
                        CONCAT(k.ad, ' ', k.soyad) AS TDAdı,
                        t.takim_adi AS Takım
                    FROM teknik_direktor td
                    JOIN kisi k ON td.kisi_id = k.kisi_id
                    JOIN takim t ON td.takim_id = t.takim_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Hakem":
                    query = @"SELECT 
                        h.hakem_id AS HakemID,
                        CONCAT(k.ad, ' ', k.soyad) AS HakemAdı,
                        h.tecrube_yili AS TecrübeYılı
                    FROM hakem h
                    JOIN kisi k ON h.kisi_id = k.kisi_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Baskan":
                    query = @"SELECT 
                        b.baskan_id AS BaskanID,
                        CONCAT(k.ad, ' ', k.soyad) AS BaskanAdı,
                        t.takim_adi AS Takım
                    FROM baskan b
                    JOIN kisi k ON b.kisi_id = k.kisi_id
                    JOIN takim t ON b.takim_id = t.takim_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Takim":
                    query = @"
            SELECT 
                takim.takim_id AS takim_id,
                takim.takim_adi AS takim_adi,
                takim.kurulus_tarihi AS kurulus_tarihi,
                takim.kısaltma AS kısaltma,
                takim.sehir AS sehir
            FROM takim
            LEFT JOIN kisi AS baskan ON takim.baskan_id = baskan.kisi_id
            LEFT JOIN kisi AS td ON takim.teknik_direktor_id = td.kisi_id;";
                    break;
                case "Taraftar Grubu":
                    query = @"
            SELECT 
                tg.grup_id AS GrupID,
                tg.grup_adi AS GrupAdı,
                t.takim_adi AS TakımAdı
            FROM taraftar_grubu tg
            JOIN takim t ON tg.takim_id = t.takim_id
            WHERE tg.grup_adi ILIKE @keyword;";
                    break;

                case "Kisi":
                    query = @"
            SELECT 
                k.kisi_id AS kisi_id,
                k.Ad AS Ad,
                k.soyad AS Soyad,
                k.dogum_tarihi AS dogum_tarihi
            FROM kisi k
            WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;

                case "Stadyum":
                    query = @"SELECT 
                        s.stadyum_id AS StadyumID,
                        s.ad AS Ad,
                        s.kapasite AS Kapasite,
                        t.takim_adi AS Takım
                    FROM stadyum s
                    JOIN takim t ON s.takim_id = t.takim_id
                    WHERE s.ad ILIKE @keyword OR s.sehir ILIKE @keyword;";
                    break;
                case "Goller":
                    query = @"SELECT 
                        g.gol_id AS GolID,
                        CONCAT(k.ad, ' ', k.soyad) AS Oyuncu,
                        m.mac_tarihi AS MaçTarihi,
                        t.takim_adi AS Takım
                    FROM gol g
                    JOIN oyuncu o ON g.oyuncu_id = o.oyuncu_id
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    JOIN mac m ON g.mac_id = m.mac_id
                    JOIN takim t ON o.takim_id = t.takim_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Ceza":
                    query = @"SELECT 
                        c.ceza_id AS CezaID,
                        CONCAT(k.ad, ' ', k.soyad) AS OyuncuAdı,
                        m.mac_tarihi AS MaçTarihi,
                        c.ceza_suresi AS CezaSüresi
                    FROM ceza c
                    JOIN oyuncu o ON c.oyuncu_id = o.oyuncu_id
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    JOIN mac m ON c.mac_id = m.mac_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Transferler":
                    query = @"
            SELECT 
                tr.transfer_id AS TransferID,
                CONCAT(k.ad, ' ', k.soyad) AS Oyuncu,
                t1.takim_adi AS EskiTakım,
                t2.takim_adi AS YeniTakım
            FROM transfer tr
            JOIN oyuncu o ON tr.oyuncu_id = o.oyuncu_id
            JOIN kisi k ON o.kisi_id = k.kisi_id
            JOIN takim t1 ON tr.eski_takim_id = t1.takim_id
            JOIN takim t2 ON tr.yeni_takim_id = t2.takim_id
            WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "İstatistik":
                    query = @"SELECT 
                        i.istatistik_id AS İstatistikID,
                        CONCAT(k.ad, ' ', k.soyad) AS OyuncuAdı,
                        i.gol_sayisi AS Goller,
                        i.asist_sayisi AS Asistler,
                        i.kart_sayisi AS Kartlar
                    FROM oyuncu_istatistik i
                    JOIN oyuncu o ON i.oyuncu_id = o.oyuncu_id
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                case "Kart":
                    query = @"SELECT 
                        ka.kart_id AS KartID,
                        CONCAT(k.ad, ' ', k.soyad) AS Oyuncu,
                        ka.kart_turu AS KartTürü,
                        m.mac_tarihi AS MaçTarihi
                    FROM kart ka
                    JOIN oyuncu o ON ka.oyuncu_id = o.oyuncu_id
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    JOIN mac m ON ka.mac_id = m.mac_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";
                    break;
                default:
                    MessageBox.Show("Bu tablo için arama yapılamıyor.");
                    return;
            }

            ExecuteQueryAndBindData(query, keywordParam);
        }

                private void AddRecord()
        {
            string selectedTable = cmbMenu.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable) || !tableColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Lütfen geçerli bir tablo seçin.");
                return;
            }

            var addForm = new Form { Text = $"{selectedTable} Ekle", Size = new Size(400, 300) };
            var inputs = new Dictionary<string, TextBox>();
            var columns = tableColumns[selectedTable];
            int y = 20;

            // Dinamik olarak form oluşturma
            foreach (var column in columns)
            {
                var label = new Label { Text = column, Location = new Point(20, y), Width = 100 };
                var textBox = new TextBox { Location = new Point(130, y), Width = 200 };
                inputs[column] = textBox;
                addForm.Controls.Add(label);
                addForm.Controls.Add(textBox);
                y += 40;
            }

            var btnSave = new Button { Text = "Kaydet", Location = new Point(20, y) };
            btnSave.Click += (s, e) =>
            {
                try
                {
                    // Kolon adlarını ve parametre adlarını oluştur
                    string columnsList = string.Join(", ", inputs.Keys);
                    string valuesList = string.Join(", ", inputs.Keys.Select(k => $"@{k}"));
                    string query = $"INSERT INTO public.{selectedTable.ToLower().Replace(" ", "_")} ({columnsList}) VALUES ({valuesList})";

                    using (var conn = new NpgsqlConnection(connectionString))
                    {
                        conn.Open();
                        using (var cmd = new NpgsqlCommand(query, conn))
                        {
                            foreach (var input in inputs)
                            {
                                // Hata çıkmaması için uygun tür dönüşümünü yap
                                var columnName = input.Key;
                                var inputValue = input.Value.Text;

                                if (int.TryParse(inputValue, out int intValue))
                                {
                                    cmd.Parameters.AddWithValue($"@{columnName}", intValue); // Integer türü için
                                }
                                else if (DateTime.TryParse(inputValue, out DateTime dateValue))
                                {
                                    cmd.Parameters.AddWithValue($"@{columnName}", dateValue); // DateTime türü için
                                }
                                else if (decimal.TryParse(inputValue, out decimal decimalValue))
                                {
                                    cmd.Parameters.AddWithValue($"@{columnName}", decimalValue); // Decimal türü için
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue($"@{columnName}", inputValue); // Text ya da diğer türler için
                                }
                            }

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Kayıt başarıyla eklendi.");
                    addForm.Close();
                    ReloadData(); // Verileri güncelle
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Kayıt eklenirken bir hata oluştu: {ex.Message}");
                }
            };

            addForm.Controls.Add(btnSave);
            addForm.ShowDialog();
        }


                    private void EditRecord()
        {
            string selectedTable = cmbMenu.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable) || !tableColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Lütfen geçerli bir tablo seçin.");
                return;
            }

            if (dataGrid.SelectedRows.Count > 0)
            {
                var selectedRow = dataGrid.SelectedRows[0];
                var editForm = new Form { Text = $"{selectedTable} Düzenle", Size = new Size(400, 300) };
                var inputs = new Dictionary<string, TextBox>();
                var columns = tableColumns[selectedTable];
                int id = Convert.ToInt32(selectedRow.Cells[0].Value); // ID sütununu almak

                int y = 20;

                // Dinamik olarak form elemanları oluşturuluyor
                foreach (var column in columns)
                {
                    var label = new Label { Text = column, Location = new Point(20, y), Width = 100 };
                    var textBox = new TextBox
                    {
                        Text = selectedRow.Cells[column].Value?.ToString(),
                        Location = new Point(130, y),
                        Width = 200
                    };
                    inputs[column] = textBox;
                    editForm.Controls.Add(label);
                    editForm.Controls.Add(textBox);
                    y += 40;
                }

                var btnSave = new Button { Text = "Kaydet", Location = new Point(20, y) };
                btnSave.Click += (s, e) =>
                {
                    try
                    {
                        // Güncelleme sorgusunu oluştur
                        string updateList = string.Join(", ", inputs.Keys.Select(k => $"{k} = @{k}"));
                        string query = $"UPDATE public.{selectedTable.ToLower().Replace(" ", "_")} SET {updateList} WHERE {selectedTable.ToLower()}_id = @id";

                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();
                            using (var cmd = new NpgsqlCommand(query, conn))
                            {
                                // Parametreleri ekle ve tür dönüşümü yap
                                foreach (var input in inputs)
                                {
                                    var columnName = input.Key;
                                    var inputValue = input.Value.Text;

                                    if (int.TryParse(inputValue, out int intValue))
                                    {
                                        cmd.Parameters.AddWithValue($"@{columnName}", intValue); // Integer türü
                                    }
                                    else if (DateTime.TryParse(inputValue, out DateTime dateValue))
                                    {
                                        cmd.Parameters.AddWithValue($"@{columnName}", dateValue); // DateTime türü
                                    }
                                    else if (decimal.TryParse(inputValue, out decimal decimalValue))
                                    {
                                        cmd.Parameters.AddWithValue($"@{columnName}", decimalValue); // Decimal türü
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue($"@{columnName}", inputValue); // Text veya diğer türler
                                    }
                                }

                                // ID parametresini ekle
                                cmd.Parameters.AddWithValue("@id", id);

                                // Sorguyu çalıştır
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Kayıt başarıyla güncellendi.");
                        editForm.Close();
                        ReloadData(); // Tabloyu yeniden yükle
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Kayıt güncellenirken bir hata oluştu: {ex.Message}");
                    }
                };

                editForm.Controls.Add(btnSave);
                editForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Lütfen düzenlenecek bir kayıt seçin.");
            }
        }

                            private void DeleteRecord()
        {
            string selectedTable = cmbMenu.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTable) || !tableColumns.ContainsKey(selectedTable))
            {
                MessageBox.Show("Lütfen geçerli bir tablo seçin.");
                return;
            }

            if (dataGrid.SelectedRows.Count > 0)
            {
                var selectedRow = dataGrid.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells[0].Value); // Seçilen satırdan ID'yi al

                var confirmation = MessageBox.Show(
                    "Bu kaydı ve bağlı tüm kayıtları silmek istediğinizden emin misiniz?",
                    "Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (confirmation == DialogResult.Yes)
                {
                    try
                    {
                        using (var conn = new NpgsqlConnection(connectionString))
                        {
                            conn.Open();

                            // Bağlı kayıtları sil
                            DeleteChildRecords(selectedTable, id, conn);

                            // Ana kaydı sil
                            string deleteQuery = $"DELETE FROM public.{selectedTable.ToLower().Replace(" ", "_")} WHERE {selectedTable.ToLower()}_id = @id";
                            using (var cmd = new NpgsqlCommand(deleteQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Kayıt ve bağlı tüm veriler başarıyla silindi.");
                        ReloadData(); // Tabloyu yeniden yükle
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Bir hata oluştu: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silinecek bir kayıt seçin.");
            }
        }


                private void DeleteChildRecords(string parentTable, int parentId, NpgsqlConnection parentConn)
        {
            // Bağlı tabloları belirle
            string fkQuery = $@"
                SELECT kcu.table_name, kcu.column_name, ccu.column_name AS foreign_column
                FROM information_schema.key_column_usage kcu
                JOIN information_schema.constraint_column_usage ccu 
                ON kcu.constraint_name = ccu.constraint_name
                WHERE ccu.table_name = '{parentTable.ToLower().Replace(" ", "_")}'";

            using (var cmd = new NpgsqlCommand(fkQuery, parentConn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string childTable = reader.GetString(0);
                    string childColumn = reader.GetString(1);

                    // Yeni bir bağlantı açarak çocuk kayıtları sil
                    using (var childConn = new NpgsqlConnection(connectionString))
                    {
                        childConn.Open();

                        string deleteChildQuery = $@"
                            DELETE FROM public.{childTable} 
                            WHERE {childColumn} = @parentId";

                        using (var childCmd = new NpgsqlCommand(deleteChildQuery, childConn))
                        {
                            childCmd.Parameters.AddWithValue("@parentId", parentId);
                            childCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }



        private void ReloadData()
        {
            string selectedTable = cmbMenu.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable))
            {
                switch (selectedTable)
                {
                    case "Oyuncu":
                        LoadOyuncular();
                        break;
                    case "Takim":
                        LoadTakimlar();
                        break;
                    case "TeknikDirektor":
                        LoadTeknikDirektor();
                        break;
                    case "Hakem":
                        LoadHakem();
                        break;
                    case "Baskan":
                        LoadBaskan();
                        break;
                    case "Stadyum":
                        LoadStadyum();
                        break;
                    case "Taraftar Grubu":
                        LoadTaraftarGrup();
                        break;
                    case "Kisiler":
                        LoadKisi();
                        break;
                    case "Goller":
                        LoadGoller();
                        break;
                    case "Ceza":
                        LoadCezalar();
                        break;
                    case "Transferler":
                        LoadTransfer();
                        break;
                    case "İstatistik":
                        Loadİstatistik();
                        break;
                    case "Kart":
                        LoadKart();
                        break;
                }
            }
        }

        private void LoadOyuncular()
        {
            string query = @"SELECT 
                        o.oyuncu_id AS OyuncuID,
                        k.ad AS Adı,
                        k.soyad AS Soyadı,
                        p.pozisyon_adi AS Pozisyon,
                        t.takim_adi AS Takım
                    FROM oyuncu o
                    JOIN kisi k ON o.kisi_id = k.kisi_id
                    JOIN pozisyon p ON o.pozisyon_id = p.pozisyon_id
                    JOIN takim t ON o.takim_id = t.takim_id
                    WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";

            ExecuteQueryAndBindData(query);
        }

        private void LoadTakimlar()
        {
            string query =query = @"
            SELECT 
                takim.takim_id AS takim_id,
                takim.takim_adi AS takim_adi,
                takim.kurulus_tarihi AS kurulus_tarihi,
                takim.kısaltma AS Kısaltma,
                takim.sehir AS sehir
            FROM takim
            LEFT JOIN kisi AS baskan ON takim.baskan_id = baskan.kisi_id
            LEFT JOIN kisi AS td ON takim.teknik_direktor_id = td.kisi_id;";
                    

            ExecuteQueryAndBindData(query);
        }

        private void LoadTeknikDirektor()
        {
            string query = @"
            SELECT 
                td.kisi_id AS TeknikDirektorID,
                k.ad AS Ad,
                k.soyad AS Soyad,
                td.dogum_tarihi AS DogumTarihi,
                td.tecrube AS Tecrube
            FROM teknik_direktor td
            JOIN kisi k ON td.kisi_id = k.kisi_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadHakem()
        {
            string query = @"
            SELECT 
                h.hakem_id AS HakemID,
                k.ad AS Ad,
                k.soyad AS Soyad,
                h.dogum_tarihi AS DogumTarihi,
                h.ulke AS Ulke
            FROM hakem h
            JOIN kisi k ON h.kisi_id = k.kisi_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadBaskan()
        {
            string query = @"
            SELECT 
                b.baskan_id AS BaskanID,
                k.ad AS Ad,
                k.soyad AS Soyad,
                b.dogum_tarihi AS DogumTarihi
            FROM baskan b
            JOIN kisi k ON b.kisi_id = k.kisi_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadStadyum()
        {
            string query = @"SELECT 
                        s.stadyum_id AS StadyumID,
                        s.ad AS Ad,
                        s.kapasite AS Kapasite,
                        t.takim_adi AS Takım
                    FROM stadyum s
                    JOIN takim t ON s.takim_id = t.takim_id
                    WHERE s.ad ILIKE @keyword OR s.sehir ILIKE @keyword;";

            ExecuteQueryAndBindData(query);
        }

        private void LoadTaraftarGrup()
        {
            string query = @"
            SELECT 
                tg.grup_adi AS GrupAdi,
                tg.sehir AS Sehir,
                tg.takim_adi AS TakimAdi
            FROM taraftar_grubu tg";

            ExecuteQueryAndBindData(query);
        }

        private void LoadKisi()
        {
            string  query = @"
            SELECT 
                k.kisi_id AS KisiID,
                k.Ad AS Ad,
                k.soyad AS Soyad,
                k.dogum_tarihi dogum_tarihi
            FROM kisi k
            WHERE k.ad ILIKE @keyword OR k.soyad ILIKE @keyword;";

            ExecuteQueryAndBindData(query);
        }

        private void LoadGoller()
        {
            string query = @"
            SELECT 
                g.gol_id AS GolID,
                o.ad AS Oyuncu,
                m.mac_tarihi AS MacTarihi,
                g.skor AS Skor
            FROM gol g
            JOIN oyuncu o ON g.oyuncu_id = o.oyuncu_id
            JOIN mac m ON g.mac_id = m.mac_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadCezalar()
        {
            string query = @"
            SELECT 
                c.ceza_id AS CezaID,
                o.ad AS Oyuncu,
                c.ceza_turu AS CezaTuru,
                m.mac_tarihi AS MacTarihi
            FROM ceza c
            JOIN oyuncu o ON c.oyuncu_id = o.oyuncu_id
            JOIN mac m ON c.mac_id = m.mac_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadTransfer()
        {
            string query = @"
            SELECT 
                t.transfer_id AS TransferID,
                o.ad AS Oyuncu,
                t.tarih AS Tarih,
                t.transfer_ucreti AS Ucret
            FROM transfer t
            JOIN oyuncu o ON t.oyuncu_id = o.oyuncu_id";

            ExecuteQueryAndBindData(query);
        }

        private void Loadİstatistik()
        {
            string query = @"
            SELECT 
                i.istatistik_id AS IstatistikID,
                o.ad AS Oyuncu,
                i.mac_id AS MacID,
                i.gol_sayisi AS GolSayisi,
                i.asist_sayisi AS AsistSayisi
            FROM istatistik i
            JOIN oyuncu o ON i.oyuncu_id = o.oyuncu_id";

            ExecuteQueryAndBindData(query);
        }

                private void LoadMaclar()
        {
            string query = @"
            SELECT 
                m.mac_id AS MacID,
                t1.takim_adi AS Takim1,
                t2.takim_adi AS Takim2,
                m.mac_tarihi AS MacTarihi,
                m.skor_takim1 AS Takim1Skor,
                m.skor_takim2 AS Takim2Skor,
                m.stadyum_adi AS Stadyum
            FROM mac m
            JOIN takim t1 ON m.takim1_id = t1.takim_id
            JOIN takim t2 ON m.takim2_id = t2.takim_id";

            ExecuteQueryAndBindData(query);
        }

        private void LoadLigTablosu()
        {
            string query = @"
            SELECT 
                lt.lig_id AS LigID,
                t.takim_adi AS TakimAdi,
                lt.puan AS Puan,
                lt.oynanan_mac_sayisi AS OynananMacSayisi,
                lt.gol_farkı AS GolFarki
            FROM ligtablosu lt
            JOIN takim t ON lt.takim_id = t.takim_id
            ORDER BY lt.puan DESC, lt.gol_farki DESC";

            ExecuteQueryAndBindData(query);
        }


        private void LoadKart()
        {
            string query = @"
            SELECT 
                k.kart_id AS KartID,
                o.ad AS Oyuncu,
                m.mac_tarihi AS MacTarihi,
                k.kart_turu AS KartTuru
            FROM kart k
            JOIN oyuncu o ON k.oyuncu_id = o.oyuncu_id
            JOIN mac m ON k.mac_id = m.mac_id";

            ExecuteQueryAndBindData(query);
        }

        private void ExecuteQueryAndBindData(string query)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                dataGrid.DataSource = dataTable;
            }
        }
    }
}