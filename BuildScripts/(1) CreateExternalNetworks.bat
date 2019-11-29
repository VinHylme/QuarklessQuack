@echo off
docker network prune --filter label=_local-network
docker network prune --filter label=_linux
timeout 1
docker network create linux-services-net --driver nat
docker network create transport-net --driver nat
