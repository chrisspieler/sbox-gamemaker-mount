using System.Threading.Tasks;
using Sandbox.Mounting;

namespace GameMakerMount.Undertale;

public class UndertaleMount : GameMakerMount
{
	public override string Ident => "undertale";
	public override string Title => "Undertale";
	protected override long AppId => 391540;
	
	protected override Task Mount( MountContext context )
	{
		// This game throws exceptions when mounting, so don't call the base mount for now.
		IsMounted = false;
		return Task.CompletedTask;
	}
}
