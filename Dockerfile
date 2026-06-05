FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DanialNetAccount.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build "DanialNetAccount.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DanialNetAccount.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DanialNetAccount.dll"]
