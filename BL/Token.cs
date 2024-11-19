using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class Token
    {
        public bool IsInHome { get; set; } = true; // Por defecto, la ficha empieza en casa.
        public int Position { get; set; } = 0;    // Posición inicial.
        public bool IsActive { get; set; } = true;
        public Player Player { get; set; } // Nuevo: Asignar el propietario de la ficha

        public Token(Player player)
        {
            Player = player;
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
