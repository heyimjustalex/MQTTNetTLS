# MQTTSmokeAlarmSystem 🔥

## Project Overview

The MQTT Smoke Alarm 🚨 System  is a study project designed to demonstrate the use of C# in developing a simple IOT alarm system. This system is capable of detecting smoke using Raspberry Pi sensors and triggering alarms when necessary. It leverages MQTTnet for communication, C# for application logic, PKI (MQTT over TLS 🔐) for secure communication, and utilizes .NET 7 for compatibility. Additionally, a simple WPF-based GUI is provided for managing the broker  🖥️.

## Project Structure

This solution consists of three main projects:
### Client (Raspberry Pi Client)

The Client project represents the Raspberry Pi client. It is responsible for collecting sensor data, detecting smoke, and communicating with the broker using MQTT over TLS for secure data transfer. Clients can be deployed both on Windows and Linux.

### BrokerGUI (Broker Graphical User Interface)

The BrokerGUI project serves as the central communication hub that manages multiple clients and their respective sensors. It provides a graphical interface developed with WPF for interacting with the broker. It allows to monitor clients' sensors state and displays alarm state. Use of WPF makes it work only on Windows.

### PKIGenerator (Public Key Infrastructure Generator)

The PKIGenerator project is responsible for generating the necessary Public Key Infrastructure (PKI) for both clients and the broker. 

## Dependencies

- MQTTnet (4.3.1.873): MQTTnet is used for MQTT communication between the broker and clients.
- .NET 7: The project utilizes .NET 7

## Getting Started

### Client simulation with Docker

If you don't have real hardware to install client on, there is a possibility to start simulated clients with mocked sensor data with containers. Just run powershell script 

"run_simulated_client_docker_containers.ps1"

The script takes your WiFi address and uses it as an input for clients. It also sets BROKER_IP_ADDRESS Windows environmental variable that is used by docker-compose.yml (which is used to deploy clients) and broker. After setting BROKER_IP_ADDRESS env variable there it might be required to restart VS.

Make sure that username, password you use is in db.json file in broker.


### Real client without Docker

Modify initial parameters of client so it has proper broker ip. Config is initialized in Client/Program.cs


## Class Diagrams

### Broker

![image](https://github.com/heyimjustalex/MQTTNetTLS/assets/21158649/1439bdbc-387e-4762-8b46-8e12ce3a774e)


### Client


### WPF GUI

![image](https://github.com/heyimjustalex/MQTTNetTLS/assets/21158649/9bca8773-8f7f-4936-9bbb-a100a95d8b8c)


