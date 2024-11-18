using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Database
    {
        private string connectionString = "Data Source=localhost;Initial Catalog=LudoDB";

        // Método para obtener la lista de jugadores desde la base de datos
        public List<Player> LoadPlayers()
        {
            List<Player> players = new List<Player>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Players", conn);
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

        // Método para guardar el estado del juego en la base de datos
        public void SaveGameState(int playerId, string tokenPosition)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Players SET TokenPosition = @position WHERE Id = @playerId", conn);
                cmd.Parameters.AddWithValue("@position", tokenPosition);
                cmd.Parameters.AddWithValue("@playerId", playerId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
