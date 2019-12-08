@echo off
cd ..
docker-compose -f docker-compose-vpn.yml up
timeout 33