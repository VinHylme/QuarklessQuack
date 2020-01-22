@echo off
cd /d ".."
timeout 5
docker create --network=transport-net --name heartbeat.base heartbeat/base.extract:latest
docker network connect linux-services-net heartbeat.base
timeout 1
docker create --network=transport-net --name heartbeat.external heartbeat/external.extract:latest
docker network connect linux-services-net heartbeat.external
timeout 1
docker create --network=transport-net --name heartbeat.targetlisting heartbeat/targetlisting:latest
docker network connect linux-services-net heartbeat.targetlisting
timeout 1
docker create --network=transport-net --name heartbeat.userinfo heartbeat/userinformation:latest
docker network connect linux-services-net heartbeat.userinfo
timeout 15