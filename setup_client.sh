#!/bin/bash

# Clone the repo
sudo apt install git
git clone https://github.com/heyimjustalex/MQTTSmokeAlarmSystem.git

# Download and install dotnet
curl -sSL https://dot.net/v1/dotnet-install.sh > dotnet-install.sh
sudo chmod +x dotnet-install.sh
./dotnet-install.sh --channel 6.0

# Add necessary environment variables

# Find python3.9 library on the system
PYTHONLIBDIR=$(find / -name libpython3.9.so 2>/dev/null)
echo "found python 3.9 lib: ${PYTHONLIBDIR}"
echo "export PYTHONNET_PYDLL=${PYTHONLIBDIR}" >> ~/.bashrc

# Dotnet env variables
echo "export DOTNET_ROOT=$HOME/.dotnet" >> ~/.bashrc
echo "export PATH=$PATH:$HOME/.dotnet" >> ~/.bashrc

# Client-specific variables
echo "export USERNAME=client1" >> ~/.bashrc
echo "export CLIENT_ID=alarm1" >> ~/.bashrc
echo "export PASSWORD=password1" >> ~/.bashrc

# This needs to be individually adjusted to each sensor
echo "export BUZZER_THRESHOLD=0.02" >> ~/.bashrc

source ~/.bashrc
