#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AkkaBlazorApp/AkkaBlazorApp.csproj", "AkkaBlazorApp/"]
RUN dotnet restore "AkkaBlazorApp/AkkaBlazorApp.csproj"
COPY . .
WORKDIR "/src/AkkaBlazorApp"
RUN dotnet build "AkkaBlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AkkaBlazorApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AkkaBlazorApp.dll"]