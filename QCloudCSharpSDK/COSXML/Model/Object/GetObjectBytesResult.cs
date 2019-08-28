

using System;
using System.Collections.Generic;
using System.IO;

namespace COSXML.Model.Object
{
    public sealed class GetObjectBytesResult : CosResult
    {
        public string eTag;

        public byte[] content;

        private COSXML.Callback.OnProgressCallback progressCallback;


        internal override void ExternInfo(CosRequest cosRequest)
        {
            GetObjectBytesRequest getObjectBytesRequest = cosRequest as GetObjectBytesRequest;
            this.progressCallback = getObjectBytesRequest.GetCosProgressCallback();
        }

        internal override void InternalParseResponseHeaders()
        {
            List<string> values;
            this.responseHeaders.TryGetValue("ETag", out values);
            if (values != null && values.Count > 0)
            {
                eTag = values[0];
            }
        }


        internal override void ParseResponseBody(Stream inputStream, string contentType, long contentLength)
        {
            //content = new byte[contentLength];
            //int recvLen = inputStream.Read(content, 0, content.Length);
            //int completed = 0;
            //while (recvLen != 0)
            //{
            //    completed += recvLen;
            //    if (progressCallback != null)
            //    {
            //        progressCallback(completed, content.Length);
            //    }
            //    recvLen = inputStream.Read(content, recvLen, content.Length - completed);
            //}
            content = ReadToEnd(inputStream);//替换原来 没有读取完整的inputStream bug
        }

        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

    }


}
