@echo off
cd /d "C:\Users\yousef.alaw\source\repos\QuarklessQuack"
timeout 5
docker-compose -f LinuxMainContainers/docker-compose.yml -f docker-compose.yml up
timeout 15