Clean.bat => used to delete contains, networks, etc basically think of it like vb clean

RunApi.bat => used to run the api (not the website), if you don't need to run and work with the api, then launch it here so that it can run in the background.

RunDevEssentials.bat => basically the most important bat file here, it is used to launch redis (cache db), selenium and also 
quarkless.security (used to pass the appconfigs to other projects), so before begining developing make sure that this is running, otherwise the other projects will fail.

NetworkCleanup.ps1 => a powershell script used to clear out any cached networking and ports which causes some errors.