FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Scandium/Scandium.csproj", "Scandium/"]
RUN dotnet restore "Scandium/Scandium.csproj"
COPY . .
WORKDIR "/src/Scandium"
RUN dotnet build "Scandium.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scandium.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:5000
ENTRYPOINT ["dotnet", "Scandium.dll"]
