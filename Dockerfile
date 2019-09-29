FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["src/BeautifulRestApi.csproj", "src/"]
RUN dotnet restore "src/BeautifulRestApi.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "BeautifulRestApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BeautifulRestApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BeautifulRestApi.dll"]
