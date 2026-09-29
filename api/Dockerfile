FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY src/ src/
RUN dotnet publish src/RagPipeline.Api/RagPipeline.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8000
EXPOSE 8000

ENTRYPOINT ["dotnet", "RagPipeline.WebHost.dll"]
