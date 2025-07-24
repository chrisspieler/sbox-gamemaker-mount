namespace GameMakerMount;

public record ArchiveSpan( ArchiveFile File, int Offset, int DataLength ) : IArchiveSpan
{
}

public interface IArchiveSpan
{
	ArchiveFile File { get; }
	int Offset { get; }
	int DataLength { get; }
}
