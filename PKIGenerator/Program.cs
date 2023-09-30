using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace PKIUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PKIUtility.generatePKI();
        }
    }

    internal class PKIUtility
    {
        public static void generatePKI()
        {
            string caCertificateFilePath = "../../../PKI/CA/rootCA.cer";
            string caCertificatePrivateKeyPath = "../../../PKI/CA/key.pem";
            string caCertificatePrivateKeyPassword = "password";

            string caCertificateToClientFilePath = "../../../../Client/PKI/CA/rootCA.cer";
            string caCertificateToServerPath = "../../../../Broker/PKI/CA/rootCA.cer";

            string certificateServerPath = "../../../PKI/Brokers/broker1.pfx";
            string certificateToServerPath = "../../../../Broker/PKI/Broker/broker1.pfx";
            string certificateServerPathPassword = "password";

            string serverKeyToServerPath = "../../../../Broker/PKI/Broker/key1.pem";

            Console.WriteLine("GENERATING CA CERTIFICATE AND SERVER CERTIFICATE");
            X509Certificate2 rootCertificate = GenerateCACertificate("RootCA-IOT");
            X509Certificate2 certificate = GenerateSignedCertificate("BROKER1-IOT",serverKeyToServerPath, rootCertificate, "password");

            ExportCertificateToFile(rootCertificate, caCertificateFilePath);
            ExportCertificateToFile(rootCertificate, caCertificateToServerPath);
            ExportCertificateToFile(rootCertificate, caCertificateToClientFilePath);
            ExportPrivateKeyToFile(rootCertificate, caCertificatePrivateKeyPath, caCertificatePrivateKeyPassword);
            ExportCertificateWithPrivateKeyToFile(certificate, certificateServerPath, certificateServerPathPassword);
            ExportCertificateWithPrivateKeyToFile(certificate, certificateToServerPath, certificateServerPathPassword);

            Console.WriteLine("CERTIFICATES HAVE BEEN GENERATED AND SAVED \n \nPRESS ENTER TO EXIT");
            Console.ReadKey();
        }
                 
        static X509Certificate2 GenerateCACertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    new X500DistinguishedName($"CN={subjectName}"),
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // Set CA certificate extensions
                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                // Create the CA certificate and make it self-signed
                var caCertificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));

                // Export the CA certificate and private key as a PFX (PKCS#12) file
                var pfxCertificate = new X509Certificate2(
                    caCertificate.Export(X509ContentType.Pfx),
                    (string)null!,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

                return pfxCertificate;
            }
        }

        public static void ExportPrivateKeyToFile(X509Certificate2 certificate, string privateKeyFilePath, string password)
        {
            try
            {
                File.WriteAllText(privateKeyFilePath, GetPrivateKeyAsPEM(certificate, password));
                Console.WriteLine("Private key exported successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting private key: {ex.Message} {ex.StackTrace}");
            }
        }

        private static string GetPrivateKeyAsPEM(X509Certificate2 certificate, string password)
        {
            try
            {
                // Check if the private key is exportable
                if (!certificate.HasPrivateKey)
                {
                    throw new Exception("Certificate does not have a private key.");
                }

                RSA privateKey = certificate.GetRSAPrivateKey();

                if (privateKey != null)
                {
                    // Export the private key in PFX (PKCS#12) format
                    byte[] pfxBytes = certificate.Export(X509ContentType.Pfx, password);

                    // Convert the PFX bytes to base64-encoded PEM format
                    string pem = Convert.ToBase64String(pfxBytes);

                    // Add PEM headers and footers
                    StringBuilder pemBuilder = new StringBuilder();
                    pemBuilder.AppendLine("-----BEGIN PRIVATE KEY-----");

                    // Split the base64-encoded PEM string into lines with a maximum line length (e.g., 64 characters)
                    int lineLength = 64;
                    for (int i = 0; i < pem.Length; i += lineLength)
                    {
                        int remainingLength = Math.Min(lineLength, pem.Length - i);
                        pemBuilder.AppendLine(pem.Substring(i, remainingLength));
                    }

                    pemBuilder.AppendLine("-----END PRIVATE KEY-----");

                    return pemBuilder.ToString();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                // Handle the exception here or log it for debugging
                Console.WriteLine("Error exporting private key: " + ex.Message);
                return string.Empty; // Return an appropriate value or handle the error as needed
            }
        }
        public static void ExportCertificateToFile(X509Certificate2 certificate, string certificateFilePath, string password)
        {
            try
            {
                File.WriteAllBytes(certificateFilePath, certificate.Export(X509ContentType.Pfx, password));
                Console.WriteLine("Certificate exported successfully with key. And its password protected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting certificate: {ex.Message}");
            }
        }
        static X509Certificate2 GenerateSignedCertificate(string subjectName, string keyFilePath, X509Certificate2 issuerCertificate, string password)
        {
            // Step 1: Generate a key pair and export the private key as key.pem
            using (RSA rsa = RSA.Create(2048))
            {
                // Step 2: Create a certificate request
                var request = new CertificateRequest(
                    new X500DistinguishedName($"CN={subjectName}"),
                    rsa,
                    HashAlgorithmName.SHA512,
                    RSASignaturePadding.Pkcs1);

                // Step 3: Create the certificate and sign it with the issuer's private key
                var certificate = request.Create(issuerCertificate, DateTimeOffset.Now, DateTimeOffset.Now.AddYears(8), Guid.NewGuid().ToByteArray());

                // Step 4: Create a certificate chain with the issuer's certificate
                X509Chain chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Skip revocation check
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                chain.ChainPolicy.ExtraStore.Add(issuerCertificate);

                // Step 5: Build and verify the certificate chain
                if (chain.Build(certificate))
                {
                    // Step 6: Export the certificate and private key as a PFX (PKCS#12) file with a password
                    var pfxBytes = certificate.Export(X509ContentType.Pkcs12, password);
                    var pfxCertificate = new X509Certificate2(pfxBytes, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                    // Step 7: Export the private key as key.pem file
                    ExportPrivateKeyToPemFile(rsa,password, keyFilePath);

                    return pfxCertificate;
                }
                else
                {
                    // Handle certificate chain validation failure (e.g., CA certificate not trusted)
                    throw new Exception("Certificate chain validation failed.");
                }
            }
        }

        private static void ExportPrivateKeyToPemFile(RSA privateKey, string password, string filePath)
        {
            var privateKeyBytes = privateKey.ExportEncryptedPkcs8PrivateKey(password, new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 1000));

            var pemBuilder = new StringBuilder();
            pemBuilder.AppendLine("-----BEGIN ENCRYPTED PRIVATE KEY-----");
            pemBuilder.AppendLine(Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks));
            pemBuilder.AppendLine("-----END ENCRYPTED PRIVATE KEY-----");

            File.WriteAllText(filePath, pemBuilder.ToString());
        }


        private static byte[] EncryptPrivateKey(byte[] privateKeyBytes, string password)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                var keyBytes = Encoding.UTF8.GetBytes(password);
                rsa.ImportEncryptedPkcs8PrivateKey(keyBytes, privateKeyBytes, out _);
                return rsa.ExportRSAPrivateKey();
            }
        }

        static void ExportPrivateKeyToPemFile(RSA privateKey, string filePath)
    {
        var privateKeyBytes = privateKey.ExportRSAPrivateKey();
        var pemBuilder = new StringBuilder();
        pemBuilder.AppendLine("-----BEGIN PRIVATE KEY-----");
        pemBuilder.AppendLine(Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks));
        pemBuilder.AppendLine("-----END PRIVATE KEY-----");
        File.WriteAllText(filePath, pemBuilder.ToString());
    }



        public static void ExportCertificateToFile(X509Certificate2 certificate, string filePath)
        {
            File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Cert));
            Console.WriteLine("Certificate exported successfully without private key");
        }
        public static void ExportCertificateWithPrivateKeyToFile(X509Certificate2 certificate, string filePath, string password)
        {
            byte[] pfxBytes = certificate.Export(X509ContentType.Pkcs12, password);
            File.WriteAllBytes(filePath, pfxBytes);
            Console.WriteLine("Certificate exported successfully with private key");
        }
    }
}
