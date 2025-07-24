namespace GameMakerMount;

public record ArchiveListChunk( ArchiveFile File, int Offset, int DataLength, string Magic, int ElementCount, int[] ElementOffsets ) 
	: ArchiveChunk( File, Offset, DataLength, Magic  )
{ }