# ============================================================
# Juggle 接口编排平台 - .NET 8 + Vue3
# Version: 1.0
# ============================================================

# ============================================================
# Stage 1: 构建前端 (Node.js)
# ============================================================
FROM node:20-alpine AS frontend-build

WORKDIR /frontend

# 先复制 package.json 利用 Docker 缓存
COPY JuggleNet6.Frontend/package*.json ./
RUN npm ci --prefer-offline

# 复制源码并构建
COPY JuggleNet6.Frontend/ ./
RUN npm run build

# ============================================================
# Stage 2: 构建后端 (.NET 8 SDK)
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /src

# 先复制 sln 和 csproj，利用 Docker 缓存 restore
COPY Juggle.sln ./
COPY Juggle.Domain/Juggle.Domain.csproj             Juggle.Domain/
COPY Juggle.Infrastructure/Juggle.Infrastructure.csproj Juggle.Infrastructure/
COPY Juggle.Application/Juggle.Application.csproj   Juggle.Application/
COPY Juggle.Api/Juggle.Api.csproj                   Juggle.Api/

RUN dotnet restore Juggle.Api/Juggle.Api.csproj

# 复制所有源码
COPY Juggle.Domain/       Juggle.Domain/
COPY Juggle.Infrastructure/ Juggle.Infrastructure/
COPY Juggle.Application/  Juggle.Application/
COPY Juggle.Api/          Juggle.Api/

# 将前端构建产物放入 wwwroot（在 publish 之前）
COPY --from=frontend-build /frontend/dist/ Juggle.Api/wwwroot/

# 发布
RUN dotnet publish Juggle.Api/Juggle.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# Stage 3: 最终运行镜像 (.NET 8 Runtime)
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# OCI 镜像元数据
LABEL org.opencontainers.image.title="Juggle 接口编排平台"
LABEL org.opencontainers.image.description="Java/Spring Boot Juggle 接口编排的 .NET 8 移植版，支持流程设计、调度、多租户"
LABEL org.opencontainers.image.version="1.0"
LABEL org.opencontainers.image.source="https://github.com/pythonHuang/JuggleNet6"
LABEL org.opencontainers.image.authors="pythonHuang"

WORKDIR /app

# 复制发布产物
COPY --from=backend-build /app/publish .

# 数据目录（SQLite 文件存放在此，通过 volume 持久化）
RUN mkdir -p /data

# 环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:9127
# SQLite 数据库路径指向 /data 目录（可通过 -e 覆盖）
ENV DB_PATH=/data/juggle.db

EXPOSE 9127

ENTRYPOINT ["dotnet", "Juggle.Api.dll"]
