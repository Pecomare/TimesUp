FROM mcr.microsoft.com/dotnet/sdk:5.0-focal-amd64 AS build
WORKDIR /src

COPY ["TimesUp.csproj", "./"]
RUN dotnet restore "TimesUp.csproj"
COPY . .

WORKDIR "/src/."
RUN dotnet publish "TimesUp.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimesUp.dll"]
