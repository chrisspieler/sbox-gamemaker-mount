using Sandbox.Mounting;

namespace GameMakerMount;

public class GameMakerSprite( SpriteChunk.Record Sprite ) : ResourceLoader<GameMakerMount>
{
	protected override object Load()
	{
		if ( Sprite.TextureCount < 1 ) return null;

		var spriteArchive = Sprite.RecordData.Archive;
		
		var texturePages = new TexturePageChunk.Record[Sprite.TextureCount];
		
		for ( int i = 0; i < 1; i++ )
		{
			var texturePageOffset = Sprite.TextureOffsets[0];
			if ( !spriteArchive.TexturePageOffsets.TryGetValue( texturePageOffset, out var texturePage ) )
				return null;

			texturePages[i] = texturePage;
		}
		return GetSpriteInfoForTexturePage( texturePages[0] );
	}

	private string GetSpriteInfoForTexturePage( TexturePageChunk.Record texturePage )
	{
		var archive = texturePage.RecordData.Archive;
		
		var textureIndex = texturePage.TextureIndex;
		if ( textureIndex < 0 || textureIndex >= archive.Textures.Count )
		{
			Log.Info( $"Unable to find embedded texture at index {textureIndex} for sprite \"{Sprite.Name}\"" );
			return null;
		}

		var textureRecord = archive.Textures[textureIndex];
		var texturePath = Host.GetAbsoluteFilePathForRecord( textureRecord );
		var textureRes = Host.Resources.FirstOrDefault( t => System.IO.Path.GetRelativePath( t.Path, texturePath ) == "." );
		if ( textureRes is null )
		{
			Log.Info( $"Unable to find texture path: {texturePath}" );
			return null;
		}
		var textureResTask = textureRes.GetOrCreate();
		textureResTask.Wait();

		if ( textureResTask.Result is null )
		{
			Log.Info( $"Texture is null: {textureRes.Path}" );
			return null;
		}

		if ( textureResTask.Result is not Texture texture )
		{
			Log.Info( $"Resource is not a texture: {textureRes.Path}" );
			return null;
		}

		if ( !texture.IsValid() )
		{
			Log.Info( $"Texture is not valid: {textureRes.Path}" );
			return null;
		}

		if ( !texture.IsLoaded )
		{
			Log.Info( $"Texture is not loaded: {textureRes.Path}" );
			return null;
		}
		
		var spriteInfo = new AtlasSprite()
		{
			TextureIndex = texture.Index,
			AtlasSize = textureRecord.Size,
			TexturePageRect = new Vector4(
				x: texturePage.SourceRect.Position.x,
				y: texturePage.SourceRect.Position.y,
				z: texturePage.SourceRect.Size.x,
				w: texturePage.SourceRect.Size.y
			)
		};
		return Json.Serialize( spriteInfo );
	}
}
