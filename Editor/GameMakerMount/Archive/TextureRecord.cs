using System.IO;

namespace GameMakerMount;

public record TextureRecord( ArchiveFile File, int Offset, int DataLength, int SizeX, int SizeY )
	: ArchiveSpan( File, Offset, DataLength )
{
	public static IEnumerable<TextureRecord> LoadRecords( ArchiveListChunk textureChunk, FileStream fs, BinaryReader br )
	{
		for ( int i = 0; i < textureChunk.ElementCount; i++ )
		{
			fs.Seek( textureChunk.ElementOffsets[i], SeekOrigin.Begin );
			yield return ReadTexture( textureChunk.File, fs, br );
		}
	}
	
	public static TextureRecord ReadTexture( ArchiveFile file, FileStream fs, BinaryReader br )
	{
		int scaled = br.ReadInt32();
		int generatedMips = br.ReadInt32();
		int dataSize = br.ReadInt32();
		int sizeX = br.ReadInt32();
		int sizeY = br.ReadInt32();
		int unk1 = br.ReadInt32();
		int addr = br.ReadInt32();
		Log.Info( $"{fs.Position:X8} Add texture at {addr:X8} width: {sizeX}, height: {sizeY}, dataSize: {dataSize}" );
		return new TextureRecord( file, addr, dataSize, sizeX, sizeY );
	}
}
