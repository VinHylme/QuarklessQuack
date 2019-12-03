@echo off
cd /d ".."
timeout 5
docker-compose -f LinuxMainContainers/docker-compose.yml -f docker-compose.yml up
timeout 15