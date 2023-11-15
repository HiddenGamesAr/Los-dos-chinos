using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using ZXing;
using AForge.Video;
using ZXing.QrCode;
using FluentValidation.Results;
using Los_dos_chinos.Validator;
using System.Windows.Controls;

namespace Los_dos_chinos.OtherForms
{
    public partial class FormMenu : Form
    {

        #region Sales REGION
        void dgViewCarrito_KeyPress (object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 100 && dgViewCarrito.CurrentCell.Value != null)
            {
                foreach (DataGridViewCell cell in dgViewCarrito.SelectedCells)
                {
                    dgViewCarrito.Rows.RemoveAt(dgViewCarrito.Rows[cell.RowIndex].Index);
                }
                txtBMontoT.Text = "$0";
                float totalAmount = 0;
                foreach (DataGridViewRow row in dgViewCarrito.Rows)
                {
                    if (row.Cells[4].Value != null)
                    {
                        totalAmount += float.Parse(row.Cells[5].Value.ToString().Replace("$", ""));
                    }
                }
                txtBMontoT.Text = $"${totalAmount}";
            }
        }
        void dgViewArticulos_KeyPress (object sender, KeyPressEventArgs e) { if (e.KeyChar == 97) { dgViewArticulos_CellDoubleClick(null, null); } }
        void dgViewCarrito_CellValueChanged (object sender, DataGridViewCellEventArgs e)
        {
            if (dgViewCarrito.RowCount > 1)
            {
                if (dgViewCarrito.CurrentCell.ColumnIndex == 4 && dgViewCarrito.CurrentCell.Value != null) //only rows that have data in their fourth cell
                {
                    // If the value of current cell isn't a number, it is set to 0
                    if (!int.TryParse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value.ToString(), out _))
                    {
                        dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value = 0;
                    }
                    // If the amount value of current cell is negative, it is turned into positive
                    if (int.Parse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value.ToString()) < 0)
                    {
                        dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value = int.Parse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value.ToString()) * -1;
                    }// When a cell value has been changed, the subtotal of the row that cell belongs to is calculated
                    float previousSubT = float.Parse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[5].Value.ToString().Replace("$", ""));
                    UpdateTotalAmount((previousSubT * -1));
                    float subTotal = float.Parse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[4].Value.ToString()) *
                    float.Parse(dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[3].Value.ToString().Replace("$", ""));
                    dgViewCarrito.Rows[dgViewCarrito.CurrentCell.RowIndex].Cells[5].Value = $"${subTotal}";
                    UpdateTotalAmount(subTotal);
                }

            }
        }
        void dgViewArticulos_CellDoubleClick (object sender, DataGridViewCellEventArgs e)
        {
            UpdateTotalAmount(0);
            var articleFound = false;
            int rowIndex = -1;
            foreach (DataGridViewRow row in dgViewArticulos.SelectedRows)
            {
                foreach (DataGridViewRow row2 in dgViewCarrito.Rows)
                {
                    if (row2.Cells[0].Value == row.Cells[0].Value && row.Cells[0].Value?.ToString() != string.Empty)
                    {
                        articleFound = true; rowIndex = row2.Index; break;
                    }
                }
                if (articleFound == false)
                {
                    dgViewCarrito.Rows.Add(row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value,
                    $"{row.Cells[3].Value}", 1, $"{row.Cells[3].Value}");
                    foreach (DataGridViewRow row2 in dgViewCarrito.Rows) // Find the row where the article is and adds its subtotal to the total amount.
                    {
                        if (row2.Cells[0].Value == row.Cells[0].Value && row.Cells[0].Value?.ToString() != string.Empty)
                        {
                            UpdateTotalAmount(float.Parse(row2.Cells[3].Value.ToString().Replace("$", "")));
                        }
                    }

                }
                else if (dgViewCarrito.Rows[rowIndex].Cells[4].Value != null)
                {
                    dgViewCarrito.Rows[rowIndex].Cells[4].Value = int.Parse(dgViewCarrito.Rows[rowIndex].Cells[4].Value.ToString()) + 1;
                    float subTotal = float.Parse(row.Cells[3].Value.ToString().Replace("$", "")) * int.Parse(dgViewCarrito.Rows[rowIndex].Cells[4].Value.ToString());
                    dgViewCarrito.Rows[rowIndex].Cells[5].Value = $"${subTotal}";
                    foreach (DataGridViewRow row2 in dgViewCarrito.Rows) // Find the row where the article is and adds its subtotal to the total amount.
                    {
                        if (row2.Cells[0].Value == row.Cells[0].Value && row.Cells[0].Value?.ToString() != string.Empty)
                        {
                            UpdateTotalAmount(float.Parse(row2.Cells[3].Value.ToString().Replace("$", "")));
                        }
                    }
                }
            }
        }
        void UpdateTotalAmount (float subtotal)
        {
            if (txtBMontoT.Text == string.Empty) { txtBMontoT.Text = "$0"; }
            else
            {
                float totalAmount = float.Parse(txtBMontoT.Text.ToString().Replace("$", "")) + subtotal;
                txtBMontoT.Text = $"${totalAmount}";
            }
        }
        void txtBArticleID_TextChanged (object sender, EventArgs e) // Show articles that match txtBArticleID's text
        {
            dgViewArticulos.AllowUserToAddRows = false;
            if (txtBArticleID.Text != string.Empty && !checkBUsarLCB.Checked)
            {
                foreach (DataGridViewRow row in dgViewArticulos.Rows)
                {
                    if (row.Cells[0].Value.ToString().Contains(txtBArticleID.Text)) { row.Visible = true; }
                    else { row.Visible = false; }
                }
            }
            else
            {
                foreach (DataGridViewRow row in dgViewArticulos.Rows) { if (!row.Visible) row.Visible = true; }
            }
        }
        void BtnGenTicket_Click (object sender, EventArgs e) // Generates the Sale's ticket
        {
            using SaveFileDialog saveFileDiag = new() { Filter = "PDF file|*.pdf", ValidateNames = true };
            if (saveFileDiag.ShowDialog() == DialogResult.OK)
            {
                List<Int64> articleIDs = new(); List<int> articlesAmount = new();
                for (int i = 0; i < dgViewCarrito.RowCount; i++)
                {
                    if (dgViewCarrito["Código2", i].Value?.ToString() != string.Empty && dgViewCarrito["Código2", i].Value != null)
                    {
                        articleIDs.Add(Int64.Parse(dgViewCarrito["Código2", i].Value.ToString()));
                        articlesAmount.Add(int.Parse(dgViewCarrito["Cantidad", i].Value.ToString()));
                    }
                }
                UpdateStock(articleIDs, articlesAmount);

                iTextSharp.text.Rectangle rect = new(360, 720);
                Document doc = new(rect, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDiag.FileName, FileMode.Create));
                doc.Open();
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ServerManager.logo, System.Drawing.Imaging.ImageFormat.Png);
                image.ScalePercent(20);
                image.Alignment = Element.ALIGN_LEFT;
                doc.Add(image);

                iTextSharp.text.Font font = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11);
                doc.Add(new Paragraph($"SUPERMERCADO: {ServerManager.supermarket.Nombre}") { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"CUIT: {ServerManager.supermarket.CUIT}") { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"DIRECCIÓN: {ServerManager.supermarket.Direccion}") { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("------------------------------------------------------------------------------------")
                { Alignment = Element.ALIGN_CENTER });

                PdfPTable table = new(dgViewCarrito.ColumnCount - 1) { WidthPercentage = 103 };
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                for (int i = 1; i < dgViewCarrito.ColumnCount; i++) { table.AddCell(new Paragraph(dgViewCarrito.Columns[i].HeaderText, font)); }
                float totalPrice = 0;
                for (int i = 0; i < dgViewCarrito.Rows.Count - 1; i++)
                {
                    table.AddCell(new Paragraph($"{dgViewCarrito[1, i].Value}", font));
                    table.AddCell(new Paragraph($"{dgViewCarrito[2, i].Value}", font));
                    table.AddCell(new Paragraph($"{dgViewCarrito[3, i].Value}", font));
                    table.AddCell(new Paragraph($"{dgViewCarrito[4, i].Value}", font));
                    table.AddCell(new Paragraph($"{dgViewCarrito[5, i].Value}", font));
                    totalPrice += float.Parse(dgViewCarrito["Subtotal", i].Value.ToString().Replace("$", string.Empty));
                }
                table.CompleteRow();
                doc.Add(table);
                doc.Add(new Paragraph($"MONTO TOTAL: ${totalPrice}") { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"FECHA Y HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}") { Alignment = Element.ALIGN_CENTER });

                Sale newSale = new("", totalPrice, DateTime.Now, ServerManager.user.UserID);
                List<string> data = new() { newSale.SaleID, newSale.Amount.ToString(), newSale.Date.ToString("yyyy/MM/dd HH:mm:ss"), newSale.UserID.ToString() };
                ServerManager.ManageSales("saleid, amount, Date, userid", data);
                doc.Add(new Paragraph($"TICKET N° {data[0]}") { Alignment = Element.ALIGN_CENTER });
                doc.Close();
                writer.Close();
            }
        }
        void txtBPaga_TextChanged (object sender, EventArgs e)
        {
            if (float.TryParse(txtBMontoT.Text.Replace("$", "").Replace(",", "."), out _) &&
                float.TryParse(txtBPaga.Text.Replace("$", "").Replace(",", "."), out _))
            {
                float montoTotal = float.Parse(txtBMontoT.Text.Replace("$", "").Replace(",", "."));
                float montoAPagar = float.Parse(txtBPaga.Text.Replace("$", "").Replace(",", "."));
                float vuelto = montoAPagar - montoTotal;
                if (vuelto >= 0) txtBVuelto.Text = $"${vuelto.ToString().Replace(".", ",")}";
                else { txtBVuelto.Text = "$0"; }
            }
        }
        void txtBMontoT_TextChanged (object sender, EventArgs e) { txtBPaga_TextChanged(null, null); }
        void UpdateStock (List<Int64> articleIDs, List<int> ArticleCantidad)
        {
            for (int i = 0; i < articleIDs.Count; i++)
            {
                int newStock = 0;
                using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = new($"SELECT * FROM Articles Where ArticleID = @id", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@id", articleIDs[i]);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                if (sqlDataReader.Read())
                {
                    newStock = (int.Parse(sqlDataReader.GetValue(5).ToString()) - ArticleCantidad[i]);
                }
                sqlDataReader.Close(); sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
                sqlCommand = new($"UPDATE Articles SET Stock = @stock WHERE ArticleID = @id", sqlConnection);
                sqlCommand.Parameters.AddWithValue("@id", articleIDs[i]);
                sqlCommand.Parameters.AddWithValue("@stock", newStock);
                sqlCommand.ExecuteNonQuery(); sqlCommand.Dispose();
            }
        }
        #endregion

        #region Scan barcode
        decimal articleID;
        FilterInfoCollection FilterInfoCollection;
        VideoCaptureDevice captureDevice;
        void checkBUseBarCodeScanner_CheckedChanged (object sender, EventArgs e) //Check if BarCodeScanner'ckeckBox is checked or  not
        {
            if (checkBUsarLCB.Checked && FilterInfoCollection.Count > 0)
            {
                captureDevice = new VideoCaptureDevice(FilterInfoCollection[0].MonikerString);
                captureDevice.NewFrame += CaptureDevice_NewFrame;
                captureDevice.Start();
                timer1.Start();
            }
            else { timer1.Stop(); captureDevice.Stop(); pictureBECD.Image = null; }
        }
        void CaptureDevice_NewFrame (object sender, NewFrameEventArgs eventArgs)
        {
            pictureBECD.Image = (Bitmap)eventArgs.Frame.Clone();
        }
        void FormMenu_FormClosing (object sender, FormClosingEventArgs e)
        {
            if (captureDevice != null && captureDevice.IsRunning) captureDevice.Stop();
            ServerManager.session.EndTime = DateTime.Now.ToLongTimeString();
            ServerManager.AddSession();
        }
        void timer1_Tick (object sender, EventArgs e)
        {
            if (pictureBECD.Image != null)
            {
                BarcodeReader BCReader = new();
                BCReader.Options.PureBarcode = true;
                BCReader.Options.TryHarder = true;
                BCReader.Options.TryInverted = true;
                Result result = BCReader.Decode((Bitmap)pictureBECD.Image);
                if (result != null && result.BarcodeFormat == BarcodeFormat.EAN_13)
                {
                    articleID = Int64.Parse(result.ToString());
                    foreach (DataGridViewRow row in dgViewArticulos.Rows) //Itera entre los artículos
                    {
                        if (decimal.Parse(row.Cells[0].Value.ToString()) == articleID) // Se encotnró el artículo
                        {
                            int rowIndex = -1;
                            bool articleFound = false;
                            foreach (DataGridViewRow row2 in dgViewCarrito.Rows) // Se comprueba si está en el carrito
                            {
                                if (row2.Cells[0].Value == row.Cells[0].Value)
                                {
                                    articleFound = true; rowIndex = row2.Index; break;
                                }
                            }
                            if (articleFound == false)
                            {
                                dgViewCarrito.Rows.Add(row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value,
                                $"{row.Cells[3].Value}", 1, $"{row.Cells[3].Value}");
                            }
                            else
                            {
                                dgViewCarrito.Rows[rowIndex].Cells[4].Value = (int)dgViewCarrito.Rows[rowIndex].Cells[4].Value + 1;
                                float subTotal = float.Parse(row.Cells[3].Value.ToString().Replace("$", "")) * int.Parse(dgViewCarrito.Rows[rowIndex].Cells[4].Value.ToString());
                                dgViewCarrito.Rows[rowIndex].Cells[5].Value = $"${subTotal}";
                            }
                            txtBArticleID.Text = result.ToString();
                            break;
                        }
                    }// Si después de buscar no se encontró el artículo se le informa al usuario.
                    //if(!articleNotStoraged) { MessageBox.Show($"Artículo no encontrado", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
                else { if (txtBArticleID.Text != string.Empty) txtBArticleID.Text = string.Empty; }
            }
        }
        #endregion
        
        #region Panels_REGION
        void BtnSalesMenu_Click (object sender, EventArgs e) //Display Sales menu
        {//The Sales panel is displayed and the articles are loaded into de artices table
            pnlSales.BringToFront();
            using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
            sqlConnection.Open();
            ServerManager.tipo = "Article";
            SqlCommand sqlCommand = new($"SELECT * FROM {ServerManager.tipo}s", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            dgViewArticulos.Rows.Clear();
            dgViewArticulos.DataSource = null;
            while (sqlDataReader.Read())
            {
                dgViewArticulos.Rows.Add($"{Int64.Parse(sqlDataReader.GetValue(0).ToString())}", $"{sqlDataReader.GetValue(1)}",
                    $"{sqlDataReader.GetValue(2)}", $"${sqlDataReader.GetValue(4)}", $"{sqlDataReader.GetValue(5)}");
            }
            sqlCommand.Dispose(); sqlDataReader.Dispose();
        }
        void BtnUsersMenu_Click (object sender, EventArgs e) //Display Users menu
        {
            pnlUsers.BringToFront(); // Se muestra el panel de usuarios y se cargan enstos en el dgviewUsers
            ServerManager.tipo = "User";
            List<User> _Users = new();
            using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new($"SELECT * FROM {ServerManager.tipo}s", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                _Users.Add(new User((int)sqlDataReader.GetValue(0), (int)sqlDataReader.GetValue(3),
                    (string)sqlDataReader.GetValue(1), (string)sqlDataReader.GetValue(2),
                    (string)sqlDataReader.GetValue(4), (string)sqlDataReader.GetValue(5)));
            }
            dgviewUsers.DataSource = _Users;
            dgviewUsers.Columns["UserID"].Visible = false;
            ServerManager.textBoxes = new() { txtBName, txtBPassword, txtBEmail, txtBCellphone, txtBAccess };
            usersSelected.Clear();
            ServerManager.CleanTxtBoxes();
            sqlCommand.Dispose(); sqlDataReader.Dispose(); sqlConnection.Close();

            // Cargar sesiones en DataGridView
            ServerManager.tipo = "Session";
            List<Session> _Sessions = new();
            sqlConnection.Open();
            sqlCommand = new($"SELECT * FROM {ServerManager.tipo}s", sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                _Sessions.Add(new(int.Parse(sqlDataReader.GetValue(1).ToString()),
                    sqlDataReader.GetDateTime(2), sqlDataReader.GetValue(3).ToString(), sqlDataReader.GetValue(4).ToString()));
            }
            dgviewSessions.DataSource = _Sessions;
            sqlCommand.Dispose(); sqlDataReader.Dispose();
        }
        void BtnArticlesMenu_Click (object sender, EventArgs e) //Display Articles menu
        {
            pnlArticles.BringToFront();
            ServerManager.tipo = "Article";
            List<Article> _Articles = new();
            using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new($"SELECT * FROM {ServerManager.tipo}s", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                _Articles.Add(new Article(Int64.Parse(sqlDataReader.GetValue(0).ToString()), sqlDataReader.GetValue(1).ToString(),
                    sqlDataReader.GetValue(2).ToString(), float.Parse(sqlDataReader.GetValue(3).ToString()),
                    float.Parse(sqlDataReader.GetValue(4).ToString()), int.Parse(sqlDataReader.GetValue(5).ToString()),
                    sqlDataReader.GetValue(6).ToString()));
            }
            dgviewArticles.DataSource = _Articles;
            ServerManager.textBoxes = new() { txtBCode, txtBDetail, txtBPresentation, txtBPurchasePrice, txtSalePrice, txtBStock };
            articlesSelected.Clear();
            ServerManager.CleanTxtBoxes();
            sqlCommand.Dispose(); sqlDataReader.Dispose();
            cmbBProveedor.Items.Clear();
            // Se agregan los proveedores al combobox del panel de artículos
            sqlCommand = new($"SELECT * FROM Suppliers", sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read()) { cmbBProveedor.Items.Add(sqlDataReader.GetValue(1).ToString()); }
            sqlCommand.Dispose(); sqlDataReader.Dispose();
        }
        void BtnSuppliersMenu_Click (object sender, EventArgs e) //Display suppliers menu
        {
            pnlSuppliers.BringToFront();
            ServerManager.tipo = "Supplier";
            List<Supplier> _Suppliers = new();
            using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new($"SELECT * FROM {ServerManager.tipo}s", sqlConnection);
            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            while (sqlDataReader.Read())
            {
                _Suppliers.Add(new Supplier(int.Parse(sqlDataReader.GetValue(0).ToString()), sqlDataReader.GetValue(1).ToString(),
                    sqlDataReader.GetValue(2).ToString(), sqlDataReader.GetValue(3).ToString(), sqlDataReader.GetValue(4).ToString(),
                    sqlDataReader.GetValue(5).ToString(), sqlDataReader.GetValue(6).ToString()));
            }
            dgviewSuppliers.DataSource = _Suppliers;
            //dgviewUsers.Columns["UserID"].Visible = false;
            ServerManager.textBoxes = new() { txtBSupName, txtBSupCUIT, txtBSupEmail, txtBSupCellphone, txtBSupArea, txtBSupAddress };
            suppliersSelected.Clear();
            ServerManager.CleanTxtBoxes();
            sqlCommand.Dispose(); sqlDataReader.Dispose();
        }
        void BtnSuperMenu_Click (object sender, EventArgs e) //Display supermarket menu
        {
            pnlSupermarket.BringToFront();
            txtBSuperCUIT.Text = ServerManager.supermarket.CUIT;
            txtBSuperName.Text = ServerManager.supermarket.Nombre;
            txtBSuperAddress.Text = ServerManager.supermarket.Direccion;
        }
        void BtnSurveillanceMenu_Click (object sender, EventArgs e) { FormSurveillanceCam formSurveillance = new(); formSurveillance.Show(); } //Display Surveillance menu as a separated window
        void BtnQuit_Click (object sender, EventArgs e) { ServerManager.formLogIn.ShowNewLogIn(); } //Close the session and returns to LogIn Menu
        #endregion

        #region Users_REGION
        void BtnUserAdd_Click (object sender, EventArgs e) // Add a user
        {
            ServerManager.textBoxes = new() { txtBName, txtBPassword, txtBAccess, txtBEmail, txtBCellphone };
            if (ServerManager.TxtBsNotEmpty())
            {
                User newUser = new(int.TryParse(txtBAccess.Text, out int value) ? value : -1, txtBName.Text, txtBPassword.Text,
                txtBEmail.Text.ToLower(), txtBCellphone.Text);
                UserVal validator = new();
                FluentValidation.Results.ValidationResult result = validator.Validate(newUser);
                if (result.IsValid)
                {

                    List<string> newUserData = new() { txtBName.Text,txtBPassword.Text, txtBAccess.Text,
                        txtBEmail.Text.ToLower(), txtBCellphone.Text};
                    string columns = "Name, Password, Access, Email, Cellphone";
                    ServerManager.AddData("Users", columns, newUserData, "Usuario");
                    BtnUsersMenu_Click(null,null);
                }
                else
                {
                    string message = "";
                    foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                    MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        void BtnUserMod_Click (object sender, EventArgs e) // Modify a user
        {
            if (usersSelected.Count == 1)
            {
                ServerManager.textBoxes = new() { txtBName, txtBPassword, txtBEmail, txtBCellphone, txtBAccess };
                if(ServerManager.TxtBsNotEmpty())
                {
                    User newUser = new(usersSelected[0].UserID, int.TryParse(txtBAccess.Text, out int value) ? value : -1, txtBName.Text, txtBPassword.Text,
               txtBEmail.Text.ToLower(), txtBCellphone.Text);
                    UserVal validator = new();
                    FluentValidation.Results.ValidationResult result = validator.Validate(newUser);
                    if (result.IsValid)
                    {
                        List<string> columns = new() { null, "name", "password", "access", "email", "cellphone" };
                        List<string> newUserData = new() { usersSelected[0].UserID.ToString(), txtBName.Text, txtBPassword.Text, txtBAccess.Text, txtBEmail.Text.ToLower(), txtBCellphone.Text };
                        ServerManager.ModifyData("Users", columns, newUserData, "Usuario", "userid");
                        BtnUsersMenu_Click(null, null);
                    }
                    else
                    {
                        string message = "";
                        foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                        MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            else if(usersSelected.Count > 1) { MessageBox.Show($"Seleccione solamente un usuario", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            else { MessageBox.Show($"No se seleccionó ningún usuario", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void BtnUserDel_Click (object sender, EventArgs e) // Delete user/s
        {
            if(usersSelected.Count >= 1)
            {
                ServerManager.tipo = "User";
                string userNames = string.Empty;
                foreach (var userSelected in usersSelected) { userNames += $"{userSelected.Name}, "; }
                userNames += "."; userNames = userNames.Replace(", .", ".");
                DialogResult respuesta = MessageBox.Show($"Está seguro que desea eliminar a los usuarios: {userNames}",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (respuesta == DialogResult.Yes)
                {
                    foreach (var user in usersSelected)
                    {
                        ServerManager.DeleteData("Users", "userid", user.UserID.ToString());
                    }
                    if (usersSelected.Count == 1) MessageBox.Show($"Usuario/s eliminado/s", "Operación satisfactoria", MessageBoxButtons.OK);
                    else { MessageBox.Show($"Usuario eliminado", "Operación satisfactoria", MessageBoxButtons.OK); }
                    BtnUsersMenu_Click(null, null);
                }
            }
            else { MessageBox.Show($"No se seleccionó ningún usuario", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void dgviewUsers_SelectionChanged (object sender, EventArgs e) //Update userSelected list
        {
            if (dgviewUsers.SelectedRows.Count == 1)
            {
                if (usersSelected.Count == 0)
                {
                    usersSelected.Add(new(int.Parse(dgviewUsers.SelectedRows[0].Cells["UserID"].Value.ToString()),
                    int.Parse(dgviewUsers.SelectedRows[0].Cells["Access"].Value.ToString()), (string)dgviewUsers.SelectedRows[0].Cells["Name"].Value,
                    (string)dgviewUsers.SelectedRows[0].Cells["Password"].Value, (string)dgviewUsers.SelectedRows[0].Cells["Email"].Value,
                    (string)dgviewUsers.SelectedRows[0].Cells["Cellphone"].Value));
                }
                else
                {
                    usersSelected[0] = new(int.Parse(dgviewUsers.SelectedRows[0].Cells["UserID"].Value.ToString()),
                    int.Parse(dgviewUsers.SelectedRows[0].Cells["Access"].Value.ToString()), (string)dgviewUsers.SelectedRows[0].Cells["Name"].Value,
                    (string)dgviewUsers.SelectedRows[0].Cells["Password"].Value, (string)dgviewUsers.SelectedRows[0].Cells["Email"].Value,
                    (string)dgviewUsers.SelectedRows[0].Cells["Cellphone"].Value);
                }
                txtBName.Text = usersSelected[0].Name; txtBPassword.Text = usersSelected[0].Password;
                txtBAccess.Text = usersSelected[0].Access.ToString(); txtBEmail.Text = usersSelected[0].Email;
                txtBCellphone.Text = usersSelected[0].Cellphone;
            }
            else if (dgviewUsers.SelectedRows.Count > 1)
            {
                ServerManager.CleanTxtBoxes();
                List<User> newUsersSelected = new();
                foreach (DataGridViewRow row in dgviewUsers.SelectedRows)
                {
                    newUsersSelected.Add(new(int.Parse(row.Cells[0].Value.ToString()), row.Cells[1].Value.ToString()));
                }
                usersSelected = newUsersSelected;
            }
            else { usersSelected.Clear(); }
        }
        #endregion

        #region Articles_REGION
        void BtnArticleAdd_Click (object sender, EventArgs e) //Add an article
        {
            ServerManager.textBoxes = new() { txtBCode, txtBDetail, txtBPresentation, txtBPurchasePrice, txtSalePrice,txtBStock};
            if (ServerManager.TxtBsNotEmpty())
            {
                Article newArticle = new(Int64.TryParse(txtBCode.Text, out Int64 value) ? value : -1,txtBDetail.Text,txtBPresentation.Text,
                    float.TryParse(txtBPurchasePrice.Text, out float floatValue) ? floatValue : -1,
                    float.TryParse(txtSalePrice.Text, out floatValue) ? floatValue : -1, int.TryParse(txtBStock.Text, out int intValue) ? intValue : -1,
                    cmbBProveedor.Text);
                ArticleVal validator = new();
                FluentValidation.Results.ValidationResult result = validator.Validate(newArticle);
                if (result.IsValid)
                {
                    List<string> newArticleData = new() { txtBCode.Text, txtBDetail.Text, txtBPresentation.Text,
                        txtBPurchasePrice.Text, txtSalePrice.Text, txtBStock.Text, cmbBProveedor.Text };
                    string columns = "ArticleID, Detail, Presentation, Buying_Price, Selling_Price, Stock, Supplier";
                    ServerManager.AddData("Articles", columns, newArticleData, "Articulo");
                    BtnArticlesMenu_Click(null, null);
                }
                else
                {
                    string message = "";
                    foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                    MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        void BtnArticleMod_Click (object sender, EventArgs e) //Modify an article
        {
            if (articlesSelected.Count == 1)
            {
                if (ServerManager.TxtBsNotEmpty())
                {
                    Article newArticle = new(Int64.TryParse(txtBCode.Text, out Int64 value) ? value : -1, txtBDetail.Text, txtBPresentation.Text,
                        float.TryParse(txtBPurchasePrice.Text, out float floatValue) ? floatValue : -1,
                        float.TryParse(txtSalePrice.Text, out floatValue) ? floatValue : -1, int.TryParse(txtBStock.Text, out int intValue) ? intValue : -1,
                        cmbBProveedor.Text);
                    ArticleVal validator = new();
                    FluentValidation.Results.ValidationResult result = validator.Validate(newArticle);
                    if (result.IsValid)
                    {
                        ServerManager.textBoxes = new() { txtBCode, txtBDetail, txtBPresentation, txtBPurchasePrice, txtSalePrice, txtBStock };
                        List<string> columns = new() { "articleID", "detail", "presentation", "Buying_Price", "Selling_Price", "stock", "Supplier" };
                        List<string> newArticleData = new() { txtBCode.Text, txtBDetail.Text, txtBPresentation.Text,txtBPurchasePrice.Text,
                    txtSalePrice.Text, txtBStock.Text, cmbBProveedor.Text.ToString()};
                        ServerManager.ModifyData("Articles", columns, newArticleData, "Artículo", "articleid");
                        BtnArticlesMenu_Click(null, null);
                    }
                    else
                    {
                        string message = "";
                        foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                        MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            else if (articlesSelected.Count > 1) { MessageBox.Show($"Seleccione solamente un artículo", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            else { MessageBox.Show($"No se seleccionó ningún artículo", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void BtnArticleDel_Click (object sender, EventArgs e) // Delete article/s
        {
            if (articlesSelected.Count >= 1)
            {
                ServerManager.tipo = "Article";
                string articlesNames = string.Empty;
                foreach (var articleSelected in articlesSelected) { articlesNames += $"{articleSelected.Detalle}, "; }
                articlesNames += "."; articlesNames = articlesNames.Replace(", .", ".");
                DialogResult respuesta = MessageBox.Show($"Está seguro que desea eliminar los artículos: {articlesNames}",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (respuesta == DialogResult.Yes)
                {
                    foreach (var article in articlesSelected)
                    {
                        ServerManager.DeleteData("Articles", "ArticleID", article.ArticuloID.ToString());
                    }
                    if (articlesSelected.Count == 1) MessageBox.Show($"Artículo/s eliminado/s", "Operación satisfactoria", MessageBoxButtons.OK);
                    else { MessageBox.Show($"Artículo eliminado", "Operación satisfactoria", MessageBoxButtons.OK); }
                    BtnArticlesMenu_Click(null, null);
                }
            }
            else { MessageBox.Show($"No se seleccionó ningún artículo", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void dgviewArticles_SelectionChanged (object sender, EventArgs e) // Update articlesSelected list
        {
            if (dgviewArticles.SelectedRows.Count == 1)
            {
                if (articlesSelected.Count == 0)
                {
                    articlesSelected.Add(new(Int64.Parse(dgviewArticles.SelectedRows[0].Cells["ArticuloID"].Value.ToString()),
                        dgviewArticles.SelectedRows[0].Cells["Detalle"].Value.ToString(),
                        dgviewArticles.SelectedRows[0].Cells["Presentacion"].Value.ToString(),
                        float.Parse(dgviewArticles.SelectedRows[0].Cells["PrecioCompra"].Value.ToString()),
                        float.Parse(dgviewArticles.SelectedRows[0].Cells["PrecioVenta"].Value.ToString()),
                        int.Parse(dgviewArticles.SelectedRows[0].Cells["Stock"].Value.ToString()), 
                        dgviewArticles.SelectedRows[0].Cells["Proveedor"].Value.ToString()));
                }
                else
                {
                    articlesSelected[0] = new(Int64.Parse(dgviewArticles.SelectedRows[0].Cells["ArticuloID"].Value.ToString()),
                        dgviewArticles.SelectedRows[0].Cells["Detalle"].Value.ToString(),
                        dgviewArticles.SelectedRows[0].Cells["Presentacion"].Value.ToString(),
                        float.Parse(dgviewArticles.SelectedRows[0].Cells["PrecioCompra"].Value.ToString()),
                        float.Parse(dgviewArticles.SelectedRows[0].Cells["PrecioVenta"].Value.ToString()),
                        int.Parse(dgviewArticles.SelectedRows[0].Cells["Stock"].Value.ToString()),
                        dgviewArticles.SelectedRows[0].Cells["Proveedor"].Value.ToString());
                }
                txtBCode.Text = articlesSelected[0].ArticuloID.ToString(); txtBDetail.Text = articlesSelected[0].Detalle;
                txtBPresentation.Text = articlesSelected[0].Presentacion; txtBPurchasePrice.Text = articlesSelected[0].PrecioCompra.ToString();
                txtSalePrice.Text = articlesSelected[0].PrecioVenta.ToString(); txtBStock.Text = articlesSelected[0].Stock.ToString();
                cmbBProveedor.SelectedIndex = cmbBProveedor.Items.IndexOf(articlesSelected[0].Proveedor);
            }
            else if (dgviewArticles.SelectedRows.Count > 1)
            {
                ServerManager.CleanTxtBoxes();
                List<Article> newArticlesSelected = new();
                foreach (DataGridViewRow row in dgviewArticles.SelectedRows)
                {
                    newArticlesSelected.Add(new(Int64.Parse(row.Cells[0].Value.ToString()), row.Cells[1].Value.ToString()));
                }
                articlesSelected = newArticlesSelected;
            }
        }
        void BtnArticlesGenList_Click (object sender, EventArgs e) //Generate pdf that contains the selected articles
        {
            if (articlesSelected.Count >= 1)
            {
                using SaveFileDialog saveFileDiag = new() { Filter = "PDF file|*.pdf", ValidateNames = true };
                if (saveFileDiag.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Rectangle rect = new(360, 720);
                    Document doc = new(rect, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDiag.FileName, FileMode.Create));
                    doc.Open();
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ServerManager.logo, System.Drawing.Imaging.ImageFormat.Png);
                    image.ScalePercent(20);
                    image.Alignment = Element.ALIGN_LEFT;
                    doc.Add(image);

                    PdfPTable table = new(6) { WidthPercentage = 103 };
                    table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    iTextSharp.text.Font font = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10);
                    iTextSharp.text.Font font2 = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10, iTextSharp.text.Font.BOLD);
                    table.AddCell(new Paragraph("ArticuloID", font2)); table.AddCell(new Paragraph("Detalle", font2));
                    table.AddCell(new Paragraph("Presentación", font2)); table.AddCell(new Paragraph("PrecioCompra", font2));
                    table.AddCell(new Paragraph("Stock", font2)); table.AddCell(new Paragraph("Proveedor", font2));
                    foreach (DataGridViewRow row in dgviewArticles.SelectedRows)
                    {
                        table.AddCell(new Phrase($"{dgviewArticles[row.Cells["ArticuloID"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewArticles[row.Cells["Detalle"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewArticles[row.Cells["Presentacion"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"${dgviewArticles[row.Cells["PrecioCompra"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewArticles[row.Cells["Stock"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewArticles[row.Cells["Proveedor"].ColumnIndex, row.Index].Value}", font));
                    }
                    doc.Add(table);
                    doc.Add(new Paragraph($"FECHA Y HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}") { Alignment = Element.ALIGN_CENTER });
                    doc.Close();
                    writer.Close();
                }
            }
            else { MessageBox.Show($"No se seleccionó ningún artículo", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void BtnGenBarCode (object sender, EventArgs e) //Generate pdf that contains tha barcode of the selected article
        {
            if (dgViewArticulos.SelectedRows.Count == 0) MessageBox.Show($"No se seleccionó ningún artículo",
                "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (dgViewArticulos.SelectedRows.Count == 1)
            {
                using SaveFileDialog saveFileDiag = new() { Filter = "PDF file|*.pdf", ValidateNames = true };
                if (saveFileDiag.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Rectangle rect = new(360, 720);
                    Document doc = new(rect, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDiag.FileName, FileMode.Create));
                    doc.Open();
                    iTextSharp.text.Font font = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11);
                    BarcodeWriter BCWriter = new()
                    {
                        Format = BarcodeFormat.EAN_13,
                        Options = new QrCodeEncodingOptions
                        { Width = 400, Height = 100 }
                    };
                    System.Drawing.Image image = BCWriter.Write($"{dgViewArticulos.SelectedRows[0].Cells[0].Value}");
                    iTextSharp.text.Image iItextSharpImg = iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Jpeg);
                    iItextSharpImg.Alignment = Element.ALIGN_CENTER;
                    doc.Add(iItextSharpImg);
                    doc.Add(new Paragraph($"{dgViewArticulos.SelectedRows[0].Cells[1].Value} - {dgViewArticulos.SelectedRows[0].Cells[2].Value} - "
                        + $"{dgViewArticulos.SelectedRows[0].Cells[3].Value}")
                    { Alignment = Element.ALIGN_CENTER });
                    doc.Close();
                    writer.Close();
                }
            }
            else { MessageBox.Show($"Seleccione solamente (1) un artículo", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        #endregion

        #region Suppliers_REGION
        void BtnSupplierAdd_Click (object sender, EventArgs e) //Add a new supplier
        {
            ServerManager.textBoxes = new() { txtBSupName, txtBSupCUIT, txtBSupEmail, txtBSupCellphone, txtBSupArea, txtBSupAddress };
            if (ServerManager.TxtBsNotEmpty())
            {
                Supplier newSupplier = new(txtBSupName.Text, txtBSupCUIT.Text, txtBSupEmail.Text.ToLower(), txtBSupCellphone.Text,
                    txtBSupArea.Text, txtBSupAddress.Text);
                SupplierVal validator = new();
                FluentValidation.Results.ValidationResult result = validator.Validate(newSupplier);
                if (result.IsValid)
                {
                    List<string> newSupplierData = new() { txtBSupName.Text, txtBSupCUIT.Text, txtBSupEmail.Text.ToLower(),
                        txtBSupCellphone.Text, txtBSupArea.Text, txtBSupAddress.Text };
                    string columns = "Name, CUIT, Email, Cellphone, Area, Address";
                    ServerManager.AddData("Suppliers", columns, newSupplierData, "Proveedor");
                    BtnSuppliersMenu_Click (null, null);
                }
                else
                {
                    string message = "";
                    foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                    MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        void BtnSupplierMod_Click (object sender, EventArgs e) //Modify selected supplier
        {
            if (suppliersSelected.Count == 1)
            {
                if (ServerManager.TxtBsNotEmpty())
                {
                    Supplier newSupplier = new(suppliersSelected[0].SupplierID,txtBSupName.Text, txtBSupCUIT.Text, txtBSupEmail.Text.ToLower(),txtBSupCellphone.Text,
                        txtBSupArea.Text,txtBSupAddress.Text);
                    SupplierVal validator = new();
                    FluentValidation.Results.ValidationResult result = validator.Validate(newSupplier);
                    if (result.IsValid)
                    {
                        ServerManager.textBoxes = new() { txtBSupName, txtBSupCUIT, txtBSupEmail, txtBSupCellphone, txtBSupArea, txtBSupAddress };
                        List<string> columns = new() {null, "name", "CUIT", "Email", "Cellphone", "Area","Address" };
                        List<string> newSupplierData = new() { newSupplier.SupplierID.ToString(), newSupplier.Name, txtBSupCUIT.Text, txtBSupEmail.Text.ToLower(),txtBSupCellphone.Text,
                    txtBSupArea.Text, txtBSupAddress.Text};
                        ServerManager.ModifyData("Suppliers", columns, newSupplierData, "Proveedor", "supplierid");
                        BtnSuppliersMenu_Click(null, null);
                    }
                    else
                    {
                        string message = "";
                        foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                        MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else { MessageBox.Show("Hay espacios sin llenar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            }
            else if (suppliersSelected.Count > 1) { MessageBox.Show($"Seleccione solamente un proveedor", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            else { MessageBox.Show($"No se seleccionó ningún proveedor", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void BtnSupplierDel_Click (object sender, EventArgs e) //Delete selected supplier/s
        {
            if (suppliersSelected.Count >= 1)
            {
                ServerManager.tipo = "Suppliers";
                string suppliersName = string.Empty;
                foreach (var supplierSelected in suppliersSelected) { suppliersName += $"{supplierSelected.Name}, "; }
                suppliersName += "."; suppliersName = suppliersName.Replace(", .", ".");
                DialogResult respuesta = MessageBox.Show($"Está seguro que desea eliminar los proveedores: {suppliersName}",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (respuesta == DialogResult.Yes)
                {
                    foreach (var supplier in suppliersSelected)
                    {
                        ServerManager.DeleteData("Suppliers", "SupplierID", supplier.SupplierID.ToString());
                    }
                    if (suppliersSelected.Count == 1) MessageBox.Show($"Proveedor eliminado", "Operación satisfactoria", MessageBoxButtons.OK);
                    else { MessageBox.Show($"Proveedores eliminados", "Operación satisfactoria", MessageBoxButtons.OK); }
                    BtnSuppliersMenu_Click(null, null);
                }
            }
            else { MessageBox.Show($"No se seleccionó ningún proveedor", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        void dgviewSuppliers_SelectionChanged (object sender, EventArgs e) //Update selectedSuppliers list
        {
            if (dgviewSuppliers.SelectedRows.Count == 1)
            {
                if (suppliersSelected.Count == 0)
                {
                    suppliersSelected.Add(new(int.Parse(dgviewSuppliers.SelectedRows[0].Cells["SupplierID"].Value.ToString()), dgviewSuppliers.SelectedRows[0].Cells["Name"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["CUIT"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Email"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Cellphone"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Area"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Address"].Value.ToString()));
                }
                else
                {
                    suppliersSelected.Clear();
                    suppliersSelected.Add(new());
                    suppliersSelected[0] = new(int.Parse(dgviewSuppliers.SelectedRows[0].Cells["SupplierID"].Value.ToString()),
                        dgviewSuppliers.SelectedRows[0].Cells["Name"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["CUIT"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Email"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Cellphone"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Area"].Value.ToString(),
                        dgviewSuppliers.SelectedRows[0].Cells["Address"].Value.ToString());
                }
                txtBSupName.Text = suppliersSelected[0].Name.ToString(); txtBSupCUIT.Text = suppliersSelected[0].CUIT;
                txtBSupEmail.Text = suppliersSelected[0].Email; txtBSupCellphone.Text = suppliersSelected[0].Cellphone;
                txtBSupArea.Text = suppliersSelected[0].Area; txtBSupAddress.Text = suppliersSelected[0].Address;
            }
            else if (dgviewSuppliers.SelectedRows.Count > 1)
            {
                ServerManager.CleanTxtBoxes();
                List<Supplier> newSuppliersSelected = new();
                foreach (DataGridViewRow row in dgviewSuppliers.SelectedRows)
                {
                    newSuppliersSelected.Add(new(int.Parse(row.Cells[0].Value.ToString()), row.Cells[1].Value.ToString()));
                }
                suppliersSelected = newSuppliersSelected;
            }
            else { suppliersSelected.Clear(); }
        }
        void BtnSuppliersGenList_Click (object sender, EventArgs e) //Generate a pdf that contains a lis of selected suppliers
        {
            if (suppliersSelected.Count >= 1)
            {
                using SaveFileDialog saveFileDiag = new() { FileName = "Lista_proveedores", Filter = "PDF file|*.pdf", ValidateNames = true };
                if (saveFileDiag.ShowDialog() == DialogResult.OK)
                {
                    iTextSharp.text.Rectangle rect = new(360, 720);
                    Document doc = new(rect, 10, 10, 10, 10);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDiag.FileName, FileMode.Create));
                    doc.Open();
                    //Logo image
                    iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ServerManager.logo, System.Drawing.Imaging.ImageFormat.Png);
                    image.ScalePercent(20);
                    image.Alignment = Element.ALIGN_LEFT;
                    doc.Add(image);
                    //table of selected suppliers
                    PdfPTable table = new(5) { WidthPercentage = 104 };
                    table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    iTextSharp.text.Font font = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11);
                    iTextSharp.text.Font font2 = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11, iTextSharp.text.Font.BOLD);
                    table.AddCell(new Paragraph("ProveedorID", font2)); table.AddCell(new Paragraph("Nombre", font2));
                    table.AddCell(new Paragraph("Rubro", font2)); table.AddCell(new Paragraph("Celular", font2));
                    table.AddCell(new Paragraph("Email", font2));
                    foreach (DataGridViewRow row in dgviewSuppliers.SelectedRows)
                    {
                        table.AddCell(new Phrase($"{dgviewSuppliers[row.Cells["SupplierID"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewSuppliers[row.Cells["Name"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewSuppliers[row.Cells["Area"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewSuppliers[row.Cells["Cellphone"].ColumnIndex, row.Index].Value}", font));
                        table.AddCell(new Phrase($"{dgviewSuppliers[row.Cells["Email"].ColumnIndex, row.Index].Value}", font));
                    }
                    doc.Add(table);
                    doc.Add(new Paragraph($"FECHA Y HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", font2) { Alignment = Element.ALIGN_CENTER });
                    doc.Close();
                    writer.Close();
                }
            }
            else { MessageBox.Show($"No se seleccionó ningún proveedor", "Operación fallida", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        #endregion

        #region OtherStuff
        List<User> usersSelected = new(); List<Article> articlesSelected = new(); List<Supplier> suppliersSelected = new(); // Lists of users, articles and suppliers selected.
        public FormMenu() { InitializeComponent(); }
        void FormMenu_Load (object sender, EventArgs e)
        {
            ServerManager.logo = System.Drawing.Image.FromFile($@"{ServerManager.ubicacionProyecto}\Images\Logo.png"); // Set logo's image from file
            BtnSalesMenu_Click(null,null);
            FilterInfoCollection = new(FilterCategory.VideoInputDevice);
            timer1.Interval = 1500;
            timer2.Start();
            if (ServerManager.user.Access == 1)
            {
                BtnArticlesMenu.Enabled = false;
                BtnUsersMenu.Enabled = false;
                BtnProveedoresMenu.Enabled = false;
                BtnSuperMenu.Enabled = false;
            }
            if (!File.Exists($@"{ServerManager.ubicacionProyecto}\supermarketdata.txt"))
            {
                using (StreamWriter sw = File.CreateText($@"{ServerManager.ubicacionProyecto}\supermarketdata.txt"))
                {
                    sw.WriteLine($"{ServerManager.supermarket.Nombre}");
                    sw.WriteLine($"{ServerManager.supermarket.CUIT}");
                    sw.WriteLine($"{ServerManager.supermarket.Direccion}");
                }
            }
            else { ServerManager.ReadSupermarketData(); }
        }
        void timer2_Tick (object sender, EventArgs e) { txtHora.Text = DateTime.Now.ToLongTimeString(); } // Update textHora.text every second
        void BtnSupermarketMod_Click (object sender, EventArgs e) // Modify the Supermarket's data
        {
            Supermarket newSupermarket = new(txtBSuperCUIT.Text, txtBSuperName.Text, txtBSuperAddress.Text);
            SupermarketVal validator = new();
            FluentValidation.Results.ValidationResult result = validator.Validate(newSupermarket);
            if (result.IsValid)
            {
                ServerManager.supermarket = newSupermarket;
                ServerManager.UpdateSupermarketData();
                MessageBox.Show($"Los datos del supermercado fueron modificados exitosamente", "Operación satisfactoria", MessageBoxButtons.OK);
                BtnSuperMenu_Click(null, null);
            }
            else
            {
                string message = "";
                foreach (ValidationFailure fail in result.Errors) { message += $"- {fail.PropertyName}: {fail.ErrorMessage}.".Trim('.') + "\n\n"; }
                MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        void BtnTicketsGenList_Click (object sender, EventArgs e) // Generate List of tickets bewtween two dates
        {
            using SaveFileDialog saveFileDiag = new() { Filter = "PDF file|*.pdf", ValidateNames = true };
            if (saveFileDiag.ShowDialog() == DialogResult.OK)
            {
                Document doc = new(PageSize.LETTER, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDiag.FileName, FileMode.Create));
                doc.Open();

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(ServerManager.logo, System.Drawing.Imaging.ImageFormat.Png);
                image.ScalePercent(20);
                image.Alignment = Element.ALIGN_LEFT;
                doc.Add(image);

                iTextSharp.text.Font font = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10);
                iTextSharp.text.Font font2 = new(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10, iTextSharp.text.Font.BOLD);
                doc.Add(new Paragraph($"SUPERMERCADO: {ServerManager.supermarket.Nombre}", font2) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"CUIT: {ServerManager.supermarket.CUIT}", font2) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"DIRECCIÓN: {ServerManager.supermarket.Direccion}", font2) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph("\n") { Alignment = Element.ALIGN_CENTER });

                PdfPTable table = new(3) { WidthPercentage = 103 };
                table.DefaultCell.HorizontalAlignment = Element.ALIGN_CENTER;
                using SqlConnection sqlConnection = new(ServerManager._ConnectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = new($"SELECT * FROM sales where Date between '{DTP1.Value.Date:yyyy/MM/dd}' and" +
                    $" '{DTP2.Value.Date:yyyy/MM/dd} 23:59:59'", sqlConnection);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                table.AddCell(new Phrase("TICKET", font2)); table.AddCell(new Phrase("MONTO", font2));
                table.AddCell(new Phrase("FECHA Y HORA", font2));
                float totalPrice = 0;
                while (sqlDataReader.Read())
                {
                    table.AddCell(new Phrase($"{sqlDataReader.GetValue(0)}", font));
                    table.AddCell(new Phrase($"${sqlDataReader.GetValue(1)}", font));
                    table.AddCell(new Phrase($"{sqlDataReader.GetValue(2):dd/MM/yyyy HH:mm:ss}", font));
                    totalPrice += float.Parse(sqlDataReader.GetValue(1).ToString());
                }
                doc.Add(table);
                doc.Add(new Paragraph($"MONTO TOTAL: ${totalPrice}", font2) { Alignment = Element.ALIGN_CENTER });
                doc.Add(new Paragraph($"FECHA Y HORA: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", font2) { Alignment = Element.ALIGN_CENTER });
                sqlCommand.Dispose();
                sqlDataReader.Dispose();
                doc.Close();
                writer.Close();
            }
        }
        #endregion
    }
}
