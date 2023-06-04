FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY .. .
RUN dotnet restore "Nowy.Database.Web/Nowy.Database.Web.csproj"
WORKDIR "/src/Nowy.Database.Web"
RUN dotnet build "Nowy.Database.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nowy.Database.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nowy.Database.Web.dll"]
