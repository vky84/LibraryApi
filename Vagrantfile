Vagrant.configure("2") do |config|
  # Base Ubuntu 22.04
  config.vm.box = "ubuntu/jammy64"
  config.vm.hostname = "bigdata-assignment"

  # Network setup
  config.vm.network "private_network", ip: "192.168.56.10"
  config.vm.network "forwarded_port", guest: 30081, host: 30081

  # Resource allocation
  config.vm.provider "virtualbox" do |vb|
    vb.name = "BigDataAssignment"
    vb.memory = "4096"
    vb.cpus = 2
  end

  # Provisioning script
  config.vm.provision "shell", inline: <<-SHELL
    echo "Updating system..."
    sudo apt-get update -y
    sudo apt-get upgrade -y

    echo "Installing dependencies..."
    sudo apt-get install -y apt-transport-https ca-certificates curl software-properties-common gnupg lsb-release conntrack

    echo "Installing Docker..."
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
    sudo apt-get update -y
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io
    sudo usermod -aG docker vagrant
    sudo systemctl enable docker
    sudo systemctl start docker

    echo "Installing Docker Compose..."
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose

    echo "Installing kubectl..."
    curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
    sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl
    rm kubectl

    echo "Installing Minikube..."
    curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
    sudo install minikube-linux-amd64 /usr/local/bin/minikube
    rm minikube-linux-amd64

    echo "Starting Minikube..."
    minikube start --driver=docker --memory=2048 --cpus=2

    echo "Set kubectl context to minikube"
    sudo chown -R vagrant:vagrant $HOME/.kube
    sudo chown -R vagrant:vagrant $HOME/.minikube

    echo "Installation and Minikube start completed!"

    # echo "Starting Minikube on boot..."
    # sudo bash -c 'echo "@reboot root minikube start --driver=docker" >> /etc/crontab'
  SHELL
end
