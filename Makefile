.PHONY: up down logs build seed migrate minio-setup dev-backend dev-frontend

up:
	docker-compose up -d

down:
	docker-compose down

build:
	docker-compose build

logs:
	docker-compose logs -f api

migrate:
	docker-compose exec api dotnet ef database update

seed:
	docker-compose exec api dotnet run --project StreamingApp.API seed

minio-setup:
	docker-compose exec minio mc alias set local http://localhost:9000 minioadmin minioadmin123
	docker-compose exec minio mc mb local/streaming
	docker-compose exec minio mc anonymous set download local/streaming

dev-backend:
	cd backend && dotnet watch run --project StreamingApp.API

dev-frontend:
	cd frontend && ng serve --proxy-config proxy.conf.json
