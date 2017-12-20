/*
*  Copyright (C) 2017 Team Gateship-One
*  (Hendrik Borghorst & Frederik Luetkes)
*
*  The AUTHORS.md file contains a detailed contributors list:
*  <https://github.com/gateship-one/malp/blob/master/AUTHORS.md>
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/
using Sharpen;

namespace org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects
{
	public class MPDCurrentStatus : android.os.Parcelable
	{
		public enum MPD_PLAYBACK_STATE
		{
			MPD_PLAYING,
			MPD_PAUSING,
			MPD_STOPPED
		}

		/// <summary>Volume: 0 - 100;</summary>
		private int pVolume;

		/// <summary>Repeat: 0,1</summary>
		private int pRepeat;

		/// <summary>Random playback: 0,1</summary>
		private int pRandom;

		/// <summary>Single playback: 0,1</summary>
		private int pSinglePlayback;

		/// <summary>Consume after playback: 0,1</summary>
		private int pConsume;

		/// <summary>Version of the playlist.</summary>
		/// <remarks>Version of the playlist. If changed the user needs a new update.</remarks>
		private int pPlaylistVersion;

		/// <summary>Number of songs in the current playlist</summary>
		private int pPlaylistLength;

		/// <summary>Index of the currently playing song</summary>
		private int pCurrentSongIndex;

		/// <summary>Index of the next song to play (could be index+1 or random)</summary>
		private int pNextSongIndex;

		/// <summary>Samplerate of the audio file (extracted out of: "audio"-field)</summary>
		private int pSamplerate;

		/// <summary>Sample resolution in bits.</summary>
		/// <remarks>Sample resolution in bits. (also audio-field)</remarks>
		private string pBitDepth;

		/// <summary>Channel count of audiofile (also audio-field)</summary>
		private int pChannelCount;

		/// <summary>Bitrate of the codec used</summary>
		private int pBitrate;

		/// <summary>Position of the player in current song</summary>
		private int pElapsedTime;

		/// <summary>Length of the currently playing song.</summary>
		private int pTrackLength;

		/// <summary>If an updating job of the database is running, the id gets saved here.</summary>
		/// <remarks>
		/// If an updating job of the database is running, the id gets saved here.
		/// Also the update commands sends back the id of the corresponding update job.
		/// </remarks>
		private int pUpdateDBJob;

		/// <summary>State of the MPD server (playing, pause, stop)</summary>
		private org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
			 pPlaybackState;

		protected internal MPDCurrentStatus(android.os.Parcel @in)
		{
			/* Create this object from parcel */
			pVolume = @in.readInt();
			pRepeat = @in.readInt();
			pRandom = @in.readInt();
			pSinglePlayback = @in.readInt();
			pConsume = @in.readInt();
			pPlaylistVersion = @in.readInt();
			pPlaylistLength = @in.readInt();
			pCurrentSongIndex = @in.readInt();
			pNextSongIndex = @in.readInt();
			pSamplerate = @in.readInt();
			pBitDepth = @in.readString();
			pChannelCount = @in.readInt();
			pBitrate = @in.readInt();
			pElapsedTime = @in.readInt();
			pTrackLength = @in.readInt();
			pUpdateDBJob = @in.readInt();
			pPlaybackState = org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
				.values()[@in.readInt()];
		}

		public MPDCurrentStatus()
		{
			pVolume = 0;
			pRepeat = 0;
			pRandom = 0;
			pSinglePlayback = 0;
			pConsume = 0;
			pPlaylistVersion = 0;
			pPlaylistLength = 0;
			pCurrentSongIndex = -1;
			pNextSongIndex = 0;
			pSamplerate = 0;
			pBitDepth = "0";
			pChannelCount = 0;
			pBitrate = 0;
			pElapsedTime = 0;
			pTrackLength = 0;
			pUpdateDBJob = -1;
			pPlaybackState = org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
				.MPD_STOPPED;
		}

		/// <summary>Copy constructor.</summary>
		/// <param name="status">Object to copy values from</param>
		public MPDCurrentStatus(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
			 status)
		{
			pVolume = status.pVolume;
			pRepeat = status.pRepeat;
			pRandom = status.pRandom;
			pSinglePlayback = status.pSinglePlayback;
			pConsume = status.pConsume;
			pPlaylistVersion = status.pPlaylistVersion;
			pPlaylistLength = status.pPlaylistLength;
			pCurrentSongIndex = status.pCurrentSongIndex;
			pNextSongIndex = status.pNextSongIndex;
			pSamplerate = status.pSamplerate;
			pBitDepth = status.pBitDepth;
			pChannelCount = status.pChannelCount;
			pBitrate = status.pBitrate;
			pElapsedTime = status.pElapsedTime;
			pTrackLength = status.pTrackLength;
			pUpdateDBJob = status.pUpdateDBJob;
			pPlaybackState = status.pPlaybackState;
		}

		public virtual int getVolume()
		{
			return pVolume;
		}

		public virtual void setVolume(int pVolume)
		{
			if (pVolume >= 0 && pVolume <= 100)
			{
				this.pVolume = pVolume;
			}
			else
			{
				this.pVolume = 0;
			}
		}

		public virtual int getRepeat()
		{
			return pRepeat;
		}

		public virtual void setRepeat(int pRepeat)
		{
			this.pRepeat = pRepeat;
		}

		public virtual int getRandom()
		{
			return pRandom;
		}

		public virtual void setRandom(int pRandom)
		{
			this.pRandom = pRandom;
		}

		public virtual int getSinglePlayback()
		{
			return pSinglePlayback;
		}

		public virtual void setSinglePlayback(int pSinglePlayback)
		{
			this.pSinglePlayback = pSinglePlayback;
		}

		public virtual int getConsume()
		{
			return pConsume;
		}

		public virtual void setConsume(int pConsume)
		{
			this.pConsume = pConsume;
		}

		public virtual int getPlaylistVersion()
		{
			return pPlaylistVersion;
		}

		public virtual void setPlaylistVersion(int pPlaylistVersion)
		{
			this.pPlaylistVersion = pPlaylistVersion;
		}

		public virtual int getPlaylistLength()
		{
			return pPlaylistLength;
		}

		public virtual void setPlaylistLength(int pPlaylistLength)
		{
			this.pPlaylistLength = pPlaylistLength;
		}

		public virtual int getCurrentSongIndex()
		{
			return pCurrentSongIndex;
		}

		public virtual void setCurrentSongIndex(int pCurrentSongIndex)
		{
			this.pCurrentSongIndex = pCurrentSongIndex;
		}

		public virtual int getNextSongIndex()
		{
			return pNextSongIndex;
		}

		public virtual void setNextSongIndex(int pNextSongIndex)
		{
			this.pNextSongIndex = pNextSongIndex;
		}

		public virtual int getSamplerate()
		{
			return pSamplerate;
		}

		public virtual void setSamplerate(int pSamplerate)
		{
			this.pSamplerate = pSamplerate;
		}

		public virtual string getBitDepth()
		{
			return pBitDepth;
		}

		public virtual void setBitDepth(string pBitDepth)
		{
			this.pBitDepth = pBitDepth;
		}

		public virtual int getChannelCount()
		{
			return pChannelCount;
		}

		public virtual void setChannelCount(int pChannelCount)
		{
			this.pChannelCount = pChannelCount;
		}

		public virtual int getBitrate()
		{
			return pBitrate;
		}

		public virtual void setBitrate(int pBitrate)
		{
			this.pBitrate = pBitrate;
		}

		public virtual int getElapsedTime()
		{
			return pElapsedTime;
		}

		public virtual void setElapsedTime(int pElapsedTime)
		{
			this.pElapsedTime = pElapsedTime;
		}

		public virtual int getTrackLength()
		{
			return pTrackLength;
		}

		public virtual void setTrackLength(int pTrackLength)
		{
			this.pTrackLength = pTrackLength;
		}

		public virtual int getUpdateDBJob()
		{
			return pUpdateDBJob;
		}

		public virtual void setUpdateDBJob(int pUpdateDBJob)
		{
			this.pUpdateDBJob = pUpdateDBJob;
		}

		public virtual org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
			 getPlaybackState()
		{
			return pPlaybackState;
		}

		public virtual void setPlaybackState(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus.MPD_PLAYBACK_STATE
			 pPlaybackState)
		{
			this.pPlaybackState = pPlaybackState;
		}

		public override void writeToParcel(android.os.Parcel dest, int flags)
		{
			/* Serialize the class attributes here */
			dest.writeInt(pVolume);
			dest.writeInt(pRepeat);
			dest.writeInt(pRandom);
			dest.writeInt(pSinglePlayback);
			dest.writeInt(pConsume);
			dest.writeInt(pPlaylistVersion);
			dest.writeInt(pPlaylistLength);
			dest.writeInt(pCurrentSongIndex);
			dest.writeInt(pNextSongIndex);
			dest.writeInt(pSamplerate);
			dest.writeString(pBitDepth);
			dest.writeInt(pChannelCount);
			dest.writeInt(pBitrate);
			dest.writeInt(pElapsedTime);
			dest.writeInt(pTrackLength);
			dest.writeInt(pUpdateDBJob);
			/* Convert enum-type to int here and back when deserializing */
			dest.writeInt((int)(pPlaybackState));
		}

		public override int describeContents()
		{
			return 0;
		}

		private sealed class _Creator_359 : android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
			>
		{
			public _Creator_359()
			{
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus createFromParcel
				(android.os.Parcel @in)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
					(@in);
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus[] 
				newArray(int size)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
					[size];
			}
		}

		public static readonly android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDCurrentStatus
			> CREATOR = new _Creator_359();

		public virtual string printStatus()
		{
			/* String output for debug purposes */
			string retString = string.Empty;
			retString += "Volume: " + Sharpen.Runtime.getStringValueOf(pVolume) + "\n";
			retString += "Repeat: " + Sharpen.Runtime.getStringValueOf(pRepeat) + "\n";
			retString += "Random: " + Sharpen.Runtime.getStringValueOf(pRandom) + "\n";
			retString += "Single: " + Sharpen.Runtime.getStringValueOf(pSinglePlayback) + "\n";
			retString += "Consume: " + Sharpen.Runtime.getStringValueOf(pConsume) + "\n";
			retString += "Playlist version: " + Sharpen.Runtime.getStringValueOf(pPlaylistVersion
				) + "\n";
			retString += "Playlist length: " + Sharpen.Runtime.getStringValueOf(pPlaylistLength
				) + "\n";
			retString += "Current song index: " + Sharpen.Runtime.getStringValueOf(pCurrentSongIndex
				) + "\n";
			retString += "Next song index: " + Sharpen.Runtime.getStringValueOf(pNextSongIndex
				) + "\n";
			retString += "Samplerate: " + Sharpen.Runtime.getStringValueOf(pSamplerate) + "\n";
			retString += "Bitdepth: " + pBitDepth + "\n";
			retString += "Channel count: " + Sharpen.Runtime.getStringValueOf(pChannelCount) 
				+ "\n";
			retString += "Bitrate: " + Sharpen.Runtime.getStringValueOf(pBitrate) + "\n";
			retString += "Elapsed time: " + Sharpen.Runtime.getStringValueOf(pElapsedTime) + 
				"\n";
			retString += "Track length: " + Sharpen.Runtime.getStringValueOf(pTrackLength) + 
				"\n";
			retString += "UpdateDB job id: " + Sharpen.Runtime.getStringValueOf(pUpdateDBJob)
				 + "\n";
			retString += "Playback state: " + Sharpen.Runtime.getStringValueOf((int)(pPlaybackState
				)) + "\n";
			return retString;
		}
	}
}
