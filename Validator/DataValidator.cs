using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Los_dos_chinos.Validator
{
    public static class CUITVal
    {
        public static bool CuitValido(string x)
        {
            if (x.Length == 13)
            {
                int numSumados = 0;
                List<int> valoresMult = new() { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                if (x.Contains("-")) { x = x.Replace("-", string.Empty); } // Si tiene guiones, se sacan
                if (x.Length == 11 && long.TryParse(x, out _)) // Se comprueba si la string es un conjunto de numeros
                {
                    for (int i = 0; i < valoresMult.Count; i++) // Se multiplican los valores del cuit por los valores de "valoresMult"
                    {
                        int valorDeCUIT = int.Parse(x[i].ToString());
                        numSumados += valorDeCUIT * valoresMult[i];
                    }
                    if ((11 - (numSumados % 11)) == int.Parse(x[10].ToString())) { return true; } // Se comprueba que el resto de la división de el último númeor del CUIT
                    else { return false; } //El cuit no es válido
                }
                else { return false; } // La string no contiene únicamente números
            }
            else { return false; } // La string no llega a 13 digitos
        }
    }
    public class SupermarketVal : AbstractValidator<Supermarket>
    {
        public SupermarketVal()
        {
            RuleFor(a => a.Nombre).NotNull().NotEmpty().MinimumLength(3);
            RuleFor(a => a.CUIT).NotNull().NotEmpty().Must(x => CUITVal.CuitValido(x)).WithMessage("CUIT no valido");
            RuleFor(a => a.Direccion).NotNull().NotEmpty().MinimumLength(6);
        }
    }
    public class ArticleVal : AbstractValidator<Article>
    {
        public ArticleVal()
        {
            RuleFor(a => a.ArticuloID).Must(x=> x.ToString().Length == 13).WithMessage("El número ingresado no tiene el formato \"EAN-13\"");
            RuleFor(a => a.Detalle).NotNull().NotEmpty().MinimumLength(2);
            RuleFor(a => a.Presentacion).NotNull().NotEmpty().MinimumLength(2);
            RuleFor(a => a.Stock).NotNull().NotEmpty();
            RuleFor(a => a.PrecioCompra).Must(x => float.TryParse(
                x.ToString(), out var val)).GreaterThanOrEqualTo(0);
            RuleFor(a => a.PrecioVenta).Must(x => float.TryParse(
                x.ToString(), out var val)).GreaterThanOrEqualTo(0);
            RuleFor(a => a.Stock).Must(x => int.TryParse(
                x.ToString(), out var val)).GreaterThanOrEqualTo(0);
            RuleFor(a => a.Proveedor).NotNull().NotEmpty().MinimumLength(2);
        }
    }
    public class UserVal : AbstractValidator<User>
    {
        public UserVal()
        {
            RuleFor(a => a.Email).EmailAddress();
            RuleFor(a => a.Access).NotNull().NotEmpty().InclusiveBetween(0,1);
            RuleFor(a => a.Cellphone).NotNull().NotEmpty().Length(6,20);
            RuleFor(a => a.Password).NotNull().NotEmpty().MinimumLength(8);
            RuleFor(a => a.Name).NotNull().NotEmpty().MinimumLength(3);
        }
    }
    public class SupplierVal : AbstractValidator<Supplier>
    {
        public SupplierVal()
        {
            RuleFor(a => a.Name).NotNull().NotEmpty().MinimumLength(3);
            RuleFor(a => a.CUIT).NotNull().NotEmpty().Must(x=> CUITVal.CuitValido(x)).WithMessage("CUIT no valido");
            RuleFor(a => a.Email).EmailAddress();
            RuleFor(a => a.Cellphone).NotNull().NotEmpty().Length(6, 20);
            RuleFor(a => a.Area).NotNull().NotEmpty().MinimumLength(4);
            RuleFor(a => a.Address).NotNull().NotEmpty().MinimumLength(6);
        }
    }
}
