using Aspire.Kafka.Api.Consumers;
using Aspire.Kafka.Api.MessageModels;
using Aspire.Kafka.ServiceDefaults;
using Confluent.Kafka;
using MassTransit;
using MassTransit.KafkaIntegration.Serializers;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var kafkaConnection = builder.Configuration.GetConnectionString("kafka-testing");

Console.WriteLine($"Using Kafka Connection: {kafkaConnection}");


builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();

    x.AddRider(rider =>
    {
        rider.AddConsumer<UserLoggedInConsumer>();

        rider.UsingKafka((context, configurator) =>
        {
            configurator.Host(kafkaConnection);

            configurator.TopicEndpoint<UserLoggedInMessageModel>("user-logged-in-topic", "user-logged-in-group-id",
                endpointConfigurator =>
                {
                    endpointConfigurator.AutoOffsetReset = AutoOffsetReset.Earliest;
                    endpointConfigurator.AutoStart = true;
                    endpointConfigurator.CreateIfMissing();
                    endpointConfigurator.SetValueDeserializer(
                        new MassTransitJsonDeserializer<UserLoggedInMessageModel>());
                    endpointConfigurator.ConfigureConsumer<UserLoggedInConsumer>(context);
                });
        });

        rider.AddProducer<UserLoggedInMessageModel>("user-logged-in-topic",
            (_, configurator) =>
            {
                configurator.SetValueSerializer(new MassTransitJsonSerializer<UserLoggedInMessageModel>());
            });
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();


app.MapPost("/UserLoggedIn", async (ITopicProducer<UserLoggedInMessageModel> producer) =>
{
    var messageModel = new UserLoggedInMessageModel() { UserId = Guid.NewGuid() };

    await producer.Produce(messageModel);

    return TypedResults.Ok(messageModel);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}