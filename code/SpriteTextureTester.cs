public class SpriteTextureTester : Component, Component.ExecuteInEditor
{
	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public Texture Texture { get; set; }
	[Property, Range( 0f, 2096, 1)]
	public float PositionX { get; set; }
	[Property, Range( 0f, 2096, 1)]
	public float PositionY { get; set; }
	[Property, Range( 0f, 2096, 1)]
	public float Width { get; set; } = 20;
	[Property, Range( 0f, 2096, 1)]
	public float Height { get; set; } = 20;

	private Material _material;

	protected override void OnPreRender()
	{
		if ( !Renderer.IsValid() )
			return;

		if ( !Texture.IsValid() )
		{
			Renderer.ClearMaterialOverrides();
			_material = null;
			return;
		}
		
		if ( !_material.IsValid() || Renderer.MaterialOverride != _material )
		{
			_material = Material.FromShader( "shaders/gamemaker_sprite.shader" );
			Renderer.MaterialOverride = _material;
		}
		
		Renderer.SceneObject.Batchable = false;
		Renderer.SceneObject.Flags.IsTranslucent = true;
		Renderer.SceneObject.Flags.IsOpaque = false;
		Renderer.Attributes.Set( "AtlasTexture", Texture );
		Renderer.Attributes.Set( "AtlasSize", new Vector2( Texture.Width, Texture.Height ) );
		var pageRect = new Vector4( PositionX, PositionY, Width, Height );
		Renderer.Attributes.Set( "TexturePageRect", pageRect );
	}
}
