using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace ChatServer
{
    class UserBaseDao
    {
        static public void Write(string Name, string Password)
        {
            SqlConnection sqlConnection;
            StringBuilder ConnectionString = new StringBuilder();
            ConnectionString.Append(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=");
            ConnectionString.Append(Directory.GetCurrentDirectory());
            ConnectionString.Append(@"\ServerData\Userbase.mdf;Integrated Security=True");
            sqlConnection = new SqlConnection(ConnectionString.ToString());
            sqlConnection.Open();
            SqlCommand command = new SqlCommand("INSERT INTO [Table] (Name, Password)VALUES(@Name, @Password)", sqlConnection);
            command.Parameters.AddWithValue("Name", Name);
            command.Parameters.AddWithValue("Password", Password);
            command.ExecuteNonQuery();
            if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
        }

        static public bool Find(string needName, string needPass)
        {
            SqlConnection sqlConnection;
            StringBuilder ConnectionString = new StringBuilder();
            ConnectionString.Append(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=");
            ConnectionString.Append(Directory.GetCurrentDirectory());
            ConnectionString.Append(@"\ServerData\Userbase.mdf;Integrated Security=True");
            sqlConnection = new SqlConnection(ConnectionString.ToString());
            sqlConnection.Open();
            SqlDataReader sqlReader = null;
            SqlCommand command = new SqlCommand("SELECT * FROM [Table]", sqlConnection);
            try
            {
                sqlReader = command.ExecuteReader();

                while (sqlReader.Read())
                {
                    if (Convert.ToString(sqlReader["Name"]).Trim() == needName && Convert.ToString(sqlReader["Password"]).Trim() == needPass)
                    {
                        if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
                        return true;
                    }
                }
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
                return false;
            }
            finally
            {
                if (sqlReader != null)
                    sqlReader.Close();
                if (sqlConnection != null && sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();

            }
        }
    }

}
