using System.IO;

namespace GameMakerMount;

public class GameMakerEmbeddedAudio : GameMakerArchiveResource
{
	public GameMakerEmbeddedAudio( AudioChunk.Record audioRecord, string name, bool shouldLoop ) : base(audioRecord.AudioData)
	{
		Name = name;
		ShouldLoop = shouldLoop;
		Index = audioRecord.Index;
		AudioFormat = audioRecord.Format;
	}

	public string Name { get; }
	public bool ShouldLoop { get; }
	public int Index { get; }
	public AudioChunk.AudioFormat AudioFormat { get; }
	

	protected override object LoadFromData( MemoryStream ms, BinaryReader br )
	{
		if ( AudioFormat != AudioChunk.AudioFormat.Wav )
		{
			Log.Info( $"Unknown audio format in embedded audio {Index}" );
			return null;
		}
		
		var bytes = new byte[DataLength];
		var bytesRead = br.Read( bytes, 0, DataLength );

		var fileName = System.IO.Path.GetFileNameWithoutExtension( Name );
		return SoundFile.FromWav( fileName, bytes, ShouldLoop );
	}
}
