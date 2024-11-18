using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class Token
    {
        public int Position { get; set; }
        public bool IsInHome { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public Player Owner { get; set; } // Nuevo: Asignar el propietario de la ficha

        public Token(Player owner)
        {
            Owner = owner;
            Position = 0; // Inicia en casa
        }

        public void ReturnToHome()
        {
            Position = 0;
            IsInHome = true;
            IsActive = true;
        }
    }

}
