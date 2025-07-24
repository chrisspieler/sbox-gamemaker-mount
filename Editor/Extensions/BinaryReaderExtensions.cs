using System.IO;

namespace Sandbox.Extensions;

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
}
