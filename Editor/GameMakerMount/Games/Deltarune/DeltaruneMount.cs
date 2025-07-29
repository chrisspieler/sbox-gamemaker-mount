using System.Threading.Tasks;
using Sandbox.Mounting;

namespace GameMakerMount.Deltarune;

public class DeltaruneMount : GameMakerMount
{
	[ConCmd( "deltarune_refresh" )]
	public static void DeltaruneRefresh()
	{
		var mount = Directory.Get( "deltarune" );
		mount.RefreshInternal();
	}
	
	public override string Ident => "deltarune";
	public override string Title => "DELTARUNE";
	public override long AppId => 1671210;
	public override bool MultiArchive => true;
	protected override string MusicDirectory => "mus";

	protected override Task Mount( MountContext context )
	{
		var task = base.Mount( context );
		Log.Info( $"Mounted \"{Title}\": {IsMounted}" );
		return task;
	}
}
