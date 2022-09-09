#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=
EXPOSE 53/udp

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Docker.Dns/Docker.Dns.csproj", "Docker.Dns/"]
RUN dotnet restore "Docker.Dns/Docker.Dns.csproj"
COPY . .
WORKDIR "/src/Docker.Dns"
RUN dotnet build "Docker.Dns.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Docker.Dns.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Docker.Dns.dll"]