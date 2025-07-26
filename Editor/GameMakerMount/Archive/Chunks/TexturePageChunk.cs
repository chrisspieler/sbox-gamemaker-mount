using System.IO;

namespace GameMakerMount;

public record TexturePageChunk( ArchiveData ChunkData, string Magic, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<TexturePageChunk.Record>( ChunkData, Magic,ElementCount, ElementOffsets )
{
	public record Record(
		ArchiveData RecordData,
		RectInt SourceRect,
		RectInt DestRect,
		Vector2Int BoundingSize,
		int TextureId
	) : ChunkRecord( RecordData );

	public override string ChunkMagic => ArchiveFile.ChunkMagicTexturePage;
	protected override Record ReadRecord( int recordOffset, FileStream fs, BinaryReader br )
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
		
		return new Record( 
				RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ),
				SourceRect: sourceRect, 
				DestRect: destRect, 
				BoundingSize: new Vector2Int( boundingWidth, boundingHeight ), 
				TextureId: textureId
			);
	}
}
