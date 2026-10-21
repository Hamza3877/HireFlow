# Build from the repository root:  docker build -f src/HireFlow.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/HireFlow.Api/HireFlow.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
# Run as a normal user, not root.
RUN adduser --disabled-password --gecos "" appuser \
    && mkdir -p /app/cv-files /app/logs \
    && chown -R appuser /app
USER appuser
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HireFlow.Api.dll"]
