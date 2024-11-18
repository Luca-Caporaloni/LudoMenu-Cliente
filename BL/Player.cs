using System.Collections.Generic;
using System.Linq;

namespace BL
{
    public class Player
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public List<Token> Tokens { get; private set; }

        public bool SkipTurn { get; set; } = false;
        public int CurrentPosition { get; set; }

        public Player(string name, string color)
        {
            Name = name;
            Color = color;
            Tokens = new List<Token>
        {
            new Token(this)
        };
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
            return Tokens.FirstOrDefault(t => !t.IsInHome && t.Position + diceRoll <= 100);
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
            return Tokens.Any(t => t.Position == position && !t.IsInHome);
        }

        public void SendTokenToHome(int position)
        {
            Token token = Tokens.FirstOrDefault(t => t.Position == position && !t.IsInHome);
            if (token != null)
            {
                token.ReturnToHome();
            }
        }

        public bool HasWon()
        {
            return Tokens.All(t => t.Position == 100);
        }
    

        public Token GetTokenToMove(int diceRoll)
        {
            // Selecciona una ficha que pueda moverse
            return Tokens.FirstOrDefault(t => !t.IsInHome && (t.Position + diceRoll <= 100));
        }
    }


}
