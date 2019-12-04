@echo off
cd ..
docker-compose -f docker-compose-security.yml up -d
timeout 2
docker-compose -f LinuxMainContainers\docker-compose-dev.yml up
timeout 15