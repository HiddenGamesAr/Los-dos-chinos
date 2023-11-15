using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Los_dos_chinos
{
    public static class ServerManager
    {

        public static List<Article> ArtEnCarrito = new();
        public static bool firstTime = true;
        public static string tipo;
        public static User user = new();
        public static Session session;
        public static FormLogIn formLogIn;
        public static List<TextBox> textBoxes;
        public static Supermarket supermarket = new ("283373338333","Los dos chinos","Ameghino 234");
        public static Image logo;
        static string ubicacionEntorno = Environment.CurrentDirectory;
        public static string ubicacionProyecto = Directory.GetParent(ubicacionEntorno).Parent.FullName;
        public static string _ConnectionString { get; set; } = "Server=DESKTOP-TAEGQRB\\SQLEXPRESS;" +
            "Database=ChinoDB; Trusted_Connection=True;TrustServerCertificate=True";
        public static void LoadFormInPanel (Form form, Panel panel) //Load a form in a panel
        {
            panel.Controls.Clear();
            form.TopLevel = false;
            form.AutoScroll = true;
            panel.Controls.Add(form);
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Show();
        }
        public static void AddData (string table, string columns, List<string> data, string typeOfData) //Add data in a table of a sqlserver databse
        {
            using SqlConnection sqlConnection = new(_ConnectionString);
            sqlConnection.Open();
            string _values = $"@{columns.Replace(", ", ", @")}";
            SqlCommand sqlCommand = new($"INSERT INTO {table}({columns}) VALUES({_values})", sqlConnection);
            string[] separator = { ", ", "," };
            string[] valuesSeparated = _values.Split(separator, StringSplitOptions.None);
            for (int i = 0; i < valuesSeparated.Length; i++)
            {
                if (int.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], int.Parse(data[i])); }
                else if (float.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], float.Parse(data[i])); }
                else if (Int64.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], Int64.Parse(data[i])); }
                else { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], data[i]); }
            }
            sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
            CleanTxtBoxes();
            MessageBox.Show($"{typeOfData} agregado", "Operación satisfactoria", MessageBoxButtons.OK);
        }
        public static void ModifyData (string table, List<string> columns, List<string> data, string typeOfData, string id) //Modify data in a table of a sqlserver databse
        {
            using SqlConnection sqlConnection = new(_ConnectionString);
            sqlConnection.Open();
            string setter = string.Empty;
            for (int i = 1; i < columns.Count; i++) { setter += $"{columns[i]} = @{columns[i]},"; }
            setter = setter.TrimEnd(',');
            SqlCommand sqlCommand = new($"UPDATE {table} SET {setter} WHERE {id} = @{id}", sqlConnection);
            for (int i = 0; i < data.Count; i++)
            {
                if (columns[0] == null && i == 0) { sqlCommand.Parameters.AddWithValue($"@{id}", data[0]); continue; }
                if (int.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue($"@{columns[i]}", int.Parse(data[i])); }
                else if (float.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue($"@{columns[i]}", float.Parse(data[i])); }
                else { sqlCommand.Parameters.AddWithValue($"@{columns[i]}", data[i]); }
            }
            sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
            CleanTxtBoxes();
            MessageBox.Show($"{typeOfData} modificado", "Operación satisfactoria", MessageBoxButtons.OK);
        }
        public static void DeleteData (string table, string id, string idValue) //Delete data in a table of a sqlserver databse
        {
            using SqlConnection sqlConnection = new(_ConnectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new($"DELETE FROM {table} WHERE {id} = @id", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@id", Int64.Parse(idValue));
            sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
            CleanTxtBoxes();
            firstTime = true;
        }
        public static void ReadSupermarketData ()
        {
            using (StreamReader sr = File.OpenText($@"{ubicacionProyecto}\supermarketdata.txt"))
            {
                string str = sr.ReadToEnd();
                string[] separators = { "\r\n" };
                string[] dataSeparated = str.Split(separators, System.StringSplitOptions.None);
                if (dataSeparated.Length >= 3)
                {
                    ServerManager.supermarket.Nombre = dataSeparated[0]; ServerManager.supermarket.CUIT = dataSeparated[1];
                    ServerManager.supermarket.Direccion = dataSeparated[2];
                }
            }
        }
        public static void UpdateSupermarketData ()
        {
            using (StreamWriter sw = File.CreateText($@"{ubicacionProyecto}\supermarketdata.txt"))
            {
                sw.WriteLine($"{supermarket.Nombre}");
                sw.WriteLine($"{supermarket.CUIT}");
                sw.WriteLine($"{supermarket.Direccion}");
            }
        }
        public static bool TxtBsNotEmpty () { if (textBoxes.FirstOrDefault(x => x.Text == string.Empty) == null) { return true; } else { return false; } } //Used to check if the textBoxes are empty or not
        public static void CleanTxtBoxes () { foreach (var txtB in textBoxes) { txtB.Clear(); } } //Clean the textBoxes
        public static void AddSession () //Add current session to the sessions table on the database
        {
            using SqlConnection sqlConnection = new(_ConnectionString);
            sqlConnection.Open();
            string columns = "UserID, Date, StartTime, EndTime";
            string _values = $"@{columns.Replace(", ", ", @")}";
            SqlCommand sqlCommand = new($"INSERT INTO Sessions({columns}) VALUES({_values})", sqlConnection);
            string[] separator = { ", ", "," };
            string[] valuesSeparated = _values.Split(separator, StringSplitOptions.None);
            sqlCommand.Parameters.AddWithValue(valuesSeparated[0], session.UserID);
            sqlCommand.Parameters.AddWithValue(valuesSeparated[1], session.Date);
            sqlCommand.Parameters.AddWithValue(valuesSeparated[2], session.StartTime);
            sqlCommand.Parameters.AddWithValue(valuesSeparated[3], session.EndTime);
            sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
            //CleanTxtBoxes();
            //MessageBox.Show($"{typeOfData} agregado", "Operación satisfactoria", MessageBoxButtons.OK);
        }
        public static void ManageSales (string columns, List<string> data) //Storage a sale and set it's saleID based on last sale's ID
        {
            string nextSaleID;
            using SqlConnection sqlConnection = new(_ConnectionString);
            sqlConnection.Open();

            SqlCommand sqlCommand = new ($"SELECT TOP 1 * FROM Sales order by SaleID DESC", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            if (sqlDataReader.Read())
            {
                string saleID = sqlDataReader.GetValue(0).ToString();
                string[] idSeparated = saleID.Split(new string[] {"-"},StringSplitOptions.RemoveEmptyEntries);
                if (idSeparated[1] == "99999")
                {
                    idSeparated[1] = "00000";
                    int firstValue = int.Parse(idSeparated[0]);
                    firstValue++;
                    idSeparated[0] = firstValue.ToString("00000");
                }
                else
                {
                    int secondValue = int.Parse(idSeparated[1]);
                    secondValue++;
                    idSeparated[1] = secondValue.ToString("00000");
                }
                nextSaleID = $"{idSeparated[0]}-{idSeparated[1]}";
            }
            else { nextSaleID = "00000-00001"; }
            data[0] = nextSaleID;
            sqlCommand.Dispose(); sqlDataReader.Dispose();
            string _values = $"@{columns.Replace(", ", ", @")}";
            string[] separator = { ", ", "," };
            string[] valuesSeparated = _values.Split(separator, StringSplitOptions.None);
            sqlCommand = new ($"INSERT INTO Sales({columns}) VALUES({_values})", sqlConnection);
            for (int i = 0; i < valuesSeparated.Length; i++)
            {
                if (int.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], int.Parse(data[i])); }
                else if (float.TryParse(data[i], out _)) { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], float.Parse(data[i])); }
                else { sqlCommand.Parameters.AddWithValue(valuesSeparated[i], data[i]); }
            }
            sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
        }
    }
}
