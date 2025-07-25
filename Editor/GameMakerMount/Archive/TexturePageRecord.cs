using System.IO;
using Sandbox.Extensions;

namespace GameMakerMount;

public record TexturePageRecord( RectInt SourceRect, RectInt DestRect, Vector2Int BoundingSize, int TextureId )
{
	public static IEnumerable<TexturePageRecord> LoadRecords( ArchiveListChunk texturePageChunk, FileStream fs, BinaryReader br )
	{
		for ( int i = 0; i < texturePageChunk.ElementCount; i++ )
		{
			fs.Seek( texturePageChunk.ElementOffsets[i], SeekOrigin.Begin );
			yield return ReadTexturePage( fs, br );
		}
	}
	
	public static TexturePageRecord ReadTexturePage( FileStream fs, BinaryReader br )
	{
		var sourceRect = new RectInt(
			x: br.ReadUInt16(),
			y: br.ReadUInt16(),
			width: br.ReadUInt16(),
			height: br.ReadUInt16()
		);
		var destRect = new RectInt(
			x: br.ReadUInt16(),
			y: br.ReadUInt16(),
			width: br.ReadUInt16(),
			height: br.ReadUInt16()
		);
		var boundingWidth = br.ReadUInt16();
		var boundingHeight = br.ReadUInt16();
		var textureId = br.ReadUInt16();
		return new TexturePageRecord( sourceRect, destRect, new Vector2Int( boundingWidth, boundingHeight ), textureId);
	}
};
