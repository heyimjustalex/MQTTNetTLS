#!/bin/bash

sudo apt update
sudo apt upgrade

#Install dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
sudo chmod +x dotnet-install.sh
./dotnet-install.sh --channel 6.0
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
echo 'export PYTHONNET_PYDLL=/usr/lib/python3.9/config-3.9-arm-linux-gnueabihf/libpython3.9.so' >> ~/.bashrc

echo 'export USERNAME=client1' >> ~/.bashrc
echo 'export PASSWORD=password1' >> ~/.bashrc
echo 'export CLIENT_ID=alarm1' >> ~/.bashrc

. ~/.bashrc
