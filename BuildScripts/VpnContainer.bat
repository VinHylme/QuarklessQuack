@echo off
cd ..
docker run -ti --cap-add=NET_ADMIN --device /dev/net/tun --name vpn -e USER=silviuandre@yahoo.com -e PASS=IYVunLc0dzjovj0= -e RANDOM_TOP=10 -e RECREATE_VPN_CRON="5 */3 * * *" -e CATEGORY="Dedicated IP" -d azinchen/nordvpn
timeout 15