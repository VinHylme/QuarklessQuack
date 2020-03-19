<#clear all previous vpn instances#>
Write-Host "Clearning all previous vpn instances"
docker ps -a -q --filter "name=^vpn" --format="{{.ID}}" | ForEach-Object -Process {docker rm $_ -f}
docker volume prune -f
Start-Sleep -Seconds 10.5
<#clean network (remove used ports)#>
Write-Host "Calling Network Clean Up Script"
& .\NetworkCleanUp.ps1
<#recreate vpns#>

$create_vpn_amount = 6;
$port_start = 9080;
Write-Host "Recreating vpn instances, Creating $create_vpn_amount instances"

For ($port=$port_start; $port -lt $create_vpn_amount + $port_start; $port++){
  docker run -ti --cap-add=NET_ADMIN --device /dev/net/tun --name vpn$port -e RANDOM_TOP=200 -e USER='silviuandre@yahoo.com' -e PASS='IYVunLc0dzjovj0=' -p $port":"$port -d azinchen/nordvpn
  Start-Sleep -Seconds 15
}
