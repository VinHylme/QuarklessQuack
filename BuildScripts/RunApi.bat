@echo off
cd ..
docker-compose -f docker-compose.yml up -d
timeout 15