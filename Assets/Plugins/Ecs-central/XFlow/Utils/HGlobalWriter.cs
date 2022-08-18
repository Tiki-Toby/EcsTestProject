using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace XFlow.Utils
{
    public class HGlobalWriter: IDisposable
    {
        private IntPtr allocatedBuffer;
        private IntPtr buffer;
        
        private int bufferSize;

        public HGlobalWriter()
        {
            bufferSize = 255;
            buffer = allocatedBuffer = Marshal.AllocHGlobal(bufferSize);
        }

        public void Dispose()
        {
            if (allocatedBuffer != IntPtr.Zero)
                Marshal.FreeHGlobal(allocatedBuffer);
            allocatedBuffer = IntPtr.Zero;
            buffer = IntPtr.Zero;
        }

        private void Extend(int size)
        {
            var pos = Convert.ToInt32(buffer.ToInt64() - allocatedBuffer.ToInt64());
            var reqSize = size + pos;
            if (reqSize > bufferSize)
            {
                reqSize += 64;
                do
                {
                    bufferSize *= 2;
                } while (bufferSize < reqSize);

                allocatedBuffer = Marshal.ReAllocHGlobal(allocatedBuffer, (IntPtr)bufferSize);
                buffer = allocatedBuffer + pos;
            }
        }

        public byte[] CopyToByteArray(byte[] array = null)
        {
            var pos = Convert.ToInt32(buffer.ToInt64() - allocatedBuffer.ToInt64());
            if (array == null)
                array = new byte[pos];
            Marshal.Copy(allocatedBuffer, array, 0, pos);
            return array;
        }

        public void WriteString(string str)
        {
            var array = Encoding.UTF8.GetBytes(str);
            WriteByteArray(array,true);
        }
        
        public void WriteListT<T>(List<T> data, bool writeSize) where T : struct
        {
            var count = data.Count;
            
            if (writeSize)
                WriteInt32(count);
            
            var oneSize = Marshal.SizeOf<T>();
            
            Extend(count * oneSize);

            for (int i = 0; i < count; ++i)
            {
                Marshal.StructureToPtr(data[i], buffer, false);
                buffer += oneSize;
            }
        }
        
        public void WriteStrings(List<string> data, bool writeSize)
        {
            var count = data.Count;
            
            if (writeSize)
                WriteInt32(count);
            
            for (int i = 0; i < count; ++i)
            {
                WriteString(data[i]);
            }
        }
        
        public void WriteByteArray(byte[] data, bool writeSize)
        {
            var count = data.Length;
            
            if (writeSize)
                WriteInt32(count);

            var size = count;
            Extend(count);

            Marshal.Copy(data, 0, buffer, size);
            buffer += size;
        }
        
        public void WriteInt32Array(List<int> data, bool writeSize)
        {
            var count = data.Count;
            
            if (writeSize)
                WriteInt32(count);
            
            Extend(count * sizeof(int));

            for (int i = 0; i < count; ++i)
            {
                Marshal.WriteInt32(buffer, data[i]);
                buffer += sizeof(int);
            }
        }
        
        public void WriteInt16Array(List<short> data, bool writeSize)
        {
            var count = data.Count;
            
            if (writeSize)
                WriteInt32(count);

            Extend(count * sizeof(short));

            for (int i = 0; i < count; ++i)
            {
                Marshal.WriteInt16(buffer, data[i]);
                buffer += sizeof(short);
            }
        }
        
        public void WriteSingleT<T>(T data) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            Extend(size);

            Marshal.StructureToPtr(data, buffer, false);
            buffer += size;
        }
        
        public void WriteInt32(int value)
        {
            WriteSingleT(value);
        }
        
        public void WriteInt16(short value)
        {
            WriteSingleT(value);
        }
        
        public void WriteByte(byte value)
        {
            WriteSingleT(value);
        }
        
        public void Reset()
        {
            buffer = allocatedBuffer;
        }
    }
}