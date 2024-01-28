docker rm -f LocalServiceTemplate
docker build . -t local-service-template && ^
docker run -d --name LocalServiceTemplate -p 0000:80 ^
--env-file ./../../secrets/secrets.local.list ^
-e ASPNETCORE_ENVIRONMENT=DockerLocal ^
-it local-service-template
echo finish local-service-template
docker image prune -f
pause
