FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShopService.Api/ShopService.Api.csproj", "ShopService.Api/"]
COPY ["ShopService.Infrastructure/ShopService.Infrastructure.csproj", "ShopService.Infrastructure/"]
COPY ["ShopService.Core/ShopService.Core.csproj", "ShopService.Core/"]
COPY ["ShopService.Shared/ShopService.Shared.csproj", "ShopService.Shared/"]
COPY ["ShopService.Application/ShopService.Application.csproj", "ShopService.Application/"]
RUN dotnet restore "ShopService.Api/ShopService.Api.csproj"
COPY . .
WORKDIR "/src/ShopService.Api"
RUN dotnet build "ShopService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ShopService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopService.Api.dll"]
