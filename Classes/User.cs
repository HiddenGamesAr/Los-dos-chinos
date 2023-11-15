using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Los_dos_chinos
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Access { get; set; }
        public string Email { get; set; }
        public string Cellphone { get; set; }
        public User(int _userID, string _name,string _password, int _access)
        {
            UserID = _userID;
            Name = _name;
            Password = _password;
            Access = _access;
        }
        public User(int _userID, string _name)
        {
            UserID = _userID;
            Name = _name;
        }
        public User(int _access = 0,string _name = null, string _password = null,
            string _email = null, string _cellphone = null)
        {
            Name = _name;
            Password = _password;
            Access = _access;
            Email = _email;
            Cellphone = _cellphone;
        }
        public User(int _userID, int _access = 0, string _name = null, string _password = null,
            string _email = null, string _cellphone = null)
        {
            UserID = _userID;
            Name = _name;
            Password = _password;
            Access = _access;
            Email = _email;
            Cellphone = _cellphone;
        }
    }
}
