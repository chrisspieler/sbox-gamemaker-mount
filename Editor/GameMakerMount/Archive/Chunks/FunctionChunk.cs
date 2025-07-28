using System.IO;

namespace GameMakerMount;

public record FunctionChunk( ArchiveData ChunkData, int[] ElementOffsets ) 
	: ArchiveListChunk<FunctionChunk.Record>( ChunkData, ChunkMagic.Functions, ElementOffsets ), IArrayChunk<FunctionChunk>
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		string Name,
		int Occurrences,
		int Pointer
	) : ChunkRecord( Index, RecordData );
	
	public static int RecordSize => 12;
	
	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		return new Record(
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, RecordSize ),
			Name: br.ReadGameMakerString(),
			Occurrences: br.ReadInt32(),
			Pointer: br.ReadInt32()
		);
	}

}
