# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件并还原依赖
COPY Endfield.Api.csproj .
RUN dotnet restore

# 复制源代码并构建
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# 运行阶段
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# 设置时区
ENV TZ=Asia/Shanghai
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# 暴露端口
EXPOSE 8001

# 设置环境变量
ENV ASPNETCORE_URLS=http://+:8001
ENV ASPNETCORE_ENVIRONMENT=Production

# 复制构建产物
COPY --from=build /app/publish .

# 启动应用
ENTRYPOINT ["dotnet", "Endfield.Api.dll"]
