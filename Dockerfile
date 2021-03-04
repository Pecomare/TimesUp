FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY ["TimesUp.csproj", "./"]
RUN dotnet restore "TimesUp.csproj"
COPY . .

WORKDIR "/src/."
RUN dotnet publish "TimesUp.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0-alpine
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimesUp.dll"]
