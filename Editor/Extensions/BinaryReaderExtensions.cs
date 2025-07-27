using System.IO;

namespace GameMakerMount;

public static class BinaryReaderExtensions
{
	public static int[] ReadInt32Array( this BinaryReader br, int elementCount )
	{
		if ( elementCount < 1 )
			return [];
		
		var elements = new int[elementCount];
		for ( int i = 0; i < elementCount; i++ )
		{
			elements[i] = br.ReadInt32();
		}
		return elements;
	}
	
	public static string ReadGameMakerStringFromId( this BinaryReader br )
	{
		var offset = br.ReadInt32();
		var returnAddress = br.BaseStream.Position;
		var foundString = br.BaseStream.ReadNullTerminatedString(offset);
		br.BaseStream.Seek( returnAddress, SeekOrigin.Begin );
		return foundString;
	}
}
