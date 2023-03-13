FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-preview9 AS build
WORKDIR /src
COPY xxxx.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

WORKDIR /app
ENTRYPOINT ["dotnet", "xxxx.dll"]
