using Microsoft.Extensions.DependencyInjection;
using Dadata;
using Dadata.Model;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder();
builder.Services.AddCors(); // добавляем Cors 
var app = builder.Build();

//создание httpClient через httpClientFactory 
var services = new ServiceCollection();
services.AddHttpClient();
var serviceProvider = services.BuildServiceProvider();
var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
var httpClient = httpClientFactory?.CreateClient();

// задаём данные для api dadata
var token = "927777ed1d86fc93ea4f121c89e662d7174b3d21";
var secret = "caac29a684135a2279bda141485fc222258a2b8e";
var api = new CleanClientAsync(token, secret);

app.UseHttpLogging(); //включаем базовое ведение журнала http
app.UseCors(builder => builder.AllowAnyOrigin()); //задаём политику для Cors 
app.UseHttpsRedirection(); //подключаем редирект на https

app.Run(async (context) =>
{
    
    context.Response.ContentType = "text/html; charset=utf-8";

    if (context.Request.Path == "/getaddress")
    {
        
        string getSource = "https://localhost:7210";//сюда идёт url страницы, с которой будет браться сырая запись адреса
        if(httpClient != null)
        {
            string address = await httpClient.GetStringAsync(getSource);
            if (address != null && address != "")
            {
                var fixedAddress = await api.Clean<Address>(address);//используем dadata, чтобы стандартизировать адрес

                //конвертируем адрес в json
                var settings = new JsonSerializerSettings { };
                string addressJson = JsonConvert.SerializeObject(fixedAddress, settings);

                await context.Response.WriteAsync($"<div><p>{addressJson}</p></div>"); //выводим json
            }
            else
            {
                app.Logger.LogError("source page is empty");
            }
        }
        else
        {
            app.Logger.LogError("httpClient is null");
        }
        
       


    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }

});

app.Run();