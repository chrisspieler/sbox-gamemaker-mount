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
	protected override long AppId => 1671210;
	protected override bool MultiArchive => false;

	protected override Task Mount( MountContext context )
	{
		var task = base.Mount( context );
		Log.Info( $"Mounted \"{Title}\": {IsMounted}" );
		return task;
	}
}
