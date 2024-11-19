using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace Networking.Models
    {
        public class ClientMessage
        {
            public string Command { get; set; }  // Ejemplo: "TirarDado"
            public int PlayerId { get; set; }   // ID del jugador
            public int DiceRoll { get; set; }   // Valor del dado
        }

        public class ServerMessage
        {
            public string MessageType { get; set; } // Ejemplo: "Movimiento", "Turno"
            public string Content { get; set; }    // Mensaje o información adicional
        }
    }


