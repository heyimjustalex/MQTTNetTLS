# Stop and remove Docker containers with names starting with "mqttalarmsystem"
$containerIds = docker ps -a --filter "name=^mqttalarmsystem" --format "{{.ID}}"
foreach ($containerId in $containerIds) {
    docker stop $containerId
    docker rm $containerId
}

# Get the Ethernet IP address
$ethernetIpAddress = (Get-NetIPAddress | Where-Object {
    $_.AddressFamily -eq 'IPv4' -and
    $_.InterfaceAlias -like '*Ethernet*' -and
    $_.PrefixOrigin -eq 'Dhcp'
}).IPAddress

# Display the Ethernet IP address
Write-Host "Your Ethernet IP Address is: $ethernetIpAddress"

# Get the Wi-Fi IP address
$wiFiIpAddress = (Get-NetIPAddress | Where-Object {
    $_.AddressFamily -eq 'IPv4' -and
    $_.InterfaceAlias -like '*Wi-Fi*' -and
    $_.PrefixOrigin -eq 'Dhcp'
}).IPAddress

# Display the Wi-Fi IP address
Write-Host "Your Wi-Fi IP Address is: $wiFiIpAddress"

# Export the IP address as an environment variable
[Environment]::SetEnvironmentVariable("MY_IP_ADDRESS", $wiFiIpAddress, [System.EnvironmentVariableTarget]::Machine)

# Print the value of the environment variable using Write-Host
Write-Host "Value of MY_IP_ADDRESS: $env:MY_IP_ADDRESS"
refreshenv
# Run Docker Compose with build
docker-compose up --build
