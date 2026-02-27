using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using PluginManager;
using System.Data;

namespace RetailPro2_X.BL
{
    internal class ADO
    {
        public static List<T> ReadAsync<T>(string query) where T : new()
        {

            using OracleConnection conn = new(GlobalVariables.OracleConnectionString.ToString());
            try
            {
                // Open the connection
                conn.Open();

                using (var cmd = new OracleCommand(query, conn))
                {
                    using (var adapter = new OracleDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        var dataJson = JsonConvert.SerializeObject(dataTable);
                        var dataSet = JsonConvert.DeserializeObject<List<T>>(dataJson)?.ToList() ?? [];
                        return dataSet;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
