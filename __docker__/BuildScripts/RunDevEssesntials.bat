@echo off
cd ..

@echo creating network...
docker network create localnet -d bridge

@echo starting python scripts language detection and google image search
docker-compose -f ..\Quarkless.Python\docker-compose.yml up -d
timeout 1

@echo starting redis, mongo and selenium chrome
docker-compose -f Setup\LinuxMainContainers\docker-compose.yml up
timeout 15