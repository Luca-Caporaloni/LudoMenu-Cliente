using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Database
    {
        string connectionString = "Data Source=localhost;Initial Catalog=LudoDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";


        // Método para obtener la lista de jugadores desde la base de datos
        public List<Player> LoadPlayers()
        {
            List<Player> players = new List<Player>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Players2", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    players.Add(new Player
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Color = reader.GetString(2)
                    });
                }
            }

            return players;
        }

        public void AgregarJugador(int playerId, string name, string color, int sessionId, bool isOnline)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Players2 (PlayerID, Name, Color, SessionID, IsOnline) " +
                               "VALUES (@PlayerID, @Name, @Color, @SessionID, @IsOnline)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerID", playerId);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Color", color);
                    command.Parameters.AddWithValue("@SessionID", sessionId);
                    command.Parameters.AddWithValue("@IsOnline", isOnline);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void ActualizarJugador(int playerId, int sessionId, bool isOnline)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Players2 SET SessionID = @SessionID, IsOnline = @IsOnline WHERE PlayerID = @PlayerID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerID", playerId);
                    command.Parameters.AddWithValue("@SessionID", sessionId);
                    command.Parameters.AddWithValue("@IsOnline", isOnline);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void ObtenerJugador(int playerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Name, Color, SessionID, IsOnline FROM Players2 WHERE PlayerID = @PlayerID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerID", playerId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string color = reader.GetString(1);
                        int sessionId = reader.GetInt32(2);
                        bool isOnline = reader.GetBoolean(3);

                        Console.WriteLine($"Jugador: {name}, Color: {color}, SessionID: {sessionId}, Conectado: {isOnline}");
                    }
                    else
                    {
                        Console.WriteLine("Jugador no encontrado.");
                    }
                }
            }
        }

        public void AgregarJuego(int gameId, int sessionId, int currentTurn, bool isGameOver)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Games (GameID, SessionID, CurrentTurn, IsGameOver) " +
                               "VALUES (@GameID, @SessionID, @CurrentTurn, @IsGameOver)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GameID", gameId);
                    command.Parameters.AddWithValue("@SessionID", sessionId);
                    command.Parameters.AddWithValue("@CurrentTurn", currentTurn);
                    command.Parameters.AddWithValue("@IsGameOver", isGameOver);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ActualizarTurno(int gameId, int currentTurn)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Games SET CurrentTurn = @CurrentTurn WHERE GameID = @GameID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GameID", gameId);
                    command.Parameters.AddWithValue("@CurrentTurn", currentTurn);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ObtenerJuego(int gameId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT CurrentTurn, IsGameOver FROM Games WHERE GameID = @GameID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GameID", gameId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int currentTurn = reader.GetInt32(0);
                        bool isGameOver = reader.GetBoolean(1);

                        Console.WriteLine($"JuegoID: {gameId}, Turno Actual: {currentTurn}, ¿Juego Terminado?: {isGameOver}");
                    }
                    else
                    {
                        Console.WriteLine("Juego no encontrado.");
                    }
                }
            }
        }


        public void AgregarToken(int tokenId, int playerId, int position, bool isInHome)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Tokens (TokenID, PlayerID, Position, IsInHome) " +
                               "VALUES (@TokenID, @PlayerID, @Position, @IsInHome)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TokenID", tokenId);
                    command.Parameters.AddWithValue("@PlayerID", playerId);
                    command.Parameters.AddWithValue("@Position", position);
                    command.Parameters.AddWithValue("@IsInHome", isInHome);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ActualizarToken(int tokenId, int position, bool isInHome)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Tokens SET Position = @Position, IsInHome = @IsInHome WHERE TokenID = @TokenID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TokenID", tokenId);
                    command.Parameters.AddWithValue("@Position", position);
                    command.Parameters.AddWithValue("@IsInHome", isInHome);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ObtenerTokensDeJugador(int playerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT TokenID, Position, IsInHome FROM Tokens WHERE PlayerID = @PlayerID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerID", playerId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int tokenId = reader.GetInt32(0);
                        int position = reader.GetInt32(1);
                        bool isInHome = reader.GetBoolean(2);

                        Console.WriteLine($"TokenID: {tokenId}, Posición: {position}, En Casa: {isInHome}");
                    }
                }
            }
        }


        public void AgregarJugadorAGame(int playerId, int gameId, bool muyTurn, int currentPos)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO PlayerGame (PlayerID, GameID, MuyTurn, CurrentPos) " +
                               "VALUES (@PlayerID, @GameID, @MuyTurn, @CurrentPos)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerID", playerId);
                    command.Parameters.AddWithValue("@GameID", gameId);
                    command.Parameters.AddWithValue("@MuyTurn", muyTurn);
                    command.Parameters.AddWithValue("@CurrentPos", currentPos);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ActualizarTurnoYPosicion(int playerGameId, bool muyTurn, int currentPos)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE PlayerGame SET MuyTurn = @MuyTurn, CurrentPos = @CurrentPos " +
                               "WHERE PlayerGameID = @PlayerGameID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerGameID", playerGameId);
                    command.Parameters.AddWithValue("@MuyTurn", muyTurn);
                    command.Parameters.AddWithValue("@CurrentPos", currentPos);

                    command.ExecuteNonQuery();
                }
            }
        }


        public void ObtenerEstadoJugadorEnJuego(int playerGameId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT MuyTurn, CurrentPos FROM PlayerGame WHERE PlayerGameID = @PlayerGameID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PlayerGameID", playerGameId);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        bool muyTurn = reader.GetBoolean(0);
                        int currentPos = reader.GetInt32(1);

                        Console.WriteLine($"Turno: {muyTurn}, Posición: {currentPos}");
                    }
                    else
                    {
                        Console.WriteLine("Jugador no encontrado en este juego.");
                    }
                }
            }
        }



        // Método para guardar el estado del juego en la base de datos
        public void SaveGameState(int playerId, string tokenPosition)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Players2 SET TokenPosition = @position WHERE Id = @playerId", conn);
                cmd.Parameters.AddWithValue("@position", tokenPosition);
                cmd.Parameters.AddWithValue("@playerId", playerId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
