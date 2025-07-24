using System.IO;
using Sandbox.Mounting;

namespace GameMakerMount;

public abstract class GameMakerArchiveResource : ResourceLoader<GameMakerMount>
{
	protected GameMakerArchiveResource( ArchiveFile archive, int offset, int dataLength )
	{
		if ( offset < 0 || offset + dataLength > archive.DataLength )
		{
			throw new IndexOutOfRangeException(
				 "$\"Offset {offset:X8} + length {dataLength:X8} would exceed length of archive file ({archive.DataLength:X8})\""
			);
		}

		Archive = archive;
		Offset = offset;
		DataLength = dataLength;
	}
	
	public ArchiveFile Archive { get; }
	public int Offset { get; }
	public int DataLength { get; }
	
	protected override object Load()
	{
		using var fs = File.OpenRead( Archive.FilePath );
		using var br = new BinaryReader( fs );
		
		fs.Seek( Offset, SeekOrigin.Begin );
		return br.ReadBytes( DataLength );
	}
}
