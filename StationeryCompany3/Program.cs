using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        
        DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);

        
        using (DbConnection connection = DbProviderFactories.GetFactory("System.Data.SqlClient").CreateConnection())
        {
            if (connection != null)
            {
                connection.ConnectionString = "Data Source=EUGENE1984; Initial Catalog=StationeryCompany; Integrated Security=True;";

                try
                {
                    await connection.OpenAsync();

                    Console.WriteLine("Подключение успешно!");

                   
                    await DisplaySalesManagersAsync(connection);

                    
                    await DisplayStationeryBelowCostAsync(connection, 10.0m);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения: {ex.Message}");
                }
            }
        }
    }

    static async Task DisplaySalesManagersAsync(DbConnection connection)
    {
        using (DbCommand command = connection.CreateCommand())
        {
            command.CommandText = "SELECT Managers.Manager_ID, Managers.FirstName, Managers.LastName, SUM(Sales.Quantity_Sold) AS TotalSales " +
                                  "FROM Sales " +
                                  "JOIN Managers ON Sales.Manager_ID = Managers.Manager_ID " +
                                  "GROUP BY Managers.Manager_ID, Managers.FirstName, Managers.LastName " +
                                  "ORDER BY TotalSales ASC";

            using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)command))
            {
                DataSet dataSet = new DataSet();
                await Task.Run(() => adapter.Fill(dataSet, "Sales"));

                Console.WriteLine("Менеджеры по продажам в порядке возрастания продаж:");
                foreach (DataRow row in dataSet.Tables["Sales"].Rows)
                {
                    Console.WriteLine($"Manager ID: {row["Manager_ID"]}, Name: {row["FirstName"]} {row["LastName"]}, Total Sales: {row["TotalSales"]}");
                }
            }
        }
    }

    static async Task DisplayStationeryBelowCostAsync(DbConnection connection, decimal maxCost)
    {
        using (DbCommand command = connection.CreateCommand())
        {
            command.CommandText = "SELECT * FROM Products WHERE Cost_Price < @MaxCost ORDER BY Quantity ASC";

            var maxCostParameter = command.CreateParameter();
            maxCostParameter.ParameterName = "@MaxCost";
            maxCostParameter.Value = maxCost;
            command.Parameters.Add(maxCostParameter);

            using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)command))
            {
                DataSet dataSet = new DataSet();
                await Task.Run(() => adapter.Fill(dataSet, "Products"));

                Console.WriteLine("Канцтовары ниже определенной стоимости, отсортированные по возрастанию оставшегося количества:");
                foreach (DataRow row in dataSet.Tables["Products"].Rows)
                {
                    Console.WriteLine($"Product ID: {row["Product_ID"]}, Name: {row["Product_Name"]}, Quantity: {row["Quantity"]}, Cost Price: {row["Cost_Price"]}");
                }
            }
        }
    }
}
