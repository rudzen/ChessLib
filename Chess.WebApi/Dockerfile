FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Chess.WebApi/Chess.WebApi.csproj", "Chess.WebApi/"]
RUN dotnet restore "Chess.WebApi/Chess.WebApi.csproj"
COPY . .
WORKDIR "/src/Chess.WebApi"
RUN dotnet build "Chess.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chess.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chess.WebApi.dll"]
