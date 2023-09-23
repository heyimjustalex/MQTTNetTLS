using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static bool OnCertificateValidation(MqttClientCertificateValidationEventArgs args)
        {

            // return true;
            X509Certificate2 serverCertificate = new X509Certificate2(args.Certificate);
            X509Certificate2 CACertificate = ReadCertificateFromFile("../../../PKI/CA/RootCA.cer");
            AddCAToTrusted(CACertificate);
            try
            {
                args.Chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                args.Chain.ChainPolicy.CustomTrustStore.Add(CACertificate);
                args.Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                var chain = args.Chain.Build(serverCertificate);

                args.Chain.ChainStatus.ToList().ForEach(x => { Console.WriteLine(x.Status.ToString()); });
                if (chain)
                {

                    Console.WriteLine("CERT BUILD TRUE");
                    return true;
                }
                else
                {
                    Console.WriteLine("CERT BUILD FALSE");
                    return false;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return false;
            };




            // if (isCertificateValid(serverCertificate, CACertificate))
            //{
            //    Console.WriteLine("CERT IS VALID");
            //    return true;

            //}
            //else
            //{
            //    Console.WriteLine("CERT IS INVALID");
            //}
            //return false;

        }
        static async Task Main(string[] args)
        {
            var mqttFactory = new MqttFactory();
            var mqttClient = mqttFactory.CreateMqttClient();
            bool connected = false;

            while (!connected)
            {
                using (var mqtttClient = mqttFactory.CreateMqttClient())
                {
                    // var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("localhost", 8883)
                    //     .WithTlsOptions(o => o.WithCertificateValidationHandler(OnCertificateValidation).WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12)).Build();
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithClientId("ClientId")
                        .WithTcpServer("localhost", 8883)
                        //.WithCredentials("Username", "Password")
                        .WithTls(new MqttClientOptionsBuilderTlsParameters()
                        {
                            UseTls = true,
                            SslProtocol = SslProtocols.Tls12,
                            CertificateValidationHandler = OnCertificateValidation
                        })
                        .WithCleanSession()
                        .Build();

                    using (var timeout = new CancellationTokenSource(5000))
                    {
                        try
                        {
                            var response = await mqtttClient.ConnectAsync(mqttClientOptions, timeout.Token);
                            Console.WriteLine("The MQTT client is connected.");
                            Console.WriteLine(response.ResultCode);
                            connected = true; 
                        }
                        
                        catch (Exception ex)
                        {
                            Console.WriteLine("EXCEPTION CLIENT CANNOT CONNECT");
                            Console.WriteLine(ex);
                            Console.WriteLine(ex.StackTrace);

                            // Delay for 2 seconds before retrying
                            await Task.Delay(2000);
                        }
                    }
                }
            }

            Console.Read();
        }


        public static void AddCAToTrusted(X509Certificate2 caCertificate)
        {
            try
            {
                // Create a new X509Store for the Trusted Root Certification Authorities
                using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadWrite); // Open the store for writing

                    // Check if the certificate already exists in the store
                    bool certificateExists = false;
                    foreach (X509Certificate2 existingCertificate in store.Certificates)
                    {
                        if (existingCertificate.Thumbprint == caCertificate.Thumbprint)
                        {
                            certificateExists = true;
                            break;
                        }
                    }

                    if (!certificateExists)
                    {
                        // Add the CA certificate to the store
                        store.Add(caCertificate);

                        Console.WriteLine("CA certificate added to Trusted Root Certification Authorities store.");
                    }
                    else
                    {
                        Console.WriteLine("CA certificate already exists in Trusted Root Certification Authorities store.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static bool isCertificateValid(X509Certificate2 certificate, X509Certificate2 caCertificate)
        {
            try
            {
                using (X509Chain chain = new X509Chain())
                {
                    chain.ChainPolicy.ExtraStore.Add(caCertificate);

                    // Enable certificate revocation check (optional, but recommended)
                    //   chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

                    // Check the entire certificate chain, including the root certificate
                    //  chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                    // THIS BREAKS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    if (chain.Build(certificate))
                    {
                        // Check if the certificate chain is valid


                        // Check if the root certificate is trusted
                        X509ChainStatus rootStatus = chain.ChainStatus.LastOrDefault();
                        bool isNotTrusted = rootStatus.Status == X509ChainStatusFlags.UntrustedRoot;
                        if (!isNotTrusted)
                        {
                            return true; // Root certificate is not trusted
                        }

                        // Chain is valid, but root is trusted

                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying certificate: {ex.Message}");
                return false;
            }
        }


        public static X509Certificate2 ReadCertificateFromFile(string filePath, string password = null)
        {
            try
            {
                return new X509Certificate2(filePath, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CA certificate: {ex.Message}");
                return null;
            }
        }

    }

}
