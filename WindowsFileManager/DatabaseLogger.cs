using System;
using System.Data.SqlClient;

public static class DatabaseLogger
{
    private static string connectionString = @"Server=MELIH\SQLEXPRESS;Database=FileLoggerDB;Trusted_Connection=True;";

    public static void Log(string actionType, string filePath, string userName, string details, string logType)
    {
        string query = "INSERT INTO dbo.FileLog (ActionType, FilePath, UserName, Details, LogType) VALUES (@ActionType, @FilePath, @UserName, @Details, @LogType)";

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ActionType", actionType);
                    command.Parameters.AddWithValue("@FilePath", filePath);
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@Details", details);
                    command.Parameters.AddWithValue("@LogType", logType);

                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            // Hata mesajını yakala ve logla veya hata için farklı bir işlem yap
            Console.WriteLine("SQL hatası: " + ex.Message);
            throw new ApplicationException("Veritabanı hatası: " + ex.Message);
        }
    }
}
