@echo off
cd /d ".."
timeout 5
docker stack deploy -c docker-compose.yml -c LinuxMainContainers/docker-compose.yml vossibility
timeout 5