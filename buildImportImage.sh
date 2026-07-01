#!/usr/bin/env bash
set -euo pipefail

echo "Building catalog image..."
docker build -t logipulse-catalog-service:latest -f CatalogService/Dockerfile .

echo "Building order image..."
docker build -t logipulse-order-api:latest -f OrderAPIs/Dockerfile .

echo "Building fleet image..."
docker build -t logipulse-fleet-service:latest -f FleetService/Dockerfile .

echo "Building notification image..."
docker build -t logipulse-notification-service:latest -f NotifierService/Dockerfile .

echo "Building frontend image..."
docker build -t logipulse-frontend-service:latest -f Frontend/Dockerfile .

echo "Images built:"
docker images | grep logipulse

echo "Importing images to k3d..."
k3d image import \
  logipulse-catalog-service:latest \
  logipulse-order-api:latest \
  logipulse-fleet-service:latest \
  logipulse-notification-service:latest \
  logipulse-frontend-service:latest \
  -c logipulse-cluster

cd /home/mohit/Development/logipulse-k8s
kubectl apply -f base
kubectl apply -f frontend.yaml
kubectl apply -f ingress.yaml

helm upgrade --install catalog-service ./charts/logipulse-service -n logipulse
helm upgrade --install fleet-service ./charts/logipulse-service -n logipulse
helm upgrade --install notification-service ./charts/logipulse-service -n logipulse
helm upgrade --install frontend-service ./charts/logipulse-service -n logipulse
helm upgrade --install order-api ./charts/logipulse-service -n logipulse

kubectl get pods -n logipulse
kubectl get svc -n logipulse
kubectl describe pod -n logipulse -l app=catalog-service

chmod +x buildImportImage.sh
./buildImportImage.sh