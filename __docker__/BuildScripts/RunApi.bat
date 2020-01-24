@echo off
cd ../..
docker-compose -f docker-compose.yml up
timeout 15