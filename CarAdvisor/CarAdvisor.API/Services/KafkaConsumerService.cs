using CarAdvisor.API;
using CarAdvisor.Domain.DTOs;
using Confluent.Kafka;
using Hangfire;
using System.Text.Json;

public class KafkaConsumerService : BackgroundService
{
    private readonly ConsumerConfig _config;

    public KafkaConsumerService()
    {
        _config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "car-advisor-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    // async anahtar kelimesini kaldırdık ve Task dönüyoruz
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // BÜYÜK DEĞİŞİKLİK BURADA: Sonsuz döngüyü ayrı bir Thread'e (iş parçacığına) atıyoruz.
        // Böylece API'nin ana açılış süreci bloklanmayacak!
        Task.Run(() =>
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
            consumer.Subscribe("car-price-scraping");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Bu satır bloklayıcıdır ama artık ayrı bir odada (Thread'de) çalıştığı için sorun yok.
                    var consumeResult = consumer.Consume(stoppingToken);

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var carData = JsonSerializer.Deserialize<CarPriceRequestDto>(consumeResult.Message.Value, options);

                    if (carData != null)
                    {
                        Console.WriteLine($"[Kafka Consumer] Mesaj Yakalandı: {carData.Make} {carData.Model} ({carData.StartYear}-{carData.EndYear}) - Tip: {carData.BodyType}");

                        BackgroundJob.Enqueue<PriceScraperJob>(x =>
                            x.ScrapePriceAsync(
                                carData.CarId,
                                carData.Make,
                                carData.Model,
                                carData.StartYear,
                                carData.EndYear,
                                carData.BodyType
                            )
                        );
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Kafka dinlenirken hata oluştu: {ex.Message}");
                }
            }

            consumer.Close();
        }, stoppingToken);

        // Ana sisteme "Benim kurulumum tamam, sen API'yi açmaya devam et" mesajı veriyoruz.
        return Task.CompletedTask;
    }
}