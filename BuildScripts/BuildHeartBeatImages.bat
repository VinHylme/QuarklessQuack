@echo off
cd /d "C:\Users\yousef.alaw\source\repos\QuarklessQuack"
timeout 5
docker build -t heartbeat/base.extract --file Quarkless.Heartbeater.BaseExtract/Dockerfile .
docker build -t heartbeat/external.extract --file Quarkless.Heartbeater.ExternalExtract/Dockerfile .
docker build -t heartbeat/targetlisting --file Quarkless.Heartbeater.TargetListing/Dockerfile .
docker build -t heartbeat/userinformation --file Quarkless.Heartbeater.UserInformation/Dockerfile .
timeout 15