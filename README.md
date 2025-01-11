# Rate Limiter API

## 📋 **Descripción**
Este proyecto implementa un servicio de limitación de tasa (Rate Limiter) utilizando **ASP.NET Core 8.0**, **PostgreSQL** y **React**. El sistema permite que los clientes compren maíz, pero limita la cantidad de compras a **una vez por minuto**. Si el cliente intenta realizar más compras dentro de ese tiempo, se devuelve un error **429 Too Many Requests**.

---

## 🛠 **Tecnologías Utilizadas**
- **ASP.NET Core 8.0**
- **PostgreSQL**
- **React**
- **Npgsql** (driver para PostgreSQL en .NET)

---

## 🗂 **Estructura del Proyecto**
```
RateLimiterAPI/
├── Controllers/
│   └── CornController.cs
├── Services/
│   └── RateLimiterService.cs
├── Program.cs
├── appsettings.json
├── ClientApp/
│   └── src/
│       └── App.js
└── README.md
```

---

## 🖥️ **Configuración del Backend (ASP.NET Core)**

### 📄 **CornController.cs**
Este controlador maneja la lógica para las solicitudes de compra de maíz.

```csharp
[ApiController]
[Route("[controller]")]
public class CornController : ControllerBase
{
    private readonly RateLimiterService _rateLimiterService;

    public CornController(RateLimiterService rateLimiterService)
    {
        _rateLimiterService = rateLimiterService;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseCorn([FromHeader] string clientId)
    {
        if (await _rateLimiterService.CanPurchaseCornAsync(clientId))
        {
            return Ok("Corn purchased successfully!");
        }
        else
        {
            return StatusCode(429, "Too Many Requests: Limit exceeded.");
        }
    }
}
```

### 📄 **RateLimiterService.cs**
Este servicio maneja la lógica para verificar si el cliente puede realizar una compra.

```csharp
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

            var checkCommand = new NpgsqlCommand(
                @"SELECT LastPurchaseTime FROM CornPurchaseLimits WHERE ClientId = @ClientId",
                connection
            );
            checkCommand.Parameters.AddWithValue("ClientId", clientId);

            var result = await checkCommand.ExecuteScalarAsync();

            if (result == null)
            {
                await AddClientRecordAsync(clientId);
                return true;
            }
            else
            {
                var lastPurchaseTime = Convert.ToDateTime(result);
                var timeElapsed = DateTime.UtcNow - lastPurchaseTime;

                if (timeElapsed.TotalMinutes >= 1)
                {
                    await UpdatePurchaseCountAsync(clientId);
                    return true;
                }
                else
                {
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
```

---

## 🗃️ **Base de Datos PostgreSQL**

### 📄 **Tabla: CornPurchaseLimits**
```sql
CREATE TABLE CornPurchaseLimits (
    ClientId VARCHAR(255) PRIMARY KEY,
    LastPurchaseTime TIMESTAMP NOT NULL,
    PurchaseCount INT NOT NULL
);
```

---

## 🌐 **Configuración del Frontend (React)**

### 📄 **App.js**
```javascript
import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [corn, setCorn] = useState();
    const [message, setMessage] = useState();
    const [clientId, setClientId] = useState()

    const buyCorn = async () => {
        try {
            const response = await fetch('Corn/purchase', {
                method: 'POST',
                headers: {
                    'ClientId': clientId
                }
            });

            if (response.status === 200) {
                const data = await response.text();
                setMessage(data); // Mensaje exitoso desde el backend
            } else if (response.status === 429) {
                setMessage('Too Many Requests: Limit exceeded.');
            } else {
                setMessage('An unexpected error occurred: ' + response.status);
            }
        } catch (error) {
            setMessage('An error occurred: ' + error.message + " " + error.toString());
        }
    };

    return (
        <div className="flex flex-col gap-8">
            <h1 id="tableLabel" className="text-white font-bold text-center text-5xl">Bob's Corn</h1>
            <input type="text" onChange={ ({target}) => {
                setClientId(target.value)
            } }/>
            <button onClick={ () => {
                setMessage("");
                buyCorn()
            } } className="border rounded-lg text-white p-3 font-bold border-2 border-white hover:border-green">🌽 Buy 1 Corn 🌽</button>
            <p className="text-yellow-300 text-center font-bold">You can buy 1 corn per minute</p>
            <p className="text-green-300 text-center font-bold">{message}</p>
        </div>
    );
}

export default App;

export default App;
```

---

## 🚀 **Ejecutar el Proyecto**

### 1️⃣ **Configurar la Base de Datos**
1. Crear la tabla `CornPurchaseLimits` en PostgreSQL.
2. Actualizar la cadena de conexión en `appsettings.json`.

### 2️⃣ **Ejecutar el Backend**
```bash
cd RateLimiterAPI
 dotnet run
```

### 3️⃣ **Ejecutar el Frontend**
```bash
cd ClientApp
npm start
```

---

## 🧪 **Pruebas**
- **Compra exitosa:** Si no ha comprado en el último minuto, devuelve un mensaje exitoso.
- **Límite excedido:** Si intenta comprar más de una vez en un minuto, devuelve un error 429.

---

## 📞 **Contacto**
Creado por Felipe Sarmiento.

