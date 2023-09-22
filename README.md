# MQTTNetTLS


## PLAN TO FIX THIS SHIT!

1. Modify function PKIGenerator GenerateSignedCertificate
- it shouldnt internally save key pem
- it should return map/touple certificate + key
- it cannot return certificate with internal key (i tried didnt work)
- it should save the key and cert in PKIGenerator/PKI/Server/ BRRRROKER11
- it should also save key and cert in Server/PKI/Server

2. Everyting should be in .pem for keys and .der/.pfx for certs ( i guess it will be easier to make pfx of rootCA)

3. rootCa even though it's self-signed shouldn't have key in certificate (and i does not have now). Private key is private

4. Implement in Client:

- private static bool OnCertificateValidation(MqttClientCertificateValidationEventArgs args)
- this function should validate certificate supplied by server by comparing against rootCA that it has alrealy supplied
- in order to make it work you might need to
    - modify signing/generating server certificate (there are different flags that might be needed)
    - in Client add to trusted CAs before you check if it's valid (gpt might help)
    - modify functions:  AddCAToTrusted,isCertificateValid, ReadCertificateFromFile
    - RootCA has different format (.der), not sure how it affects


5. Try to make mutual auth with client

