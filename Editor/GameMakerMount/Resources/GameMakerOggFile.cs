using System.IO;
using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sandbox.Mounting;
using Sandbox.Diagnostics;

namespace GameMakerMount;

public class GameMakerOggFile( string filePath ) : ResourceLoader<GameMakerMount>
{
	public string FilePath { get; private set; } = filePath;

	protected override object Load()
	{
		Assert.True( System.IO.Path.GetExtension( FilePath ) == ".ogg", $"Music must have .ogg file extension: {FilePath}" );
		
		Log.Info( $"Reading .ogg: {FilePath}" );
		
		using var vorbisStream = new VorbisWaveReader( FilePath );
		
		var sampleProvider = new SampleToWaveProvider16( vorbisStream );
		var format = sampleProvider.WaveFormat;
		var channels = format.Channels;
		var sampleRate = (uint)format.SampleRate;
		var bitsPerSample = format.BitsPerSample;
		var totalBytes = vorbisStream.Length;
		
		Log.Info( $"Expecting to read {totalBytes} bytes, {bitsPerSample} bits per sample, {channels} channels, {sampleRate} sample rate" );
		
		var byteData = new byte[vorbisStream.Length];
		var memoryStream = new MemoryStream( byteData );

		WaveFileWriter.WriteWavFileToStream( memoryStream, sampleProvider );
		
		var fileName = System.IO.Path.GetFileNameWithoutExtension( FilePath );
		return SoundFile.FromWav( fileName, byteData, true );
	}
}
