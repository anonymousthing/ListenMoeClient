﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrappyListenMoe
{
	//Taken from the NAudio sample code
	public class ReadFullyStream : Stream
	{
		private readonly Stream sourceStream;
		private long pos;
		private readonly byte[] readAheadBuffer;
		private int readAheadLength;
		private int readAheadOffset;

		public ReadFullyStream(Stream sourceStream)
		{
			this.sourceStream = sourceStream;
			readAheadBuffer = new byte[4096];
		}
		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush()
		{
			throw new InvalidOperationException();
		}

		public override long Length
		{
			get { return pos; }
		}

		public override long Position
		{
			get
			{
				return pos;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}
		
		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesRead = 0;
			while (bytesRead < count)
			{
				int readAheadAvailableBytes = readAheadLength - readAheadOffset;
				int bytesRequired = count - bytesRead;
				if (readAheadAvailableBytes > 0)
				{
					int toCopy = Math.Min(readAheadAvailableBytes, bytesRequired);
					Array.Copy(readAheadBuffer, readAheadOffset, buffer, offset + bytesRead, toCopy);
					bytesRead += toCopy;
					readAheadOffset += toCopy;
				}
				else
				{
					readAheadOffset = 0;
					try
					{
						readAheadLength = sourceStream.Read(readAheadBuffer, 0, readAheadBuffer.Length);
					}
					catch (Exception e)
					{
						//Read will throw an exception when pausing due to the thread dying, so we just ignore it.
					}
					//Debug.WriteLine(String.Format("Read {0} bytes (requested {1})", readAheadLength, readAheadBuffer.Length));
					if (readAheadLength == 0)
					{
						break;
					}
				}
			}
			pos += bytesRead;
			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			//Not implemented, but it is called by the Vorbis decoder on the first frame.
			return 0;
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException();
		}
	}
}