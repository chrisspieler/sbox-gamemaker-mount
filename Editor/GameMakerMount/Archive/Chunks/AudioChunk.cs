using System.IO;

namespace GameMakerMount;

public record AudioChunk( ArchiveData ChunkData, int[] ElementOffsets )
	: ArchiveListChunk<AudioChunk.Record>( ChunkData, ChunkMagic.Audio, ElementOffsets )
{
	public enum AudioFormat
	{
		Unknown,
		Wav,
		Vorbis,
		Mp3
	}
	
	public record Record(
		int Index,
		ArchiveData RecordData,
		AudioFormat Format,
		int Channels,
		int Frequency,
		ArchiveData AudioData
	) : ChunkRecord( Index, RecordData );
	
	protected override Record ReadRecord( int recordIndex, int recordOffset, FileStream fs, BinaryReader br )
	{
		var audioDataLength = br.ReadInt32();
		var audioStartAddress = (int)fs.Position;

		var channels = 0;
		var frequency = 0;
		var audioFormat = AudioFormat.Unknown;
		
		// This is where the audio data actually starts. We should see magic number here (e.g., "RIFF")
		var magic = System.Text.Encoding.UTF8.GetString( br.ReadBytes( 4 ) );
		switch (magic)
		{
			case "RIFF":
				ReadWavData();
				break;
			case "OggS":
				ReadVorbisData();
				break;
			default:
				throw new Exception( $"{fs.Position:X8} Unexpected magic: {magic}" );
		}
		
		return new Record(
			Index: recordIndex,
			RecordData: new ArchiveData( ChunkData.Archive, recordOffset, 4 ),
			Format: audioFormat,
			Channels: channels,
			Frequency: frequency,
			AudioData: new ArchiveData( ChunkData.Archive, audioStartAddress, audioDataLength )
		);

		void ReadWavData()
		{
			audioFormat = AudioFormat.Wav;
			
			audioDataLength = br.ReadInt32() + 8;
			var fileString = System.Text.Encoding.UTF8.GetString( br.ReadBytes( 4 ) );
			if ( fileString != "WAVE" )
				throw new Exception( $"{fs.Position:X8} Unexpected file format: {fileString}" );

			// Skip FormatBlocID and BlocSize
			fs.Seek( 8, SeekOrigin.Current );

			// We're only handling PCM for now. 
			var wavFormat = br.ReadInt16();
			if ( wavFormat != 1 )
				throw new Exception( $"{fs.Position:X8} Unsupported audio format: {audioFormat}" );
			
			channels = br.ReadInt16();
			frequency = br.ReadInt32();
		}

		void ReadVorbisData()
		{
			audioFormat = AudioFormat.Vorbis;
			
			// Don't do anything with Vorbis yet.
		}
	}
}
