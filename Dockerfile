FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS baseimage
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS buildproject
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BPCalculator/BPCalculator.csproj", "BPCalculator/"]
RUN dotnet restore "./BPCalculator/BPCalculator.csproj"
COPY . .
WORKDIR "/src/BPCalculator"
RUN dotnet build "./BPCalculator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM buildproject AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BPCalculator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM baseimage AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BPCalculator.dll"]