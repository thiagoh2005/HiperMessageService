using HiperMessageService;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var hiperMessageConnStr = "Server=localhost;Port=5433;Database=HiperMessageDB;Username=postgres;Password=1234";
await using var hiperMessageDB = NpgsqlDataSource.Create(hiperMessageConnStr);
var messageSql = @"
    INSERT INTO pedido_estoque_message (
    id,
    responsavel,
    descricao,
    item_id,
    item,
    data_entregue,
    unidades
)
VALUES (
    @Id,
    @Responsavel,
    @Descricao,
    @ItemId,
    @Item,
    @DataEntregue,
    @Unidades
);";

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.QueueDeclareAsync(queue: "receberPedidoEstoque", durable: false, exclusive: false, autoDelete: false, arguments: null);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, eventeArgs) =>
{
    var body = eventeArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var pedidoCommand = JsonSerializer.Deserialize<PedidoEstoqueMessageCommand>(message);

    Console.WriteLine($"Recebido: {message}");
    await using (var cmd = hiperMessageDB.CreateCommand(messageSql))
    {
        cmd.Parameters.AddWithValue("Id", pedidoCommand.Id);
        cmd.Parameters.AddWithValue("Responsavel", pedidoCommand.Responsavel);
        cmd.Parameters.AddWithValue("Descricao", pedidoCommand.Descricao);
        cmd.Parameters.AddWithValue("ItemId", pedidoCommand.ItemId);
        cmd.Parameters.AddWithValue("Item", pedidoCommand.Item);
        cmd.Parameters.AddWithValue("DataEntregue", pedidoCommand.DataEntregue);
        cmd.Parameters.AddWithValue("Unidades", pedidoCommand.Unidades);
        await cmd.ExecuteNonQueryAsync();
    }
    Console.WriteLine("Persistido no banco");
};

await channel.BasicConsumeAsync("receberPedidoEstoque", autoAck: true, consumer: consumer);

Console.ReadLine();