using MySql.Data.MySqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace loginapi.Repository
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public int? AuthenticateUser(string username, string password)
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                string encryptedPassword = EncryptPassword(password);
                connection.Open();
                using (var command = new MySqlCommand("sp_AuthenticateUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@p_username", username);
                    command.Parameters.AddWithValue("@p_password", encryptedPassword);

                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            finally
            {
                connection.Close();
            }
                
            
        }

        public bool RegisterUser(string username, string password)
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            try
            {
                string encryptedPassword = EncryptPassword(password);
                connection.Open();
                //string query = "INSERT INTO users(Username,Password) VALUES (@Username,@Password)";
                MySqlCommand command = new MySqlCommand("sp_RegisterUser", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@p_username", username);
                command.Parameters.AddWithValue("@p_password", encryptedPassword);
                int rowAffected = command.ExecuteNonQuery();
                return rowAffected > 0;
            }
            finally
            {
                connection.Close ();
            }

        }

        private string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
