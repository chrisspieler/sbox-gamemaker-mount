using System.IO;

namespace GameMakerMount;

public record AudioGroupChunk( ArchiveData ChunkData, string Magic, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<AudioGroupChunk.Record>( ChunkData, Magic, ElementCount, ElementOffsets )
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		string Name
	) : ChunkRecord( Index, RecordData );

	public override string ChunkMagic => ArchiveFile.ChunkMagicAudioGroup;
	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		var name = br.ReadGameMakerStringFromId();
		Log.Info( $"Audio group {recordIndex}: {name}" );
		
		return new Record(
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ),
			Name: name
		);
	}
}
