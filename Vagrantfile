Vagrant.configure("2") do |config|
  # Lightweight Ubuntu 22.04 (bento is smaller and optimized for Vagrant)
  config.vm.box = "bento/ubuntu-22.04"
  config.vm.hostname = "bigdata-assignment"

  # Network setup
  config.vm.network "private_network", ip: "192.168.56.10"
  config.vm.network "forwarded_port", guest: 30081, host: 30081

  # Resource allocation
  config.vm.provider "virtualbox" do |vb|
    vb.name = "BigDataAssignment"
    vb.memory = "3072"
    vb.cpus = 2
    vb.customize ["modifyvm", :id, "--ioapic", "on"]
    vb.customize ["modifyvm", :id, "--paravirtprovider", "kvm"]
  end

  # Provisioning script
  config.vm.provision "shell", inline: <<-SHELL
    echo "=== Step 1: Update system ==="
    sudo apt-get update -y

    echo "=== Step 2: Install Docker (needed for Minikube driver) ==="
    sudo apt-get install -y apt-transport-https ca-certificates curl conntrack
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt-get update -y
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io
    sudo usermod -aG docker vagrant
    sudo systemctl enable docker
    sudo systemctl start docker

    echo "=== Step 3: Install kubectl ==="
    curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
    sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl
    rm kubectl

    echo "=== Step 4: Install Minikube ==="
    curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
    sudo install minikube-linux-amd64 /usr/local/bin/minikube
    rm minikube-linux-amd64

    echo "=== Step 5: Start Minikube cluster ==="
    sudo -u vagrant minikube start --driver=docker --memory=2048 --cpus=2

    echo "=== Step 6: Pull microservice Docker images ==="
    sudo -u vagrant docker pull vky84/libraryapi:1.0
    sudo -u vagrant docker pull vky84/notificationservice:1.0
    sudo -u vagrant docker pull postgres:latest

    echo "=== Step 7: Load images into Minikube ==="
    sudo -u vagrant minikube image load vky84/libraryapi:1.0
    sudo -u vagrant minikube image load vky84/notificationservice:1.0
    sudo -u vagrant minikube image load postgres:latest

    echo "=== Step 8: Deploy to Kubernetes (Services first, then Deployments) ==="
    cd /vagrant/LibraryApi/k8s
    
    # Apply services first (best practice)
    sudo -u vagrant kubectl apply -f libraryapi-service.yaml
    sudo -u vagrant kubectl apply -f notificationservice-service.yaml
    
    # Then apply deployments
    sudo -u vagrant kubectl apply -f postgres-deployment.yaml
    sudo -u vagrant kubectl apply -f libraryapi-deployment.yaml
    sudo -u vagrant kubectl apply -f notificationservice-deployment.yaml

    echo "=== Step 9: Wait for pods to be ready ==="
    sudo -u vagrant kubectl wait --for=condition=ready pod --all --timeout=300s

    echo "=== Step 10: Show deployment status ==="
    sudo -u vagrant kubectl get pods
    sudo -u vagrant kubectl get services

    echo ""
    echo "✅ Setup complete! Your microservices are deployed."
    echo "Access Swagger at: http://192.168.56.10:30081/swagger"
  SHELL
end
