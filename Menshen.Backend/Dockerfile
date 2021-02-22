FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source

COPY Menshen.Backend.csproj .
RUN dotnet restore "./Menshen.Backend.csproj"

COPY . .
RUN dotnet publish -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine
WORKDIR /app
COPY --from=build /app ./

RUN apk add --no-cache tzdata

ENTRYPOINT ["dotnet", "Menshen.Backend.dll"]
