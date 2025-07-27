using System.Text.Json.Serialization;

public class SpriteTextureTester : Component, Component.ExecuteInEditor
{
	[Property] public MountManager MountManager { get; set; }
	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public Material Material { get; set; }
	
	[Property] public string SpriteName { get; set; }
	[Property, ReadOnly, JsonIgnore]
	public bool SpriteNameIsValid { get; private set; }
	[Button]
	public void SelectRandomSprite()
	{
		if ( !MountManager.IsValid() || MountManager.Sprites.Count < 1 )
			return;

		var sprite = MountManager.Sprites.ElementAt( Game.Random.Int( 0, MountManager.Sprites.Count - 1 ) );
		SpriteName = sprite.Key;
	}
	
	[Property] public int AtlasTextureIndex { get; set; }
	[Property] public Vector2 AtlasTextureSize { get; set; }
	[Property, Range( 0f, 2096 ), Step( 1 )]
	public float PositionX { get; set; }
	[Property, Range( 0f, 2096 ), Step( 1 ) ]
	public float PositionY { get; set; }
	[Property, Range( 0f, 2096), Step( 1 )]
	public float Width { get; set; } = 20;
	[Property, Range( 0f, 2096), Step( 1 )]
	public float Height { get; set; } = 20;



	protected override void OnPreRender()
	{
		if ( !MountManager.IsValid() || SpriteName is null )
			return;

		SpriteNameIsValid = MountManager.Sprites.ContainsKey( SpriteName );

		if ( !SpriteNameIsValid || !Renderer.IsValid() )
			return;

		if ( !Material.IsValid() )
		{
			Material = Material.FromShader( "shaders/gamemaker_sprite.shader" );
			Renderer.MaterialOverride = Material;
		}

		Renderer.MaterialOverride = Material;
		Renderer.SceneObject.Batchable = false;
		Renderer.SceneObject.Flags.IsTranslucent = true;
		Renderer.SceneObject.Flags.IsOpaque = false;

		LoadSpriteInfo( MountManager.Sprites[SpriteName] );

		LocalScale = new Vector3( Height, Width, 1f ) / 50f;
		
		var pageRect = new Vector4( PositionX, PositionY, Width, Height );
		UpdateAttributes( AtlasTextureIndex, AtlasTextureSize, pageRect );
	}

	private void LoadSpriteInfo( AtlasSprite spriteInfo )
	{
		AtlasTextureIndex = spriteInfo.TextureIndex;
		AtlasTextureSize = spriteInfo.AtlasSize;
		PositionX = spriteInfo.TexturePageRect.x;
		PositionY = spriteInfo.TexturePageRect.y;
		Width = spriteInfo.TexturePageRect.z;
		Height = spriteInfo.TexturePageRect.w;
	}

	private void UpdateAttributes( int textureIndex, Vector2 atlasSize, Vector4 texturePageRect )
	{
		Renderer.Attributes.Set( "g_iAtlasTextureIndex", textureIndex );
		Renderer.Attributes.Set( "g_vAtlasSize", atlasSize );
		PositionX = texturePageRect.x;
		PositionY = texturePageRect.y;
		Width = texturePageRect.z;
		Height = texturePageRect.w;
		Renderer.Attributes.Set( "g_vTexturePageRect", texturePageRect );
	}
}
