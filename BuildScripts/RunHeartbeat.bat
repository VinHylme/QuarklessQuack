@echo off
cd ..
docker-compose -f docker-compose-heartbeat.yml up
timeout 15