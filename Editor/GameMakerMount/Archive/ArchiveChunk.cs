namespace GameMakerMount;

public record ArchiveChunk( ArchiveFile File, int Offset, int DataLength, string Magic  ) 
	: ArchiveSpan( File, Offset, DataLength )
{
}