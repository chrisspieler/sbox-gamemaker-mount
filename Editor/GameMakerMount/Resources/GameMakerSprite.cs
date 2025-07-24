using System.IO;
using System.Threading.Tasks;

namespace GameMakerMount;

public class GameMakerSprite : GameMakerArchiveResource
{
	public GameMakerSprite(ArchiveFile archive, int offset, int dataLength) : base(archive, offset, dataLength)
	{
		
	}

	protected override object Load()
	{
		using var ms = new MemoryStream( (byte[])base.Load() );
		using var br = new BinaryReader( ms );


		return null;
	}
}
