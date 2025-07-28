using System.IO;

namespace GameMakerMount;

public record CodeChunk( ArchiveData ChunkData, int[] ElementOffsets ) 
	: ArchiveListChunk<CodeChunk.Record>( ChunkData, ChunkMagic.Code, ElementOffsets )
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		string Name,
		int Length,
		ushort LocalCount,
		ushort ArgumentCount,
		int BytecodeAddress,
		int Offset
	) : ChunkRecord( Index, RecordData );

	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		return new Record(
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, dataLength: sizeof(int) * 5 ),
			Name: br.ReadGameMakerString(),
			Length: br.ReadInt32(),
			LocalCount: br.ReadUInt16(),
			ArgumentCount: br.ReadUInt16(),
			BytecodeAddress: (int)fs.Position - br.ReadInt32(),
			Offset: br.ReadInt32()
		);
	}
}
