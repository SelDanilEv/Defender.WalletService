docker rm -f DevWalletService
docker build . -t dev-wallet-service && ^
docker run -d --name DevWalletService -p 49058:80 ^
--env-file ./../../secrets/secrets.dev.list ^
-e ASPNETCORE_ENVIRONMENT=DockerDev ^
-it dev-wallet-service
echo finish dev-wallet-service
docker image prune -f
pause
