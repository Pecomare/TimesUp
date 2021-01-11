FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0 AS build
WORKDIR /src
COPY ["TimesUp.csproj", "./"]
RUN dotnet restore "TimesUp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TimesUp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimesUp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimesUp.dll"]
