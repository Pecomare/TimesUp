FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TimesUp.csproj", "./"]
RUN dotnet restore "TimesUp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TimesUp.csproj" -c Release -o /app/build

RUN dotnet publish "TimesUp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TimesUp.dll"]
