using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System;
using Dapper;

namespace EmailUploader.API.Services
{
    public interface IEmailService
    {
        Task<int> SaveUniqueEmails(List<string> emails);
    }

    public class EmailService : IEmailService
    {
        private readonly string _connectionString;

        public EmailService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public async Task<int> SaveUniqueEmails(List<string> emails)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    
                    var emailTable = new DataTable();
                    emailTable.Columns.Add("Email", typeof(string));

                    foreach (var email in emails)
                    {
                        emailTable.Rows.Add(email);
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@EmailList", emailTable.AsTableValuedParameter("dbo.EmailListType"));
                    parameters.Add("@InsertedCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    await connection.ExecuteAsync(
                        "dbo.usp_InsertUniqueEmails",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    return parameters.Get<int>("@InsertedCount");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving emails to database: " + ex.Message, ex);
            }
        }
    }
}