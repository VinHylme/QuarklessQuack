@echo off
cd /d "C:\Users\yousef.alaw\source\repos\QuarklessQuack"
timeout 5
docker stack deploy -c docker-compose.yml -c LinuxMainContainers/docker-compose.yml vossibility
timeout 5