namespace GameMakerMount;

public record ArchiveChunk( ArchiveData ChunkData, string Magic  )
{
	public string ChunkMagic => Magic;
}
