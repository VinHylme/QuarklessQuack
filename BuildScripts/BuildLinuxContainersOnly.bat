@echo off
cd /d ".."
timeout 2
docker-compose -f LinuxMainContainers\docker-compose.yml up
timeout 15
