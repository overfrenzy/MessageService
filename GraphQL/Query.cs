using MessageService.Models;
using MessageService.DataAccess;
using Microsoft.Extensions.Logging;

namespace MessageService.GraphQL
{
    public class Query
    {
        private readonly MessageRepository _repository;
        private readonly ILogger<Query> _logger;

        public Query(MessageRepository repository, ILogger<Query> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [GraphQLName("messages")]
        public async Task<List<Message>> GetMessagesAsync(DateTime from, DateTime to)
        {
            // Проверка на корректность диапазона дат
            if (from >= to)
            {
                _logger.LogWarning("Попытка запроса сообщений с некорректным диапазоном: from {From} - to {To}.", from, to);
                throw new GraphQLException("Начальная дата должна быть меньше конечной даты.");
            }

            _logger.LogInformation("Запрос сообщений с {From} по {To}.", from, to);

            try
            {
                // Получение списка сообщений из базы данных
                var messages = await _repository.GetMessagesAsync(from, to);
                _logger.LogInformation("Получено {Count} сообщений в диапазоне от {From} до {To}.", messages.Count, from, to);
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщений с {From} по {To}.", from, to);
                throw new GraphQLException("Ошибка при получении сообщений.");
            }
        }
    }
}
