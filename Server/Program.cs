using MQTTnet; 
using MQTTnet.Server;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO;

namespace Server
{
    internal class Program
    {
        public static async Task onNewClientConnection(ClientConnectedEventArgs e)
        {
            Console.WriteLine("New Client, calling OnNewClient ");  
                
        }
        /*
        public static X509Certificate2 CreateSelfSignedCertificate(string oid)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");

            using (var rsa = RSA.Create())
            {
                var certRequest = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);

                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                certRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(oid) }, false));

                certRequest.CertificateExtensions.Add(sanBuilder.Build());

                using (var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddMinutes(-10), DateTimeOffset.Now.AddMinutes(10)))
                {
                    var pfxCertificate = new X509Certificate2(
                        certificate.Export(X509ContentType.Pfx),
                        (string)null!,
                        X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

                    return pfxCertificate;
                }
            }
        }*/
        public static X509Certificate2 ReadCertificateWithPrivateKey(string fileCertPath, string keyPath, string password)
        {
            try
            {
                // Read the private key from key.pem file
                string keyPem = File.ReadAllText(keyPath);

                // Create an RSA key from the private key PEM and password
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromEncryptedPem(keyPem, password);
                    // Load the certificate from fileCertPath
                    var certificate = new X509Certificate2(fileCertPath, password);

                    var certificate2 = certificate.CopyWithPrivateKey(rsa);
                    // Associate the private key with the certificate


                    return certificate2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading certificate with private key: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();


            string serverCertPath = "../../../PKI/Server/server.pfx";
            string keyCertPath = "../../../PKI/Server/key.pem";
            // WITH THIS DOES NOT WORK
            var certificate = ReadCertificateWithPrivateKey(serverCertPath, keyCertPath, "password");


            // WITH THIS DOES WORK

            // var certificate2 = CreateSelfSignedCertificate("1.3.6.1.5.5.7.3.1");

            // var mqttServerOptions = new MqttServerOptionsBuilder().WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12).WithEncryptionCertificate(certificate).WithEncryptedEndpoint().Build();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithEncryptionCertificate(certificate.Export(X509ContentType.Pfx)).WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12).WithEncryptedEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                mqttServer.ClientConnectedAsync += onNewClientConnection;
                try {
                    
                    await mqttServer.StartAsync();
                
                
                } catch (Exception ex) {
                    Console.WriteLine($"Error when client connecting: {ex.Message} {ex.StackTrace}");
                }

                Console.WriteLine("Waitng for connections \n Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }
    }
}
