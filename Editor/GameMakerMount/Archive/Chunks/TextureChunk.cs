using System.IO;

namespace GameMakerMount;

public record TextureChunk( ArchiveData Data, string Magic, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<TextureChunk.Record>( Data, Magic, ElementCount, ElementOffsets )
{
	public record Record( 
			int Scaled,
			int GeneratedMips,
			Vector2Int Size,
			ArchiveData TextureData
		)
	{
	}

	public override string ChunkMagic => "TXTR";

	protected override Record ReadRecord( int recordOffset, FileStream fs, BinaryReader br )
	{
		int scaled = br.ReadInt32();
		int generatedMips = br.ReadInt32();
		int dataSize = br.ReadInt32();
		int sizeX = br.ReadInt32();
		int sizeY = br.ReadInt32();
		int unk1 = br.ReadInt32();
		int addr = br.ReadInt32();
		
		var recordData = new ArchiveData( 
				archive: Data.Archive, 
				offset: addr, 
				dataLength: dataSize 
			);
		
		return new Record(
				scaled,
				generatedMips,
				new Vector2Int( sizeX, sizeY ),
				recordData 
			);
	}
}
