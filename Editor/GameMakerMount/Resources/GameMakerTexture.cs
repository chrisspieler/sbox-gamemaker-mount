using System.IO;
using UndertaleModLib.Util;
using FileSystem = Sandbox.FileSystem;

namespace GameMakerMount;

public class GameMakerTexture( TextureRecord texture )
	: GameMakerArchiveResource( texture.File, texture.Offset, texture.DataLength )
{
	protected override object Load()
	{
		var bytes = (byte[])base.Load();
		var ms = new MemoryStream( bytes );
		var br = new BinaryReader( ms );
		var magic = new string( br.ReadChars( 4 ) );
		var unkFlags = br.ReadInt32();
		var uncompressedSize = br.ReadInt32();
		
		Log.Info( "Uncompressed size: " + uncompressedSize );
		var buffer = new byte[uncompressedSize];
		var unzipStream = new BZip2InputStream( ms );
		var bytesRead = unzipStream.Read( buffer );
		Log.Info( $"Read {bytesRead} bytes from BZ2 stream" );
		
		var decompressedStream = new MemoryStream( buffer );
		var imageData = QoiConverter.GetImageFromStream( decompressedStream, out int width, out int height );
		Log.Info( $"Creating {width}x{height} texture" );
		return Texture.Create( width, height, ImageFormat.RGBA8888 )
			.WithData( imageData )
			.Finish();
	}
}
