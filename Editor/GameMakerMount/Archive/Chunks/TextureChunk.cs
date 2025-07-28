using System.IO;

namespace GameMakerMount;

public record TextureChunk( ArchiveData ChunkData, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<TextureChunk.Record>( ChunkData, ArchiveFile.ChunkMagicTexture, ElementCount, ElementOffsets )
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		int Scaled,
		int GeneratedMips,
		Vector2Int Size,
		ArchiveData TextureData
	) : ChunkRecord( Index, RecordData );
	
	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		int scaled = br.ReadInt32();
		int generatedMips = br.ReadInt32();
		int dataSize = br.ReadInt32();
		int sizeX = br.ReadInt32();
		int sizeY = br.ReadInt32();
		int unk1 = br.ReadInt32();
		int addr = br.ReadInt32();
		
		var textureData = new ArchiveData( 
				archive: ChunkData.Archive, 
				offset: addr, 
				dataLength: dataSize 
			);

		return new Record(
				Index: recordIndex,
				RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ),
				Scaled: scaled,
				GeneratedMips: generatedMips,
				Size: new Vector2Int( sizeX, sizeY ),
				TextureData: textureData 
			);
	}
}
