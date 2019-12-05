@echo off
cd ..

@echo creating network...
docker network create localnet -d nat

@echo starting quarkless.security...
docker-compose -f docker-compose-security.yml up -d
timeout 1

cd Quarkless.Python
@echo starting python scripts language detection and google image search
docker-compose -f docker-compose.yml up -d
timeout 1

cd ..
@echo starting redis, mongo and selenium chrome
docker-compose -f LinuxMainContainers\docker-compose-dev.yml up
timeout 15