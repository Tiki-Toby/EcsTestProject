using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace XFlow.P2P
{
    public class ClientAddr
    {
        public const int ADDR_LEN = 4;
        
        public byte[] Address;
        public string AddressString;

        public ClientAddr(string str)
        {
            if (str.Length != ADDR_LEN)
            {
                throw new Exception($"'{str}'.Lenght != {ADDR_LEN}");
            }

            AddressString = str;
            Address = Encoding.UTF8.GetBytes(str);
        }
    }
    
    public static class P2P
    {
        public static readonly ClientAddr ADDR_SERVER = new ClientAddr("serv");
        public static readonly ClientAddr ADDR_BROADCAST = new ClientAddr("****");

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
        
        
        
        public static byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            //using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Compress))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
                
            //using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
        
        
        public static byte[] BuildRequest<T>(ClientAddr addr, T body) where T:class
        {
            var dataStr =  JsonUtility.ToJson(body);
            var data = Compress(Encoding.UTF8.GetBytes(dataStr));

            return Combine(addr.Address, data);
        }

        public static T ParseResponse<T>(byte[] body) where T : class
        {
            var decoded = Decompress(body);
            var json = Encoding.UTF8.GetString(decoded);
            return JsonUtility.FromJson<T>(json);
        }

        public static bool CheckError(byte[] body, out string errAddr)
        {
            errAddr = "";
            if (body.Length == 4)
            {
                
                errAddr = Encoding.UTF8.GetString(new ReadOnlySpan<byte>(body, 0, 4));
                //if (int.TryParse(str, out errAddr))
                return true;                   
                
            }

            return false;
        }

        public static string GetDevRoom()
        {
            var name = Environment.MachineName;            
            byte[] encoded = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(name));
            //readable room id
            var value = 1000 + BitConverter.ToUInt32(encoded, 0) % 5000;
            return value.ToString();
        }
    }    
}
