using System.Security.Cryptography.X509Certificates;

namespace Client.PKI
{
    internal class PKIUtilityStatic
    {
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
