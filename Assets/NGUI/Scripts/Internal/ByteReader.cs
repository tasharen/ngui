//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2012 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// MemoryStream.ReadLine has an interesting oddity: it doesn't always advance the stream's position by the correct amount:
/// http://social.msdn.microsoft.com/Forums/en-AU/Vsexpressvcs/thread/b8f7837b-e396-494e-88e1-30547fcf385f
/// Solution? Custom line reader with the added benefit of not having to use streams at all.
/// </summary>

public class ByteReader
{
	byte[] mBuffer;
	int mOffset = 0;

	public ByteReader (byte[] bytes) { mBuffer = bytes; }
	public ByteReader (TextAsset asset) { mBuffer = asset.bytes; }

	/// <summary>
	/// Whether the buffer is readable.
	/// </summary>

	public bool canRead { get { return (mBuffer != null && mOffset < mBuffer.Length); } }

	/// <summary>
	/// Read a single line from the buffer.
	/// </summary>

	public string ReadLine()
	{
		int max = mBuffer.Length;

		// Skip empty characters
		while (mOffset < max && mBuffer[mOffset] < 32) ++mOffset;

		int end = mOffset;

		if (end < max)
		{
			for (; ; )
			{
				if (end < max)
				{
					int ch = mBuffer[end++];
					if (ch != '\n' && ch != '\r') continue;
				}
				else ++end;

				string line = Encoding.UTF8.GetString(mBuffer, mOffset, end - mOffset - 1);
				mOffset = end;
				return line;
			}
		}
		mOffset = max;
		return null;
	}

	/// <summary>
	/// Assume that the entire file is a collection of key/value pairs.
	/// </summary>

	public Dictionary<string, string> ReadDictionary ()
	{
		Dictionary<string, string> dict = new Dictionary<string, string>();
		char[] separator = new char[] { '=' };

		while (canRead)
		{
			string line = ReadLine();
			if (line == null) break;

			string[] split = line.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 2) dict.Add(split[0].Trim(), split[1].Trim());
		}
		return dict;
	}
}