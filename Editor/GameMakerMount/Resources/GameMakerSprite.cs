using Sandbox.Mounting;

namespace GameMakerMount;

public class GameMakerSprite( SpriteRecord Sprite ) : ResourceLoader<GameMakerMount>
{
	protected override object Load()
	{
		var material = Material.Create( Sprite.Name, "shaders/gamemaker_sprite.shader" );
		// material.Attributes.Set( "AtlasTexture", );
		return material;
	}
}
