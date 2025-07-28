using System.IO;

namespace GameMakerMount;

public interface IArrayChunk
{
	/// <summary>
	/// The offset of the record count relative to the start of the chunk.
	/// </summary>
	static virtual int RecordCountOffset => 8;
	
	/// <summary>
	/// The offset of the first record of this chunk relative to the start of the chunk.
	/// </summary>
	static virtual int RecordBaseAddressOffset => 12;
	
	/// <summary>
	/// The size in bytes of each record in the chunk.
	/// </summary>
	static abstract int RecordSize { get; }
}

public interface IArrayChunk<TChunk> : IArrayChunk where TChunk : ArchiveListChunk, IArrayChunk<TChunk>
{
	public static int[] GetRecordOffsets( ArchiveData chunkData, BinaryReader reader )
	{
		reader.BaseStream.Position = chunkData.Offset + TChunk.RecordCountOffset;
		var baseAddress = chunkData.Offset + TChunk.RecordBaseAddressOffset;
		var recordCount = reader.ReadInt32();
		var recordOffsets = new int[recordCount];
		
		for ( int i = 0; i < recordCount; i++ )
		{
			recordOffsets[i] = baseAddress + i * TChunk.RecordSize;
		}
		
		return recordOffsets;
	}
}
