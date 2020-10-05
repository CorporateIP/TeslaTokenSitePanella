#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TeslaKey/TeslaKey.csproj", "TeslaKey/"]
RUN dotnet restore "TeslaKey/TeslaKey.csproj"
COPY . .
WORKDIR "/src/TeslaKey"
RUN dotnet build "TeslaKey.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TeslaKey.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=123bert
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=data/certificate.pfx
ENV ASPNETCORE_URLS=https://+;http://+
ENTRYPOINT ["dotnet", "TeslaKey.dll"]
