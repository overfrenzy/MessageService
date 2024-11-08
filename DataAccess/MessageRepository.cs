using MessageService.Models;
using Npgsql;

namespace MessageService.DataAccess
{
    public class MessageRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(string connectionString, ILogger<MessageRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task SaveMessageAsync(Message message)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "INSERT INTO messages (text, timestamp, sequencenumber) VALUES (@text, @timestamp, @sequencenumber)";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("text", message.Text);
                command.Parameters.AddWithValue("timestamp", message.Timestamp);
                command.Parameters.AddWithValue("sequencenumber", message.SequenceNumber);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении сообщения с SequenceNumber {SequenceNumber}.", message.SequenceNumber);
                throw;
            }
        }

        public async Task<List<Message>> GetMessagesAsync(DateTime from, DateTime to)
        {
            var messages = new List<Message>();
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT text, timestamp, sequencenumber FROM messages WHERE timestamp BETWEEN @from AND @to";
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("from", from);
                command.Parameters.AddWithValue("to", to);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var message = new Message
                    {
                        Text = reader.GetString(0),
                        Timestamp = reader.GetDateTime(1),
                        SequenceNumber = reader.GetInt32(2)
                    };
                    messages.Add(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщений в диапазоне от {From} до {To}.", from, to);
                throw;
            }
            return messages;
        }

        public async Task<int> GetNextSequenceNumberAsync()
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "SELECT COALESCE(MAX(sequencenumber), 0) + 1 FROM messages";
                using var command = new NpgsqlCommand(query, connection);

                var result = await command.ExecuteScalarAsync();
                var nextSequenceNumber = Convert.ToInt32(result);
                return nextSequenceNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении следующего SequenceNumber.");
                throw;
            }
        }
    }
}
