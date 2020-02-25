@echo off
cd ..

@echo creating network...
docker network create localnet -d bridge


@echo starting redis, mongo and selenium chrome
docker-compose -f Setup\LinuxMainContainers\docker-compose.yml up
timeout 15