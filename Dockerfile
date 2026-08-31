FROM node:22-alpine AS web
WORKDIR /web
COPY src/AlgoLab.Api/web/package.json src/AlgoLab.Api/web/package-lock.json ./
RUN npm ci
COPY src/AlgoLab.Api/web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY global.json Directory.Build.props ./
COPY src/ src/
RUN dotnet publish src/AlgoLab.Api/AlgoLab.Api.csproj -c Release -o /app/publish
COPY --from=web /web/dist /app/publish/wwwroot

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AlgoLab.Api.dll"]
