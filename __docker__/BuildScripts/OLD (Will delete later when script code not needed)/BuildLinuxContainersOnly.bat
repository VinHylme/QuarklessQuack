@echo off
cd /d "C:\Users\yousef.alaw\source\repos\QuarklessQuack"
timeout 2
docker-compose -f LinuxMainContainers\docker-compose.yml up
timeout 15
