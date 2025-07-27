using System.IO;
using Sandbox.Diagnostics;
using Sandbox.Mounting;

namespace GameMakerMount;

public abstract class GameMakerArchiveResource : ResourceLoader<GameMakerMount>
{
	protected GameMakerArchiveResource( ArchiveData archiveData )
	{
		if ( archiveData.Offset < 0 || archiveData.Offset + archiveData.DataLength > archiveData.Archive.DataLength )
		{
			Log.Info(
				 $"Offset {archiveData.Offset:X8} + length {archiveData.DataLength:X8} would exceed length of archive file ({archiveData.Archive.DataLength:X8}). Just thought you'd want to know."
			);
		}

		Archive = archiveData.Archive;
		FileOffset = archiveData.Offset;
		Data = archiveData.GetData();
		DataLength = Data.Length;
		
		Assert.AreEqual( DataLength, archiveData.DataLength );
	}

	public ArchiveFile Archive { get; }
	public int FileOffset { get; }
	public int DataLength { get; }
	private byte[] Data { get; }

	protected override object Load()
	{
		using var memoryStream = new MemoryStream( Data );
		using var reader = new BinaryReader( memoryStream );

		return LoadFromData( memoryStream, reader );
	}

	protected abstract object LoadFromData( MemoryStream ms, BinaryReader br );
}
