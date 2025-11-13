cleanup vagrant
vagrant destroy -f
Remove-Item -Recurse -Force .vagrant


docker build -t vky84/libraryapi:1.0 .
# from the repo root (c:\BigDataAssignment)
docker build --target libraryapi -t libraryapi:latest .
docker build --target notificationservice -t notificationservice:latest .

docker push vky84/libraryapi:1.0

docker pull vky84/libraryapi:1.0

-----------------------------------------------

vagrant ssh 
vagrant ssh -c "kubectl port-forward service/libraryapi-service 8080:8080"


ls /vagrant

minikube status
minikube start --driver=docker

# Apply Kubernetes manifests (services before deployments)
cd /vagrant/LibraryApi/k8s

# Apply services BEFORE deployments (best practice)
kubectl apply -f libraryapi-service.yaml
kubectl apply -f notificationservice-service.yaml

# Then apply deployments
kubectl apply -f postgres-deployment.yaml
kubectl apply -f libraryapi-deployment.yaml
kubectl apply -f notificationservice-deployment.yaml

kubectl port-forward --address 0.0.0.0 service/library  api-service 5081:8080

kubectl port-forward --address 0.0.0.0 service/library  api-service 5062:8080

kubectl port-forward --address 0.0.0.0 service/libraryapi-service 30081:8080 &

kubectl get pods
kubectl get services

http://192.168.56.10:30080/swagger/index.htm

http://localhost:5262/api/Books


kubectl rollout restart deployment libraryapi


Presentation Flow: 

-> Show the code
-> Show swagger
-> Show DB
-> Run some endpoints
-> Show data in DB
-> Show yml files in the source code
-> Show docker files
-> Show vagrant files
-> Show VM
-> Open powershell 
-> Vagrant Up
-> Vagrnt ssh

===============================================================================
                    KUBERNETES DEPLOYMENT GUIDE (KIND)
===============================================================================

NOTE: This setup uses Kind (Kubernetes in Docker) because VirtualBox conflicts
with Hyper-V on Windows systems that have Docker Desktop installed.

-------------------------------------------------------------------------------
1. STOP/CLEANUP EXISTING DEPLOYMENT
-------------------------------------------------------------------------------

# Stop and delete the Kind cluster
.\kind-windows-amd64.exe delete cluster --name minikube

# Optional: Clean up Docker images (if needed)
docker rmi vky84/libraryapi:1.0
docker rmi vky84/notificationservice:1.0

# Optional: Remove Kind executable (if you want to start completely fresh)
Remove-Item kind-windows-amd64.exe
Remove-Item kind-config.yaml

-------------------------------------------------------------------------------
2. FRESH RUN - COMPLETE DEPLOYMENT
-------------------------------------------------------------------------------

### Step 1: Download Kind (if not already present)
curl.exe -Lo kind-windows-amd64.exe https://kind.sigs.k8s.io/dl/v0.27.0/kind-windows-amd64

### Step 2: Create Kind cluster configuration
# Create kind-config.yaml with this content:
@"
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
  extraPortMappings:
  - containerPort: 30081
    hostPort: 30081
    protocol: TCP
  - containerPort: 30082
    hostPort: 30082
    protocol: TCP
"@ | Out-File -FilePath kind-config.yaml -Encoding UTF8

### Step 3: Create the Kind cluster
.\kind-windows-amd64.exe create cluster --name minikube --config kind-config.yaml

### Step 4: Build Docker images
docker build -t vky84/libraryapi:1.0 --target libraryapi .
docker build -t vky84/notificationservice:1.0 --target notificationservice .

### Step 5: Load images into Kind cluster
.\kind-windows-amd64.exe load docker-image vky84/libraryapi:1.0 --name minikube
.\kind-windows-amd64.exe load docker-image vky84/notificationservice:1.0 --name minikube

### Step 6: Deploy to Kubernetes
# Deploy PostgreSQL first
kubectl apply -f LibraryApi/k8s/postgres-deployment.yaml

# Deploy services
kubectl apply -f LibraryApi/k8s/libraryapi-service.yaml
kubectl apply -f LibraryApi/k8s/notificationservice-service.yaml

# Deploy applications
kubectl apply -f LibraryApi/k8s/libraryapi-deployment.yaml
kubectl apply -f LibraryApi/k8s/notificationservice-deployment.yaml

### Step 7: Wait for pods to be ready (optional)
kubectl wait --for=condition=ready pod --all --timeout=300s

### Step 8: Check deployment status
kubectl get pods
kubectl get services
kubectl get all

-------------------------------------------------------------------------------
3. ACCESS YOUR SERVICES
-------------------------------------------------------------------------------

LibraryAPI Swagger UI: http://localhost:30081/swagger
LibraryAPI Base URL:   http://localhost:30081
NotificationService:   http://localhost:30082

-------------------------------------------------------------------------------
4. USEFUL COMMANDS
-------------------------------------------------------------------------------

# View logs for a specific pod
kubectl logs <pod-name>

# View logs for LibraryAPI
kubectl logs -l app=libraryapi

# View logs for NotificationService
kubectl logs -l app=notificationservice

# Restart a deployment
kubectl rollout restart deployment/libraryapi-deployment
kubectl rollout restart deployment/notificationservice-deployment

# Check rollout status
kubectl rollout status deployment/libraryapi-deployment

# Describe a pod (useful for troubleshooting)
kubectl describe pod <pod-name>

# Get into a pod shell (for debugging)
kubectl exec -it <pod-name> -- /bin/bash

# View cluster info
kubectl cluster-info
kubectl config get-contexts

# List all resources
kubectl get all
kubectl get pods -o wide

-------------------------------------------------------------------------------
5. TROUBLESHOOTING
-------------------------------------------------------------------------------

### If pods are in Error or CrashLoopBackOff state:

# Check pod logs
kubectl logs <pod-name>

# Check pod events
kubectl describe pod <pod-name>

# Force restart by deleting the pod (it will auto-recreate)
kubectl delete pod <pod-name>

### If database has stale data:

# Delete and recreate PostgreSQL with fresh data
kubectl delete deployment postgres
kubectl delete pvc postgres-pvc
kubectl delete pv postgres-pv
kubectl apply -f LibraryApi/k8s/postgres-deployment.yaml

# Restart applications to reconnect
kubectl rollout restart deployment/libraryapi-deployment
kubectl rollout restart deployment/notificationservice-deployment

### If ports are already in use:

# Check what's using the ports
netstat -ano | findstr :30081
netstat -ano | findstr :30082

# Stop the process or delete the old cluster
.\kind-windows-amd64.exe delete cluster --name minikube

-------------------------------------------------------------------------------
6. QUICK START SCRIPT (Copy-paste this entire block)
-------------------------------------------------------------------------------

# Stop existing cluster (if any)
.\kind-windows-amd64.exe delete cluster --name minikube 2>$null

# Download Kind if needed
if (-not (Test-Path "kind-windows-amd64.exe")) {
    curl.exe -Lo kind-windows-amd64.exe https://kind.sigs.k8s.io/dl/v0.27.0/kind-windows-amd64
}

# Create config
@"
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
  extraPortMappings:
  - containerPort: 30081
    hostPort: 30081
    protocol: TCP
  - containerPort: 30082
    hostPort: 30082
    protocol: TCP
"@ | Out-File -FilePath kind-config.yaml -Encoding UTF8

# Create cluster
.\kind-windows-amd64.exe create cluster --name minikube --config kind-config.yaml

# Build and load images
docker build -t vky84/libraryapi:1.0 --target libraryapi .
docker build -t vky84/notificationservice:1.0 --target notificationservice .
.\kind-windows-amd64.exe load docker-image vky84/libraryapi:1.0 --name minikube
.\kind-windows-amd64.exe load docker-image vky84/notificationservice:1.0 --name minikube

# Deploy everything
kubectl apply -f LibraryApi/k8s/postgres-deployment.yaml
kubectl apply -f LibraryApi/k8s/libraryapi-service.yaml
kubectl apply -f LibraryApi/k8s/notificationservice-service.yaml
kubectl apply -f LibraryApi/k8s/libraryapi-deployment.yaml
kubectl apply -f LibraryApi/k8s/notificationservice-deployment.yaml

Write-Host "\nWaiting for pods to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "\n=== Deployment Status ===" -ForegroundColor Green
kubectl get all

Write-Host "\n=== Access Your Services ===" -ForegroundColor Cyan
Write-Host "LibraryAPI Swagger: http://localhost:30081/swagger"
Write-Host "NotificationService: http://localhost:30082"

===============================================================================

-------------------------------------------------------------------------------
7. ACCESSING POSTGRESQL DATABASE FROM PGADMIN (Host Computer)
-------------------------------------------------------------------------------

### Step 1: Start Port Forwarding
# Open a NEW PowerShell window and run this command (keep it running):
kubectl port-forward service/postgres 5432:5432

# This will forward the PostgreSQL port from the pod to your local machine
# Keep this window open while you want to access the database

### Step 2: Configure pgAdmin Connection
Open pgAdmin on your host computer and create a new server with these settings:

General Tab:
  Name: LibraryDB (Kind Cluster)    # Or any name you prefer

Connection Tab:
  Host name/address: localhost       # Or 127.0.0.1
  Port: 5432
  Maintenance database: LibraryDb    # The database name
  Username: postgres
  Password: postgres

Advanced Tab (optional):
  DB restriction: LibraryDb

### Step 3: Connect
Click "Save" and pgAdmin will connect to your database!

### Step 4: View Tables
Once connected, navigate to:
  Servers > LibraryDB (Kind Cluster) > Databases > LibraryDb > Schemas > public > Tables

You should see:
  - Books
  - BorrowingRecords
  - Users (created by NotificationService)
  - Notifications (created by NotificationService)
  - __EFMigrationsHistory

### Alternative: Using Command Line
# Connect to PostgreSQL using psql command line:
kubectl exec -it <postgres-pod-name> -- psql -U postgres -d LibraryDb

# Get the pod name first:
kubectl get pods | findstr postgres

# Then replace <postgres-pod-name> with actual pod name, for example:
kubectl exec -it postgres-84fc8cc777-vlrlr -- psql -U postgres -d LibraryDb

# Once connected, you can run SQL commands:
# \dt              -- List all tables
# \d Books         -- Describe Books table structure
# SELECT * FROM "Books";                  -- Query books
# SELECT * FROM "Users";                  -- Query users
# SELECT * FROM "BorrowingRecords";       -- Query borrowing records
# \q               -- Quit psql

### Troubleshooting Port Forward:

# If port 5432 is already in use:
netstat -ano | findstr :5432

# Use a different local port:
kubectl port-forward service/postgres 15432:5432
# Then in pgAdmin, use port 15432 instead of 5432

# Stop port forwarding:
# Press Ctrl+C in the PowerShell window running the port-forward command

### IMPORTANT NOTES:
- Port forwarding must remain active to keep the connection
- If you close the port-forward window, pgAdmin will lose connection
- You can run port-forward in the background, but it's easier to keep it in a separate window
- The database credentials are NOT secure (using default postgres/postgres)
- This is for DEVELOPMENT purposes only

===============================================================================
