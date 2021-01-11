FROM mcr.microsoft.com/dotnet/nightly/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/nightly/sdk:6.0-buster-slim-amd64 AS build
WORKDIR /src
COPY ["TimesUp.csproj", "./"]
RUN dotnet restore "TimesUp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "TimesUp.csproj" -c Release -o /app/build

FROM build AS publish
ARG TARGETPLATFORM
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
		RID=linux-x64 ; \
	elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
		RID=linux-arm64 ; \
	elif [ "$TARGETPLATFORM" = "linux-arm" ]; then \
		RID=linux-arm ; \
	fi \
	&& dotnet publish "TimesUp.csproj" -c Release -o /app/publish -r $RID

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimesUp.dll"]
