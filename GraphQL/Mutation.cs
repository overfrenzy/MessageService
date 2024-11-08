using MessageService.Models;
using MessageService.DataAccess;
using Microsoft.AspNetCore.SignalR;
using MessageService.Hubs;

namespace MessageService.GraphQL
{
    public class Mutation
    {
        private readonly MessageRepository _repository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<Mutation> _logger;

        public Mutation(MessageRepository repository, IHubContext<MessageHub> hubContext, ILogger<Mutation> logger)
        {
            _repository = repository;
            _hubContext = hubContext;
            _logger = logger;
        }

        [GraphQLName("addMessage")]
        public async Task<Message> AddMessageAsync(string text)
        {
            // Проверка на пустой текст сообщения
            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Попытка добавить сообщение с пустым текстом.");
                throw new GraphQLException("Текст сообщения не может быть пустым.");
            }

            // Создание нового сообщения с текущей меткой времени и порядковым номером
            var message = new Message
            {
                Text = text,
                Timestamp = DateTime.UtcNow,
                SequenceNumber = await _repository.GetNextSequenceNumberAsync()
            };

            _logger.LogInformation("Добавление сообщения с SequenceNumber {SequenceNumber}.", message.SequenceNumber);

            try
            {
                // Сохранение сообщения в базе данных
                await _repository.SaveMessageAsync(message);
                _logger.LogInformation("Сообщение с SequenceNumber {SequenceNumber} успешно сохранено.", message.SequenceNumber);

                // Отправка сообщения всем подключенным клиентам через SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Text, message.Timestamp, message.SequenceNumber);
                _logger.LogInformation("Сообщение с SequenceNumber {SequenceNumber} отправлено всем клиентам.", message.SequenceNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении сообщения с SequenceNumber {SequenceNumber}.", message.SequenceNumber);
                throw new GraphQLException("Ошибка при добавлении сообщения.");
            }

            return message;
        }
    }
}
