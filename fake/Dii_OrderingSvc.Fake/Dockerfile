#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["fake/Dii_OrderingSvc.Fake/Dii_OrderingSvc.Fake.csproj", "fake/Dii_OrderingSvc.Fake/"]
COPY ["tobenuget/Common/Common.csproj", "tobenuget/Common/"]
RUN dotnet restore "fake/Dii_OrderingSvc.Fake/Dii_OrderingSvc.Fake.csproj"
COPY . .
WORKDIR "/src/fake/Dii_OrderingSvc.Fake"
RUN dotnet build "Dii_OrderingSvc.Fake.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dii_OrderingSvc.Fake.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dii_OrderingSvc.Fake.dll"]
