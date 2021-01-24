using System; 
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SsdWebApi.Models;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SsdWebApi
{
    public class Persistence
    {

        private readonly IndexContext _context;
        public Persistence(IndexContext context)
        {
            _context = context;
        }

        public string readByIndex(int id)
        {
            List<string> series = new List<string>();
            string[] indexes = new string[]{"id","Data", "SP_500", "FTSE_MIB_", "GOLD_SPOT", "MSCI_EM", "MSCI_EURO", "All_Bonds", "Us_Treasury"};
            string attribute = indexes[id];

            StreamWriter fout = new StreamWriter($"Models/"+attribute+".csv",false);

            series.Add(attribute);
            fout.WriteLine(attribute);

            using(var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = $"SELECT {attribute} FROM indici";
                _context.Database.OpenConnection();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        fout.WriteLine(reader[attribute].ToString().Replace(",","."));
                        series.Add(reader[attribute].ToString());
                    }
                }
            }
            _context.Database.CloseConnection();
            fout.Close();
            Forecast f = new Forecast();
            //series.Add(f.forecastSARIMAindex(indexes[id]));
            return "{"+f.forecastSARIMAindex(indexes[id])+"}";
        }

        public void indicesDB()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            //Use DB in project directory.  If it does not exist, create it:
            connectionStringBuilder.DataSource = "./finindices.sqlite";
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                //Read data:
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT SP_500 FROM indici";
                using (var reader = selectCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {   
                        double sp500 = Convert.ToDouble(reader.GetString(0));
                        Console.WriteLine(sp500);
                    }
                }   
            }
        }
    }
}