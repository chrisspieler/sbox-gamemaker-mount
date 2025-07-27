using System.IO;
using UndertaleModLib.Util;
using FileSystem = Sandbox.FileSystem;

namespace GameMakerMount;

public class GameMakerTexture( TextureChunk.Record textureRecord )
	: GameMakerArchiveResource( textureRecord.TextureData )
{
	protected override object LoadFromData( MemoryStream ms, BinaryReader br )
	{
		// TODO: Validate this magic to determine if BZ2, QOI, PNG, or whatever.
		var magic = new string( br.ReadChars( 4 ) );
		var unkFlags = br.ReadInt32();
		// How big is the data we are decompressing?
		var uncompressedSize = br.ReadInt32();
		
		var decompressedStream = Decompress();
		
		// Convert the decompressed QOI file to raw bytes.
		var imageData = QoiConverter.GetImageFromStream( decompressedStream, out int width, out int height );
		
		Log.Info( $"Creating {width}x{height} texture" );
		
		// Create a texture using that raw data. Apparently it's BGRA.
		var texture = Texture.Create( width, height, ImageFormat.BGRA8888 )
			.WithData( imageData )
			.WithName( Host.GetRelativeFilePathForRecord( textureRecord ) )
			.Finish();
		return texture;

		Stream Decompress()
		{
			Log.Info( "Uncompressed size: " + uncompressedSize );
			var buffer = new byte[uncompressedSize];
			var unzipStream = new BZip2InputStream( ms );
			var bytesRead = unzipStream.Read( buffer );
			Log.Info( $"Read {bytesRead} bytes from BZ2 stream" );
		
			return new MemoryStream( buffer );
		}
	}
}
