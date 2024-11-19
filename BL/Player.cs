using System;
using System.Collections.Generic;
using System.Linq;

namespace BL
{
    public class Player
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Token> Tokens { get; private set; } = new List<Token>();

        public bool SkipTurn { get; set; } = false;
        public int CurrentPosition { get; set; }

        public int Id { get; set; }
        public int PlayerId { get; set; }

        public Player(string name, string color, int tokenCount = 1)
        {
            Name = name;
            Color = color;

            // Crear las fichas para el jugador
            for (int i = 0; i < 1; i++)
            {
                Tokens.Add(new Token(this));
            }
        }


        // Verifica si todas las fichas están en casa
        public bool AllTokensInHome()
        {
            return Tokens.All(t => t.IsInHome);
        }

        // Selecciona una ficha para mover
        public Token SelectTokenToMove(int diceRoll)
        {
            // Si hay fichas fuera de casa que puedan moverse
            Token token = Tokens.FirstOrDefault(t => !t.IsInHome && t.Position + diceRoll <= 100);

            if (token == null)
            {
                Console.WriteLine($"{Name} no tiene fichas disponibles para mover con el dado: {diceRoll}.");
            }

            return token;
        }


        public bool HasTokensInHome()
        {
            return Tokens.Any(t => t.IsInHome);
        }

        public Token ReleaseTokenFromHome()
        {
            Token token = Tokens.FirstOrDefault(t => t.IsInHome);
            if (token != null)
            {
                token.Position = 1; // Sacar a la primera casilla
                token.IsInHome = false;
            }
            return token; // Devolver la ficha que fue sacada de la casa
        }



        public bool HasTokenAtPosition(int position)
        {
            bool result = Tokens.Any(t => t.Position == position && !t.IsInHome);
            Console.WriteLine(result ? $"Hay una ficha en la posición {position}" : $"No hay fichas en la posición {position}");
            return result;
        }

        public void SendTokenToHome(int position)
        {
            Token token = Tokens.FirstOrDefault(t => t.Position == position && !t.IsInHome);
            if (token != null)
            {
                Console.WriteLine($"La ficha en la posición {position} fue enviada a casa.");
                token.ReturnToHome();
            }
            else
            {
                Console.WriteLine($"No se encontró ninguna ficha para enviar a casa en la posición {position}.");
            }
        }


        public bool HasWon()
        {
            return Tokens.All(t => t.Position == 100);
        }


        public Token GetTokenToMove(int diceRoll)
        {
            // Retorna una ficha que pueda moverse o null si no hay fichas disponibles
            Token token = Tokens.FirstOrDefault(t => !t.IsInHome && t.Position + diceRoll <= 100);

            if (token == null)
            {
                Console.WriteLine($"{Name} no tiene fichas para mover con el dado: {diceRoll}.");
            }

            return token;
        }

    }


}
