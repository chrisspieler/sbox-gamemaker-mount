using System.IO;

namespace GameMakerMount;

public readonly struct ArchiveData
{
	public ArchiveData(ArchiveFile archive, int offset, int dataLength)
	{
		Archive = archive;
		Offset = offset;
		DataLength = dataLength;
	}

	public ArchiveFile Archive { get; }
	public int Offset { get; }
	public int DataLength { get; }

	public byte[] GetData()
	{
		var fs = System.IO.File.OpenRead( Archive.FilePath );
		var br = new BinaryReader( fs );
		
		fs.Seek( Offset, SeekOrigin.Begin );
		return br.ReadBytes( DataLength );
	}

	public static IDisposable Scope( ArchiveFile file, int offset )
	{
		return new DataScope( file, offset );
	}
	
	public class DataScope : IDisposable
	{
		public DataScope( ArchiveFile archive, int offset )
		{
			Archive = archive;
			Stream = File.OpenRead( archive.FilePath );
			Reader = new BinaryReader( Stream );

			if ( offset <= 0 ) return;
			
			StartOffset = offset;
			Stream.Seek( offset, SeekOrigin.Begin );
		}

		public ArchiveFile Archive { get; }
		public int StartOffset { get; }
		public FileStream Stream { get; }
		public BinaryReader Reader { get; }
		/// <summary>
		/// You can get this after the scope is disposed.
		/// </summary>
		public ArchiveData Data => new ArchiveData( Archive, StartOffset, (int)(Stream.Position - StartOffset) );

		public void Dispose()
		{
			if ( Stream.Position - StartOffset < 1 )
			{
				throw new InvalidOperationException( $"Cannot end {nameof(ArchiveData)} {nameof(Scope)} stream at a position earlier than the start position." );
			}
			Stream.Dispose();
			Reader.Dispose();
		}
	}
}
