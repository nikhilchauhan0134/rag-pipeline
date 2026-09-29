using RagPipeline.Api;
using RagPipeline.History;
using RagPipeline.Rag;
using RagPipeline.VectorDb;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
var qdrantUrl = builder.Configuration["Qdrant:Url"];
var qdrantKey = builder.Configuration["Qdrant:ApiKey"];
if (!string.IsNullOrWhiteSpace(qdrantUrl) && !string.IsNullOrWhiteSpace(qdrantKey))
{
    var collection = builder.Configuration["Qdrant:Collection"] ?? "documents";
    var vectorSize = builder.Configuration.GetValue<ulong?>("Qdrant:VectorSize") ?? 8;
    builder.Services.AddSingleton<IVectorStore>(_ => new QdrantVectorStore(qdrantUrl, qdrantKey, collection, vectorSize));
}
else
{
    builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
}
builder.Services.AddSingleton<IChatHistoryStore, InMemoryChatHistoryStore>();
builder.Services.AddSingleton<IEmbeddingGenerator, HashEmbeddingGenerator>();
builder.Services.AddSingleton<IRagPipeline, RagPipelineService>();
builder.Services.AddHttpClient<IAnswerClient, GroqAnswerClient>(client =>
{
    var baseUrl = builder.Configuration["Groq:BaseUrl"] ?? "https://api.groq.com/openai/v1/";
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseCors("ui");
app.MapControllers();

app.Run();

public partial class Program;
