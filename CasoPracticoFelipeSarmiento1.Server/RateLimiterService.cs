using Npgsql;

public class RateLimiterService
{
    private readonly string _connectionString;

    public RateLimiterService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("CornConnection");
    }

    public async Task<bool> CanPurchaseCornAsync(string clientId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Comprobar si el cliente existe
            var checkCommand = new NpgsqlCommand(
                @"SELECT LastPurchaseTime FROM CornPurchaseLimits WHERE ClientId = @ClientId",
                connection
            );
            checkCommand.Parameters.AddWithValue("ClientId", clientId);

            var result = await checkCommand.ExecuteScalarAsync();

            if (result == null)
            {
                // El cliente no existe, crear un nuevo registro
                await AddClientRecordAsync(clientId);
                return true;
            }
            else
            {
                // El cliente existe, comprobar el tiempo transcurrido
                var lastPurchaseTime = Convert.ToDateTime(result);
                var timeElapsed = DateTime.UtcNow - lastPurchaseTime;

                if (timeElapsed.TotalMinutes >= 1)
                {
                    // Ha pasado más de un minuto, actualizar el registro
                    await UpdatePurchaseCountAsync(clientId);
                    return true;
                }
                else
                {
                    // No ha pasado 1 minuto, devolver error
                    return false;
                }
            }
        }
    }

    private async Task AddClientRecordAsync(string clientId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                @"INSERT INTO CornPurchaseLimits (ClientId, LastPurchaseTime, PurchaseCount)
                  VALUES (@ClientId, now(), 1)",
                connection
            );
            command.Parameters.AddWithValue("ClientId", clientId);

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task UpdatePurchaseCountAsync(string clientId)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var command = new NpgsqlCommand(
                @"UPDATE CornPurchaseLimits 
                  SET PurchaseCount = PurchaseCount + 1, LastPurchaseTime = now()
                  WHERE ClientId = @ClientId",
                connection
            );
            command.Parameters.AddWithValue("ClientId", clientId);

            await command.ExecuteNonQueryAsync();
        }
    }
}
