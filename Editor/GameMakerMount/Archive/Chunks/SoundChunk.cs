using System.IO;

namespace GameMakerMount;

public record SoundChunk( ArchiveData ChunkData, int ElementCount, int[] ElementOffsets ) 
	: ArchiveListChunk<SoundChunk.Record>( ChunkData, ArchiveFile.ChunkMagicSound, ElementCount, ElementOffsets )
{
	public record Record(
		int Index,
		ArchiveData RecordData,
		string Name,
		string FileName,
		float Volume,
		float Pitch,
		int AudioGroupId,
		int AudioId
	) : ChunkRecord( Index, RecordData );

	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		var name = br.ReadGameMakerStringFromId();
		var flags = br.ReadInt32();
		var type = br.ReadInt32();
		var fileName = br.ReadGameMakerStringFromId();
		var unk0 = br.ReadInt32();
		var volume = br.ReadSingle();
		var pitch = br.ReadSingle();
		var audioGroupId = br.ReadInt32();
		var audioId = br.ReadInt32();
		
		return new Record( 
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, (int)fs.Position - recordOffset ),
			Name: name,
			FileName: fileName,
			Volume: volume,
			Pitch: pitch,
			AudioGroupId : audioGroupId,
			AudioId: audioId
		);
	}
}
