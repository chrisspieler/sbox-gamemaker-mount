using System.IO;

namespace GameMakerMount;

public record ArchiveListChunk( ArchiveData Data, string Magic, int ElementCount, int[] ElementOffsets )
	: ArchiveChunk( Data, Magic )
{
}

public abstract record ArchiveListChunk<TElement>( ArchiveData Data, string Magic, int ElementCount, int[] ElementOffsets )
	: ArchiveListChunk( Data, Magic, ElementCount, ElementOffsets )
{
	public abstract string ChunkMagic { get; }
	
	public readonly TElement[] Records = new TElement[ElementCount];

	protected abstract TElement ReadRecord( int recordOffset, FileStream fs, BinaryReader br );
	
	public void ReadAllRecordsFromDisk()
	{
		using var fs = File.OpenRead( Data.Archive.FilePath );
		using var br = new BinaryReader( fs );
		for ( int i = 0; i < ElementCount; i++ )
		{
			fs.Seek( ElementOffsets[i], SeekOrigin.Begin );
			Records[i] = ReadRecord( (int)fs.Position, fs, br );
		}
	}

}
