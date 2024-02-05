#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM node:latest AS node_base

RUN echo "NODE Version:" && node --version
RUN echo "NPM Version:" && npm --version


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY --from=node_base . .

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
COPY --from=node_base . .

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["directory.build.props", "."]
COPY ["Rdmp.Web.Server/Rdmp.Web.Server.csproj", "Rdmp.Web.Server/"]
COPY ["Rdmp.Core/Rdmp.Core.csproj", "Rdmp.Core/"]
COPY ["Rdmp.React.UI/Rdmp.React.UI.esproj", "Rdmp.React.UI/"]
RUN dotnet restore "./Rdmp.Web.Server/./Rdmp.Web.Server.csproj"
COPY . .
WORKDIR "/src/Rdmp.Web.Server"
RUN dotnet build "./Rdmp.Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Rdmp.Web.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rdmp.Web.Server.dll"]