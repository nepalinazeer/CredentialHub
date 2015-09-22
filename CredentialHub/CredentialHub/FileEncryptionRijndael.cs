/*
*Serializes/Deserializes Credential object, encrypts/decrypts it and writes/reads to/from file
*Thanks to code project example from http://www.codeproject.com/Articles/5719/Simple-encrypting-and-decrypting-data-in-C
*/
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace CredentialHub
{
    public static class FileEncryptionRijndael
    {
        private readonly static byte[] SALT = { 0x26, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x22, 0x3c };
        private readonly static string password = "e2gGhWYDq7AAzyrT";
        public static void EncryptAndSaveToFile(object obj, string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();

                bf.Serialize(ms, obj);
                byte[] contentToWrite = Encrypt(ms.ToArray());

                using (Stream file = File.OpenWrite(fileName))
                {
                    file.Write(contentToWrite, 0, contentToWrite.Length);
                }
            }
           
        }

        public static object ReadFromFileAndDecrypt(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            using (var ms = new MemoryStream())
            {
                byte[] plainByte = Decrypt(bytes);
                ms.Write(plainByte, 0, plainByte.Length);
                ms.Seek(0, SeekOrigin.Begin);

                var bf = new BinaryFormatter();
                Object obj = (Object)bf.Deserialize(ms);
                return obj;
            }

        }

        private static byte[] Encrypt(byte[] clearData)
        {

            MemoryStream ms = new MemoryStream();

            Rijndael alg = Rijndael.Create();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, SALT);
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            alg.Padding = PaddingMode.PKCS7;

            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] retVal = ms.ToArray();
            return retVal;

        }

        private static byte[] Decrypt(byte[] cipherData)
        {
            MemoryStream ms = new MemoryStream();

            Rijndael alg = Rijndael.Create();
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(password, SALT);
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);
            alg.Padding = PaddingMode.PKCS7;

            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] retVal = ms.ToArray();
            return retVal;

        }
    }
}
