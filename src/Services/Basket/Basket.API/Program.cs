
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Discount.Grpc.Protos;
using MassTransit;

namespace Basket.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Redis Configuration
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
            });

            // General Configuration
            builder.Services.AddScoped<IBasketRepository, BasketRepository>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Grpc Configuration
            builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o => o.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]));
            builder.Services.AddScoped<DiscountGrpcService>();

            builder.Services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
                });
            });



            // builder.Services.AddMassTransitHostedService(); .NET7 doesn't need this line.

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}