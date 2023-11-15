using Los_dos_chinos.OtherForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Los_dos_chinos
{
    public partial class FormLogIn : Form
    {
        public FormLogIn() { InitializeComponent(); ServerManager.formLogIn = this; }
        User validUser = null;
        public List<User> _UsersList { get; set; } = new();
        public bool LogIn (string userName, string password)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ServerManager._ConnectionString))
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new ("SELECT * FROM users",sqlConnection);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                while (sqlDataReader.Read())
                {
                    _UsersList.Add(new User((int)sqlDataReader.GetValue(0),
                        (string)sqlDataReader.GetValue(1), (string)sqlDataReader.GetValue(2), (int)sqlDataReader.GetValue(3)));
                }
                sqlCommand.Dispose();
                sqlDataReader.Dispose();
            }
            validUser = _UsersList.FirstOrDefault(user => user.Name == userName && user.Password == password);
            if(validUser != null) { return true; } else { return false; }
        }
        private void BtnIngresar_Click (object sender, EventArgs e)
        {
            if (LogIn (txtBoxUsuario.Text, txtBoxContra.Text))
            {
                FormMenu formMenu = new();
                ServerManager.user.Access = validUser.Access; Width = formMenu.Width; Height = formMenu.Height;
                CenterToScreen();
                ServerManager.LoadFormInPanel(formMenu, panel1);
                ServerManager.session = new (validUser.UserID, DateTime.Now.Date, DateTime.Now.ToLongTimeString());
            }
            else
            {
                MessageBox.Show("El usuario ingresado no existe", "Usuario no encontrado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void ShowNewLogIn ()
        {
            FormLogIn formLogIn = new();
            Hide();
            formLogIn.ShowDialog();
            Close();
        }
        private void BtnSalir_Click (object sender, EventArgs e) { Application.Exit(); }
    }
}
