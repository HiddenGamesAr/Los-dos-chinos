using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Supplier
{
    public int SupplierID { get; set; }
    public string Name { get; set; }
    public string CUIT { get; set; }
    public string Email { get; set; }
    public string Cellphone { get; set; }
    public string Area { get; set; }
    public string Address { get; set; }
    public Supplier()
    {
            
    }
    public Supplier(int supplierID, string _name)
    {
        SupplierID = supplierID;
        Name = _name;
    }
    public Supplier(string _name, string _cuit, string _email, string _cellphone, string _area, string _address)
    {
        //SupplierID = supplierID;
        Name = _name;
        CUIT = _cuit;
        Email = _email;
        Cellphone = _cellphone;
        Area = _area;
        Address = _address;
    }

    public Supplier(int supplierID, string name, string _cuit, string email, string cellphone, string area, string _address)
    {
        SupplierID = supplierID;
        Name = name;
        CUIT= _cuit;
        Email = email;
        Cellphone = cellphone;
        Area = area;
        Address = _address;
    }
}
