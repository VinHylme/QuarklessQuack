@echo off
cd /d "C:\Users\yousef.alaw\source\repos\QuarklessQuack"
timeout 1
FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker stop %%i
FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker rm %%i
docker network prune --filter label=_local-network
docker network prune --filter label=_linux
docker system prune --volumes
docker stack rm mainstack

timeout 15