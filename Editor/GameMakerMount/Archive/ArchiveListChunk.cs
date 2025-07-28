using System.IO;

namespace GameMakerMount;

public record ArchiveListChunk( ArchiveData ChunkData, string Magic, int[] ElementOffsets )
	: ArchiveChunk( ChunkData, Magic )
{
	public static int[] GetRecordOffsets( ArchiveData chunkData, BinaryReader reader )
	{
		// Skip magic and data size.
		reader.BaseStream.Position = chunkData.Offset + 8;
		
		var elementCount = reader.ReadInt32();
		return reader.ReadInt32Array( elementCount );
	}
}

public abstract record ArchiveListChunk<TElement>( ArchiveData ChunkData, string Magic, int[] ElementOffsets )
	: ArchiveListChunk( ChunkData, Magic, ElementOffsets )
{
	public IEnumerable<TElement> Records
	{
		get
		{
			if ( !IsLoaded )
			{
				ReadAllRecordsFromDisk();
			}
			return _records;
		}
	}
	private TElement[] _records = [];

	public bool IsLoaded => ElementOffsets.Length > 0 && _records.Length == ElementOffsets.Length;
	protected abstract TElement ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br );
	
	public void ReadAllRecordsFromDisk()
	{
		_records = new TElement[ElementOffsets.Length];
		
		using var fs = File.OpenRead( ChunkData.Archive.FilePath );
		using var br = new BinaryReader( fs );
		for ( int i = 0; i < ElementOffsets.Length; i++ )
		{
			fs.Seek( ElementOffsets[i], SeekOrigin.Begin );
			_records[i] = ReadRecord( i, (int)fs.Position, fs, br );
		}
	}

}
