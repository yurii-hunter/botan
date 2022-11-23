setup:
	docker compose up -d --remove-orphans

cleanup:
	docker compose down

fmt:
	dotnet format
    
test:
	dotnet test

build:
	dotnet build

run:
	dotnet run