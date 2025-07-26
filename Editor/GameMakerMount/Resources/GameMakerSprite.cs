using Sandbox.Mounting;

namespace GameMakerMount;

public class GameMakerSprite( SpriteChunk.Record Sprite ) : ResourceLoader<GameMakerMount>
{
	protected override object Load()
	{
		Log.Info( $"Loading sprite with offsets {Sprite.TextureOffsets}" );
		var material = Material.Create( Sprite.Name, "shaders/gamemaker_sprite.shader" );
		// material.Attributes.Set( "AtlasTexture", );
		return material;
	}
}
