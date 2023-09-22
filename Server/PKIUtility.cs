using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Net;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace PKIUtility
{
    internal class PKIUtility
    {     
        //this works for MQTT TLS
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
        }
        //static X509Certificate2 GenerateCACertificate(string subjectName)
        //{
        //    var sanBuilder = new SubjectAlternativeNameBuilder();
        //    sanBuilder.AddIpAddress(IPAddress.Loopback);
        //    sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        //    sanBuilder.AddDnsName("local.iot.project");

        //    string oid = "2.5.29.37.0";

        //    using (RSA rsa = RSA.Create(2048))
        //    {
        //        var request = new CertificateRequest(
        //            new X500DistinguishedName($"CN={subjectName}"),
        //            rsa,
        //            HashAlgorithmName.SHA256,
        //            RSASignaturePadding.Pkcs1);

        //        request.CertificateExtensions.Add(
        //          new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature| X509KeyUsageFlags.NonRepudiation | X509KeyUsageFlags.KeyCertSign, true));

        //        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(oid) }, false));

        //        request.CertificateExtensions.Add(sanBuilder.Build());

        //        request.CertificateExtensions.Add(
        //            new X509BasicConstraintsExtension(true, false, 0, true));




        //        return request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
        //    }
        //}

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


        public static X509Certificate2 ReadCertificateWithPrivateKey(string certificateFilePath, string privateKeyFilePath, string privateKeyPassword)
        {
            try

            {
                string privateKeyPem = File.ReadAllText(privateKeyFilePath);
                X509Certificate2 certificate = new X509Certificate2(certificateFilePath);
                RSA privateKey = GetPrivateKeyFromPEM(privateKeyPem, privateKeyPassword);
                X509Certificate2 certificateWithPrivateKey = certificate.CopyWithPrivateKey(privateKey);

                return certificateWithPrivateKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading certificate and private key: {ex.Message} {ex.StackTrace}");
                return null;
            }
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
                Console.WriteLine($"Error reading CA certificate: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
        public static bool VerifyCertificate(X509Certificate2 certificate, X509Certificate2 caCertificate)
        {
            try
            {
                using (X509Chain chain = new X509Chain())
                {
                    chain.ChainPolicy.ExtraStore.Add(caCertificate);
                    chain.Build(certificate);

                    // Check if the certificate chain status is valid
                    return chain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying certificate: {ex.Message}");
                return false;
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
                Console.WriteLine($"Error exporting private key: {ex.Message}");
            }
        }
        private static string GetPrivateKeyAsPEM(X509Certificate2 certificate, string password)
        {
            RSA privateKey = certificate.GetRSAPrivateKey();

            if (privateKey != null)
            {
                // Export the private key as a PEM string
                byte[] privateKeyBytes = privateKey.ExportRSAPrivateKey();
                System.Text.StringBuilder pemBuilder = new StringBuilder();
                pemBuilder.AppendLine("-----BEGIN ENCRYPTED PRIVATE KEY-----");
                pemBuilder.AppendLine(Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks));
                pemBuilder.AppendLine("-----END ENCRYPTED PRIVATE KEY-----");

                return pemBuilder.ToString();
            }
            return string.Empty;
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

        //static X509Certificate2 GenerateCertificate(string subjectName, X509Certificate2 issuerCertificate)
        //{
        //    using (RSA rsa = RSA.Create(2048))
        //    {
        //        var request = new CertificateRequest(
        //            new X500DistinguishedName($"CN={subjectName}"),
        //            rsa,
        //            HashAlgorithmName.SHA256,
        //            RSASignaturePadding.Pkcs1);

        //        // Create the certificate and sign it with the CA's private key

        //        var certificate = request.Create(
        //            issuerCertificate,
        //            DateTimeOffset.Now,
        //            DateTimeOffset.Now.AddYears(8),
        //            Guid.NewGuid().ToByteArray());

        //       var pfxCertificate = new X509Certificate2(
        //    certificate.Export(X509ContentType.Pfx),
        //    (string)null!,
        //    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

        //        return pfxCertificate;
        //    }
        //}
        static X509Certificate2 GenerateCertificate(string subjectName, X509Certificate2 issuerCertificate)
        {

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");

            string oid = "1.3.6.1.5.5.7.3.1";


            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(
                    new X500DistinguishedName($"CN={subjectName}"),
                    rsa,
                    HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(
                   new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

                request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new(oid) }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                // Create the certificate and sign it with the CA's private key
                var certificate = request.Create(
                    issuerCertificate,
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddYears(8),
                    Guid.NewGuid().ToByteArray());

                // Include the issuer certificate (CA certificate) in the certificate chain
                var chain = new X509Chain();
                chain.ChainPolicy.ExtraStore.Add(issuerCertificate);
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Skip revocation check
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

                // Build and verify the certificate chain
                if (chain.Build(certificate))
                {
                    // Export the certificate and private key as a PFX (PKCS#12) file
                    var pfxCertificate = new X509Certificate2(
                        certificate.Export(X509ContentType.Pfx),
                        (string)null!,
                        X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

                    return pfxCertificate;
                }
                else
                {
                    // Handle certificate chain validation failure (e.g., CA certificate not trusted)
                    throw new Exception("Certificate chain validation failed.");
                }

            }
        }
        public static void ExportCertificateToFile(X509Certificate2 certificate, string filePath)
        {
            File.WriteAllBytes(filePath, certificate.Export(X509ContentType.Cert));
            Console.WriteLine("Certificate exported successfully without private key");
        }



        private static RSA GetPrivateKeyFromPEM(string privateKeyPem, string privateKeyPassword)
        {
            // Parse the PEM-encoded private key and return an RSA object
            var rsa = RSA.Create();

            rsa.ImportFromEncryptedPem(privateKeyPem, privateKeyPassword);
            return rsa;
        }


    }
}
