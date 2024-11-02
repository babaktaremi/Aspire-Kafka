using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var kafkaService = builder.AddKafka("kafka-testing").WithDataVolume("kafka-volume").WithKafkaUI();

builder.AddProject<Projects.Aspire_Kafka_Api>("kafkaApi")
    .WithReference(kafkaService)
    .WithExternalHttpEndpoints();

builder.Build().Run();