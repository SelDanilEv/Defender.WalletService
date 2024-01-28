docker rm -f LocalWalletService
docker build . -t local-wallet-service && ^
docker run -d --name LocalWalletService -p 47058:80 ^
--env-file ./../../secrets/secrets.local.list ^
-e ASPNETCORE_ENVIRONMENT=DockerLocal ^
-it local-wallet-service
echo finish local-wallet-service
docker image prune -f
pause
