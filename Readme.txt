cleanup vagrant
vagrant destroy -f
Remove-Item -Recurse -Force .vagrant


docker build -t vky84/libraryapi:1.0 .
# from the repo root (c:\BigDataAssignment)
docker build -t vky84/libraryapi:1.0 .
docker build -t vky84/notificationservice:latest .

docker push vky84/libraryapi:1.0
docker push vky84/notificationservice:latest

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
