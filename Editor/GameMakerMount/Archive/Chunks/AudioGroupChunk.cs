using System.IO;

namespace GameMakerMount;

public record AudioGroupChunk( ArchiveData ChunkData, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<AudioGroupChunk.Record>( ChunkData, ArchiveFile.ChunkMagicAudioGroup, ElementCount, ElementOffsets )
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		string Name
	) : ChunkRecord( Index, RecordData );

	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		var name = br.ReadGameMakerString();
		Log.Info( $"Audio group {recordIndex}: {name}" );
		
		return new Record(
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ),
			Name: name
		);
	}
}
