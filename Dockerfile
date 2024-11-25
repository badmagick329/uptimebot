FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/runtime:9.0
RUN apt-get update && apt-get install -y bash
WORKDIR /app
COPY --from=build /out .
COPY appsettings.json .
COPY .env .
ENTRYPOINT ["./UptimeBot.Console"]