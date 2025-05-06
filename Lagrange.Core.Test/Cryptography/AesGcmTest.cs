using System.Security.Cryptography;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Test.Cryptography;

[TestFixture]
public class AesGcmTests
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    private static readonly int[] PlaintextSizes = [0, 1, 15, 16, 33, 64];

    [TestCase(128)]
    [TestCase(192)]
    [TestCase(256)]
    public void Encrypt_Decrypt_Matches_SystemAesGcm(int keyBits)
    {
        int keySize = keyBits / 8;
        byte[] key = new byte[keySize];
        byte[] nonce = new byte[12]; // Standard 12-byte nonce for GCM
        byte[] associatedData = new byte[17];

        Rng.GetBytes(key);
        Rng.GetBytes(nonce);
        Rng.GetBytes(associatedData);

        foreach (int ptSize in PlaintextSizes)
        {
            byte[] plaintext = new byte[ptSize];
            Rng.GetBytes(plaintext);

            byte[] expectedCiphertext = new byte[ptSize];
            byte[] expectedTag = new byte[AesGcmImpl.TagSize];
            using (var net = new AesGcm(key, AesGcmImpl.TagSize))
            {
                net.Encrypt(nonce, plaintext, expectedCiphertext, expectedTag, associatedData);
            }

            byte[] actualCiphertext = new byte[ptSize];
            byte[] actualTag = new byte[AesGcmImpl.TagSize];
            var impl = new AesGcmImpl(key);
            impl.Encrypt(nonce, plaintext, associatedData, actualCiphertext, actualTag);

            Assert.That(actualCiphertext, Is.EqualTo(expectedCiphertext), $"Ciphertext mismatch for {keyBits}-bit key, plaintext length {ptSize}");
            Assert.That(actualTag, Is.EqualTo(expectedTag), $"Tag mismatch for {keyBits}-bit key, plaintext length {ptSize}");

            byte[] decrypted = new byte[ptSize];
            impl.Decrypt(nonce, actualCiphertext, associatedData, actualTag, decrypted);
            Assert.That(decrypted, Is.EqualTo(plaintext), $"Decrypted plaintext mismatch for {keyBits}-bit key");
        }
    }

    [TestCase(128)]
    [TestCase(192)]
    [TestCase(256)]
    public void CrossDecrypt_CustomDecryptsSystemCipher(int keySizeBits)
    {
        byte[] key = new byte[keySizeBits / 8];
        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(key);
        RandomNumberGenerator.Fill(nonce);

        foreach (int ptLen in (int[])[0, 1, 16, 100, 1024])
        {
            byte[] plaintext = new byte[ptLen];
            byte[] aad = new byte[16];
            RandomNumberGenerator.Fill(plaintext);
            RandomNumberGenerator.Fill(aad);

            byte[] ciphertext = new byte[ptLen];
            byte[] tag = new byte[16];

            using (var sysAes = new AesGcm(key, AesGcmImpl.TagSize))
            {
                sysAes.Encrypt(nonce, plaintext, ciphertext, tag, aad);
            }

            var impl = new AesGcmImpl(key);
            byte[] decrypted = new byte[ptLen];
            impl.Decrypt(nonce, ciphertext, aad, tag, decrypted);

            Assert.That(decrypted, Is.EqualTo(plaintext), $"System→Custom cross-decrypt failed for keySize={keySizeBits}, ptLen={ptLen}");
        }
    }

    [TestCase(128)]
    [TestCase(192)]
    [TestCase(256)]
    public void CrossDecrypt_SystemDecryptsCustomCipher(int keySizeBits)
    {
        byte[] key = new byte[keySizeBits / 8];
        byte[] nonce = new byte[12];
        RandomNumberGenerator.Fill(key);
        RandomNumberGenerator.Fill(nonce);

        foreach (int ptLen in new[] { 0, 1, 16, 100, 1024 })
        {
            byte[] plaintext = new byte[ptLen];
            byte[] aad = new byte[16];
            RandomNumberGenerator.Fill(plaintext);
            RandomNumberGenerator.Fill(aad);

            byte[] ciphertext = new byte[ptLen];
            byte[] tag = new byte[16];

            var impl = new AesGcmImpl(key);
            impl.Encrypt(nonce, plaintext, aad, ciphertext, tag);

            byte[] decrypted = new byte[ptLen];
            using (var sysAes = new AesGcm(key, AesGcmImpl.TagSize))
            {
                sysAes.Decrypt(nonce, ciphertext, tag, decrypted, aad);
            }

            Assert.That(decrypted, Is.EqualTo(plaintext), $"Custom→System cross-decrypt failed for keySize={keySizeBits}, ptLen={ptLen}");
        }
    }
        
        
    [OneTimeTearDown]
    public void Cleanup()
    {
        Rng.Dispose();
    }
}