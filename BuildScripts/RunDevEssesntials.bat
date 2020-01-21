@echo off
cd ..

@echo creating network...
docker network create localnet -d bridge

@echo starting redis, mongo and selenium chrome
docker-compose -f LinuxMainContainers\docker-compose-dev.yml up
timeout 15