# Docker 部署指南

## 快速启动

```bash
# 进入后端目录
cd JuggleNet6

# 一键构建并启动（首次构建约需 3-5 分钟）
docker compose up -d --build

# 查看日志
docker compose logs -f juggle
```

访问：http://localhost:9127  
默认账号：**juggle / juggle**  
Swagger：http://localhost:9127/swagger

---

## 常用命令

```bash
# 停止
docker compose down

# 停止并删除数据卷（⚠️ 数据库数据会丢失）
docker compose down -v

# 重新构建（代码有变更后）
docker compose up -d --build

# 进入容器
docker exec -it juggle sh

# 查看 SQLite 数据库位置（容器内）
docker exec -it juggle ls -la /data/
```

---

## 端口映射

默认映射宿主机 `9127` → 容器 `9127`。

如需修改宿主机端口，编辑 `docker-compose.yml`：
```yaml
ports:
  - "8080:9127"   # 改为 8080
```

---

## 数据持久化

SQLite 数据库存储在 Docker 命名卷 `juggle_data`，路径对应容器内 `/data/juggle.db`。

查看卷位置：
```bash
docker volume inspect juggle_juggle_data
```

备份数据库：
```bash
docker cp juggle:/data/juggle.db ./juggle_backup.db
```

恢复数据库：
```bash
docker cp ./juggle_backup.db juggle:/data/juggle.db
docker compose restart juggle
```

---

## 环境变量

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | 运行环境 |
| `ASPNETCORE_URLS` | `http://+:9127` | 监听地址 |
| `DB_PATH` | `/data/juggle.db` | SQLite 文件路径 |
| `Jwt__Key` | （见 appsettings） | JWT 密钥，生产建议覆盖 |

在 `docker-compose.yml` 的 `environment` 节添加：
```yaml
environment:
  - Jwt__Key=your-production-secret-key-min-32-chars
```

---

## 单独构建镜像

```bash
# 构建镜像
docker build -t juggle-net8:latest .

# 运行（不用 compose）
docker run -d \
  --name juggle \
  -p 9127:9127 \
  -v juggle_data:/data \
  -e DB_PATH=/data/juggle.db \
  juggle-net8:latest
```
