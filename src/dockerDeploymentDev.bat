docker rm -f DevServiceTemplate
docker build . -t dev-service-template && ^
docker run -d --name DevServiceTemplate -p 0000:80 ^
--env-file ./../../secrets/secrets.dev.list ^
-e ASPNETCORE_ENVIRONMENT=DockerDev ^
-it dev-service-template
echo finish dev-service-template
docker image prune -f
pause
