@echo off
cd /d ".."
timeout 5
docker-compose -f docker-compose.yml up
timeout 15