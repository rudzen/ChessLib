FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Rudzoft.ChessLib.WebApi/Rudzoft.ChessLib.WebApi.csproj", "Rudzoft.ChessLib.WebApi/"]
RUN dotnet restore "Rudzoft.ChessLib.WebApi/Rudzoft.ChessLib.WebApi.csproj"
COPY . .
WORKDIR "/src/Rudzoft.ChessLib.WebApi"
RUN dotnet build "Rudzoft.ChessLib.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rudzoft.ChessLib.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rudzoft.ChessLib.WebApi.dll"]
