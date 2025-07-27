using System.IO;

namespace GameMakerMount;

public record ArchiveListChunk( ArchiveData ChunkData, string Magic, int ElementCount, int[] ElementOffsets )
	: ArchiveChunk( ChunkData, Magic )
{
}

public abstract record ArchiveListChunk<TElement>( ArchiveData ChunkData, string Magic, int ElementCount, int[] ElementOffsets )
	: ArchiveListChunk( ChunkData, Magic, ElementCount, ElementOffsets )
{
	public abstract string ChunkMagic { get; }

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

	public bool IsLoaded => _records.Length == ElementCount;
	protected abstract TElement ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br );
	
	public void ReadAllRecordsFromDisk()
	{
		_records = new TElement[ElementCount];
		
		using var fs = File.OpenRead( ChunkData.Archive.FilePath );
		using var br = new BinaryReader( fs );
		for ( int i = 0; i < ElementCount; i++ )
		{
			fs.Seek( ElementOffsets[i], SeekOrigin.Begin );
			_records[i] = ReadRecord( i, (int)fs.Position, fs, br );
		}
	}

}
