using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace XFlow.Utils
{
    public class HGlobalReader: IDisposable
    {
        private IntPtr allocatedBuffer;
        private IntPtr buffer;
        
        public HGlobalReader(byte[] array)
        {
            allocatedBuffer = Marshal.AllocHGlobal(array.Length);
            buffer = allocatedBuffer;

            Marshal.Copy(array, 0, buffer, array.Length);
        }
        
        public List<int> ReadInt32Array(List<int> result = null)
        {
            var count = ReadInt32();

            if (result == null)
                result = new List<int>();
            else
                result.Clear();
           
            for (int i = 0; i < count; ++i)
            {
                result.Add(ReadInt32());
            }

            return result;
        }
        
        public List<short> ReadInt16Array(List<short> result = null)
        {
            var count = ReadInt32();

            if (result == null)
                result = new List<short>();
            else
                result.Clear();
           
            for (int i = 0; i < count; ++i)
            {
                result.Add(ReadInt16());
            }

            return result;
        }

        public int ReadInt32()
        {
            int value = Marshal.ReadInt32(buffer);
            buffer += 4;
            return value;
        }
        
        public short ReadInt16()
        {
            short value = Marshal.ReadInt16(buffer);
            buffer += 2;
            return value;
        }
        
        public byte ReadByte()
        {
            byte value = Marshal.ReadByte(buffer);
            buffer += 1;
            return value;
        }

        public List<T> ReadStructures<T>(List<T> result = null) where T : struct
        {
            if (result == null)
                result = new List<T>();
            result.Clear();

            var size = ReadInt32();

            var itemSize = Marshal.SizeOf<T>();
            for (int i = 0; i < size; ++i)
            {
                result.Add(Marshal.PtrToStructure<T>(buffer));
                buffer += itemSize;
            }

            return result;
        }
        
        public List<string> ReadStrings(List<string> result = null)
        {
            if (result == null)
                result = new List<string>();
            result.Clear();

            var size = ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                result.Add(ReadString());
            }

            return result;
        }

        public byte[] ReadByteArray()
        {
            var size = ReadInt32();
            var array = new byte[size];
            Marshal.Copy(buffer, array, 0, size);
            buffer += size;

            return array;
        }

        public T ReadStructure<T>() where T: struct
        {
            var itemSize = Marshal.SizeOf<T>();
            T data = Marshal.PtrToStructure<T>(buffer);
            buffer += itemSize;
            return data;
        }

        public string ReadString()
        {
            var array = ReadByteArray();
            return Encoding.UTF8.GetString(array);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(allocatedBuffer);
        }
    }
}