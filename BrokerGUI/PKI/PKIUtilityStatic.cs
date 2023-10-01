using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;

namespace Broker.PKI
{
    internal class PKIUtilityStatic
    {
        public static X509Certificate2 ReadCertificateWithPrivateKey(string fileCertPath, string keyPath, string password)
        {
            try
            {
                string keyPem = File.ReadAllText(keyPath);
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromEncryptedPem(keyPem, password);
                    var certificate = new X509Certificate2(fileCertPath, password);
                    var certificate2 = certificate.CopyWithPrivateKey(rsa);
                    return certificate2;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading certificate with private key: {ex.Message} {ex.StackTrace}");
                return null;
            }
        }
    }
}
