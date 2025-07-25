using System.Threading.Tasks;
using Sandbox.Mounting;

namespace GameMakerMount.Games.RivalsOfAether;

public class RivalsMount : GameMakerMount
{
	public override string Ident => "rivals_of_aether";
	public override string Title => "Rivals of Aether";
	protected override long AppId => 383980;

	protected override Task Mount( MountContext context )
	{
		// This game throws exceptions when mounting, so don't call the base mount for now.
		IsMounted = false;
		return Task.CompletedTask;
	}
}
