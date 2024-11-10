using System.Collections.Concurrent;

namespace Ejemplo_Arquitectura
{

    public class CryptoConnectionManager
    {
        // Pool de conexiones criptográficas (simulación)
        private static readonly ConcurrentBag<ICryptoConnection> _connectionPool = new ConcurrentBag<ICryptoConnection>();

        // Cache de respuestas criptográficas (simulación)
        private static readonly ConcurrentDictionary<string, string> _cryptoCache = new ConcurrentDictionary<string, string>();

        // Maximo número de conexiones que se pueden mantener activas
        private const int MaxConnections = 10;

        // Método para obtener una conexión desde el pool o crear una nueva si es necesario
        public async Task<ICryptoConnection> GetConnectionAsync()
        {
            // Intentar obtener una conexión del pool
            if (_connectionPool.TryTake(out var connection))
            {
                return connection;
            }

            // Si el pool está vacío y no hemos alcanzado el límite, crear una nueva conexión
            if (_connectionPool.Count < MaxConnections)
            {
                connection = new CryptoConnection();
                return connection;
            }

            // Si hemos alcanzado el límite, esperar hasta que se libere una conexión
            await Task.Delay(100);  // Simulación de espera

            // Intentar nuevamente obtener una conexión
            return await GetConnectionAsync();
        }

        // Método para devolver la conexión al pool
        public void ReturnConnection(ICryptoConnection connection)
        {
            if (_connectionPool.Count < MaxConnections)
            {
                _connectionPool.Add(connection);
            }
        }

        // Método para realizar una operación criptográfica, usando cacheo y conexión al appliance
        public async Task<string> ProcessCryptoRequestAsync(string data)
        {
            // Verificar si el resultado ya está en cache
            if (_cryptoCache.ContainsKey(data))
            {
                return _cryptoCache[data];
            }

            // Obtener una conexión para el appliance criptográfico
            var connection = await GetConnectionAsync();

            try
            {
                // Simular la operación criptográfica (por ejemplo, cifrado o firma)
                var result = await connection.ProcessAsync(data);

                // Cachear el resultado para futuras solicitudes
                _cryptoCache[data] = result;

                return result;
            }
            finally
            {
                // Devolver la conexión al pool
                ReturnConnection(connection);
            }
        }
    }

    public interface ICryptoConnection
    {
        Task<string> ProcessAsync(string data);
    }

    public class CryptoConnection : ICryptoConnection
    {
        // Simula la interacción con un appliance criptográfico
        public async Task<string> ProcessAsync(string data)
        {
            // Simulación de procesamiento criptográfico (por ejemplo, cifrado)
            await Task.Delay(500); // Simulación de latencia de red
            return $"Encrypted_{data}";
        }
    }

    // Clase de ejemplo para llamar los métodos
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cryptoManager = new CryptoConnectionManager();

            // Simulación de múltiples solicitudes
            var data1 = "Transaction1";
            var data2 = "Transaction2";

            var result1 = await cryptoManager.ProcessCryptoRequestAsync(data1);
            Console.WriteLine($"Resultado para {data1}: {result1}");

            var result2 = await cryptoManager.ProcessCryptoRequestAsync(data2);
            Console.WriteLine($"Resultado para {data2}: {result2}");

            // Solicitar nuevamente el mismo dato para verificar el cacheo
            var result1Again = await cryptoManager.ProcessCryptoRequestAsync(data1);
            Console.WriteLine($"Resultado para {data1} (de nuevo, desde cache): {result1Again}");
        }
    }
}