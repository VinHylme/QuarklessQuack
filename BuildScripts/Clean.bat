@echo off
cd ..
FOR /f "tokens=*" %%i IN ('docker ps -q') DO docker rm %%i
timeout 15