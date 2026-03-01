# ── Stage 1: base runtime image ───────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# ── Stage 2: build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем .csproj файлы отдельно для кэширования слоя restore
COPY ["src/API/API.csproj",                  "src/API/"]
COPY ["src/Application/Application.csproj",  "src/Application/"]
COPY ["src/Domain/Domain.csproj",            "src/Domain/"]
COPY ["src/Persistence/Persistence.csproj",  "src/Persistence/"]

RUN dotnet restore "src/API/API.csproj"

# Копируем весь исходный код и собираем
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "API.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# ── Stage 3: test ─────────────────────────────────────────────────────────────
# Наследуется от build, поэтому исходники уже скопированы
FROM build AS test
WORKDIR /src

COPY ["tests/Application.UnitTests/Application.UnitTests.csproj", \
      "tests/Application.UnitTests/"]
RUN dotnet restore "tests/Application.UnitTests/Application.UnitTests.csproj"

RUN dotnet test "tests/Application.UnitTests/Application.UnitTests.csproj" \
    --no-restore \
    --logger "console;verbosity=normal" \
    --configuration Release

# ── Stage 4: publish ──────────────────────────────────────────────────────────
# FROM test — publish стартует только после успешного прохождения тестов
FROM test AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/src/API"
RUN dotnet publish "API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ── Stage 5: final runtime image ──────────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]