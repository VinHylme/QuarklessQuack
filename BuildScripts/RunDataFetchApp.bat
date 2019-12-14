@echo off
cd ..
docker-compose -f docker-compose-dataFetch.yml up
timeout 15