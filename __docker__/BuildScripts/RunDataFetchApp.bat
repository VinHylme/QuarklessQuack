@echo off
cd ../../
docker-compose -f Quarkless.Operations\Quarkless.Services.DataFetcher.Console\docker-compose-dataFetch.yml up
timeout 15