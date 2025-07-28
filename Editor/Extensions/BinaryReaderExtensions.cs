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
	
	/// <summary>
	/// Reads an int from the stream, jumps to the absolute offset that was read, reads a null-terminated string,
	/// then returns to the position immediately after the int that was first read, and returns the string that was
	/// read.
	/// </summary>
	public static string ReadGameMakerString( this BinaryReader br )
	{
		var offset = br.ReadInt32();
		if ( offset <= 0 )
			return null;
		
		var returnAddress = br.BaseStream.Position;
		var foundString = br.BaseStream.ReadNullTerminatedString(offset);
		br.BaseStream.Seek( returnAddress, SeekOrigin.Begin );
		return foundString;
	}

	public static Guid ReadGuid( this BinaryReader br ) => new Guid( br.ReadBytes( 16 ) );
	public static Vector2Int ReadVector2Int( this BinaryReader br ) => new Vector2Int( br.ReadInt32(), br.ReadInt32() );
}
