docker build -t vky84/libraryapi:1.0 .
docker push vky84/libraryapi:1.0

docker pull vky84/libraryapi:1.0

-----------------------------------------------

vagrant ssh 
vagrant ssh -c "kubectl port-forward service/libraryapi-service 8080:8080"


ls /vagrant

minikube status
minikube start --driver=docker

cd /vagrant/LibraryApi/k8s
kubectl apply -f libraryapi-deployment.yaml
kubectl apply -f libraryapi-service.yaml

kubectl port-forward --address 0.0.0.0 service/libraryapi-service 5081:8080

kubectl get pods
kubectl get services

http://192.168.56.10:30080/swagger/index.htm