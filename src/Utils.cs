using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ModernMT
{
    public static class Utils
    {
        public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509Key)
        {
            // encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] oid = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

            // stream to read the asn.1 encoded SubjectPublicKeyInfo blob
            var mem = new MemoryStream(x509Key);
            var binr = new BinaryReader(mem);   // wrap MemoryStream with BinaryReader for easy reading

            try 
            {
                var btx2 = binr.ReadUInt16();
                switch (btx2)
                {
                    case 0x8130:    // data read as little endian order (actual data order for sequence is 30 81)
                        binr.ReadByte();    // advance 1 byte
                        break;
                    case 0x8230:
                        binr.ReadInt16();   // advance 2 bytes
                        break;
                    default:
                        return null;
                }

                var seq = binr.ReadBytes(15);   // read the sequence OID
                if (!CompareBytearrays(seq, oid))   // make sure sequence for OID is correct
                    return null;

                btx2 = binr.ReadUInt16();
                switch (btx2)
                {
                    
                    case 0x8103:    // data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    // advance 1 byte
                        break;
                    case 0x8203:
                        binr.ReadInt16();   // advance 2 bytes
                        break;
                    default:
                        return null;
                }

                var bt = binr.ReadByte();
                if (bt != 0x00) // expect null byte next
                    return null;

                btx2 = binr.ReadUInt16();
                switch (btx2)
                {
                    case 0x8130:    // data read as little endian order (actual data order for sequence is 30 81)
                        binr.ReadByte();    // advance 1 byte
                        break;
                    case 0x8230:
                        binr.ReadInt16();   // advance 2 bytes
                        break;
                    default:
                        return null;
                }

                byte lowByte;
                byte highByte;
                
                btx2 = binr.ReadUInt16();
                switch (btx2)
                {
                    case 0x8102:    // data read as little endian order (actual data order for Integer is 02 81)
                        lowByte = binr.ReadByte();  // read next bytes which is bytes in modulus
                        highByte = 0x00;
                        break;
                    case 0x8202:
                        highByte = binr.ReadByte(); // advance 2 bytes
                        lowByte = binr.ReadByte();
                        break;
                    default:
                        return null;
                }

                // reverse byte order since asn.1 key uses big endian order
                byte[] modInt = { lowByte, highByte, 0x00, 0x00 };  
                var modSize = BitConverter.ToInt32(modInt, 0);
                var firstByte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstByte == 0x00)  // if first byte (highest order) of modulus is zero, don't include it
                {
                    binr.ReadByte();    // skip this null byte
                    modSize -= 1;   // reduce modulus buffer size by 1
                }

                var modulus = binr.ReadBytes(modSize);  // read the modulus bytes

                if (binr.ReadByte() != 0x02)    // expect an Integer for the exponent data
                    return null;
                
                // only need one byte for actual exponent data (for all useful values)
                int expBytes = binr.ReadByte(); 
                var exponent = binr.ReadBytes(expBytes);

                // create RSACryptoServiceProvider instance and initialize with public key
                var rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = exponent
                });
                
                return rsa;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                binr.Close();
            }
        }
        
        private static bool CompareBytearrays(IReadOnlyCollection<byte> a, IReadOnlyList<byte> b)
        {
            if (a.Count != b.Count)
                return false;
            var i = 0;
            foreach (var c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
    }
}