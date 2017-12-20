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
	/// <summary>This class represents an MPDTrack.</summary>
	/// <remarks>
	/// This class represents an MPDTrack. This is the same type for tracks and files.
	/// This is used for tracks in playlist, album, search results,... and for music files when
	/// retrieving an directory listing from the mpd server.
	/// </remarks>
	public class MPDTrack : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
		, org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem, android.os.Parcelable
	{
		/// <summary>Title of the song</summary>
		private string pTrackTitle;

		/// <summary>Artist of the song</summary>
		private string pTrackArtist;

		/// <summary>Associated album of the song</summary>
		private string pTrackAlbum;

		/// <summary>The artist of the album of this song.</summary>
		/// <remarks>The artist of the album of this song. E.g. Various Artists for compilations
		/// 	</remarks>
		private string pTrackAlbumArtist;

		/// <summary>The date of the song</summary>
		private string pDate;

		/// <summary>MusicBrainz ID for the artist</summary>
		private string pTrackArtistMBID;

		/// <summary>MusicBrainz ID for the song itself</summary>
		private string pTrackMBID;

		/// <summary>MusicBrainz ID for the album of the song</summary>
		private string pTrackAlbumMBID;

		/// <summary>MusicBrainz ID for the album artist</summary>
		private string pTrackAlbumArtistMBID;

		/// <summary>Length in seconds</summary>
		private int pLength;

		/// <summary>Track number within the album of the song</summary>
		private int pTrackNumber;

		/// <summary>Count of songs on the album of the song.</summary>
		/// <remarks>Count of songs on the album of the song. Can be 0</remarks>
		private int pAlbumTrackCount;

		/// <summary>The number of the medium(of the songs album) the song is on</summary>
		private int pDiscNumber;

		/// <summary>The count of mediums of the album the track is on.</summary>
		/// <remarks>The count of mediums of the album the track is on. Can be 0.</remarks>
		private int pAlbumDiscCount;

		/// <summary>Available for tracks in the current playlist</summary>
		private int pSongPosition;

		/// <summary>Available for tracks in the current playlist</summary>
		private int pSongID;

		/// <summary>
		/// Used for
		/// <see cref="org.gateshipone.malp.application.adapters.CurrentPlaylistAdapter"/>
		/// to save if an
		/// image is already being fetchted from the internet for this item
		/// </summary>
		private bool pImageFetching;

		/// <summary>Create empty MPDTrack (track).</summary>
		/// <remarks>
		/// Create empty MPDTrack (track). Fill it with setter methods during
		/// parsing of mpds output.
		/// </remarks>
		/// <param name="path">The path of the file. This should never change.</param>
		public MPDTrack(string path)
			: base(path)
		{
			pTrackTitle = string.Empty;
			pTrackArtist = string.Empty;
			pTrackAlbum = string.Empty;
			pTrackAlbumArtist = string.Empty;
			pDate = string.Empty;
			pTrackArtistMBID = string.Empty;
			pTrackMBID = string.Empty;
			pTrackAlbumMBID = string.Empty;
			pTrackAlbumArtistMBID = string.Empty;
			pLength = 0;
			pImageFetching = false;
		}

		/// <summary>Create a MPDTrack from a parcel</summary>
		/// <param name="in">Parcel to deserialize</param>
		protected internal MPDTrack(android.os.Parcel @in)
			: base(@in.readString())
		{
			pTrackTitle = @in.readString();
			pTrackAlbum = @in.readString();
			pTrackArtist = @in.readString();
			pTrackAlbumArtist = @in.readString();
			pDate = @in.readString();
			pTrackMBID = @in.readString();
			pTrackAlbumMBID = @in.readString();
			pTrackArtistMBID = @in.readString();
			pTrackAlbumArtistMBID = @in.readString();
			pLength = @in.readInt();
			pTrackNumber = @in.readInt();
			pAlbumTrackCount = @in.readInt();
			pDiscNumber = @in.readInt();
			pAlbumDiscCount = @in.readInt();
			pSongPosition = @in.readInt();
			pSongID = @in.readInt();
			pImageFetching = @in.readInt() == 1;
		}

		public virtual string getTrackTitle()
		{
			return pTrackTitle;
		}

		public virtual void setTrackTitle(string pTrackTitle)
		{
			this.pTrackTitle = pTrackTitle;
		}

		public virtual string getTrackArtist()
		{
			return pTrackArtist;
		}

		public virtual void setTrackArtist(string pTrackArtist)
		{
			this.pTrackArtist = pTrackArtist;
		}

		public virtual string getTrackAlbum()
		{
			return pTrackAlbum;
		}

		public virtual void setTrackAlbum(string pTrackAlbum)
		{
			this.pTrackAlbum = pTrackAlbum;
		}

		public virtual string getTrackAlbumArtist()
		{
			return pTrackAlbumArtist;
		}

		public virtual void setTrackAlbumArtist(string pTrackAlbumArtist)
		{
			this.pTrackAlbumArtist = pTrackAlbumArtist;
		}

		public virtual string getDate()
		{
			return pDate;
		}

		public virtual void setDate(string pDate)
		{
			this.pDate = pDate;
		}

		public virtual string getTrackArtistMBID()
		{
			return pTrackArtistMBID;
		}

		public virtual void setTrackArtistMBID(string pTrackArtistMBID)
		{
			this.pTrackArtistMBID = pTrackArtistMBID;
		}

		public virtual string getTrackAlbumArtistMBID()
		{
			return pTrackAlbumArtistMBID;
		}

		public virtual void setTrackAlbumArtistMBID(string pTrackArtistMBID)
		{
			this.pTrackAlbumArtistMBID = pTrackArtistMBID;
		}

		public virtual string getTrackMBID()
		{
			return pTrackMBID;
		}

		public virtual void setTrackMBID(string pTrackMBID)
		{
			this.pTrackMBID = pTrackMBID;
		}

		public virtual string getTrackAlbumMBID()
		{
			return pTrackAlbumMBID;
		}

		public virtual void setTrackAlbumMBID(string pTrackAlbumMBID)
		{
			this.pTrackAlbumMBID = pTrackAlbumMBID;
		}

		public virtual int getLength()
		{
			return pLength;
		}

		public virtual void setLength(int pLength)
		{
			this.pLength = pLength;
		}

		public virtual void setTrackNumber(int trackNumber)
		{
			pTrackNumber = trackNumber;
		}

		public virtual int getTrackNumber()
		{
			return pTrackNumber;
		}

		public virtual void setDiscNumber(int discNumber)
		{
			pDiscNumber = discNumber;
		}

		public virtual int getDiscNumber()
		{
			return pDiscNumber;
		}

		public virtual int getAlbumTrackCount()
		{
			return pAlbumTrackCount;
		}

		public virtual void setAlbumTrackCount(int albumTrackCount)
		{
			pAlbumTrackCount = albumTrackCount;
		}

		public virtual int getAlbumDiscCount()
		{
			return pAlbumDiscCount;
		}

		public virtual void psetAlbumDiscCount(int discCount)
		{
			pAlbumDiscCount = discCount;
		}

		public virtual int getSongPosition()
		{
			return pSongPosition;
		}

		public virtual void setSongPosition(int position)
		{
			pSongPosition = position;
		}

		public virtual int getSongID()
		{
			return pSongID;
		}

		public virtual void setSongID(int id)
		{
			pSongID = id;
		}

		public virtual bool getFetching()
		{
			return pImageFetching;
		}

		public virtual void setFetching(bool fetching)
		{
			pImageFetching = fetching;
		}

		/// <returns>String that is used for section based scrolling</returns>
		public override string getSectionTitle()
		{
			return pTrackTitle.Equals(string.Empty) ? mPath : pTrackTitle;
		}

		/// <summary>Describes if it is a special parcel type (no)</summary>
		/// <returns>0</returns>
		public virtual int describeContents()
		{
			return 0;
		}

		private sealed class _Creator_342 : android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
			>
		{
			public _Creator_342()
			{
			}

			/// <summary>Create a new MPDTrack with parcel creator.</summary>
			/// <param name="in">Parcel to use for creating the MPDTrack object</param>
			/// <returns>The deserialized MPDTrack object</returns>
			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack createFromParcel
				(android.os.Parcel @in)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack(@in);
			}

			/// <summary>Used to create an array of MPDTrack objects</summary>
			/// <param name="size">Size of the array to create</param>
			/// <returns>The created array</returns>
			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack[] newArray
				(int size)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack[size];
			}
		}

		/// <summary>Static creator class to create MPDTrack objects from parcels.</summary>
		public static readonly android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
			> CREATOR = new _Creator_342();

		/// <summary>Serialized the MPDTrack object to a parcel.</summary>
		/// <remarks>
		/// Serialized the MPDTrack object to a parcel. Check that this method is equivalent with the
		/// deserializing creator above.
		/// </remarks>
		/// <param name="dest">Parcel to write the properties to</param>
		/// <param name="flags">Special flags</param>
		public virtual void writeToParcel(android.os.Parcel dest, int flags)
		{
			// Serialize MPDTrack properties
			dest.writeString(mPath);
			dest.writeString(pTrackTitle);
			dest.writeString(pTrackAlbum);
			dest.writeString(pTrackArtist);
			dest.writeString(pTrackAlbumArtist);
			dest.writeString(pDate);
			dest.writeString(pTrackMBID);
			dest.writeString(pTrackAlbumMBID);
			dest.writeString(pTrackArtistMBID);
			dest.writeString(pTrackAlbumArtistMBID);
			dest.writeInt(pLength);
			dest.writeInt(pTrackNumber);
			dest.writeInt(pAlbumTrackCount);
			dest.writeInt(pDiscNumber);
			dest.writeInt(pAlbumDiscCount);
			dest.writeInt(pSongPosition);
			dest.writeInt(pSongID);
			dest.writeInt(pImageFetching ? 1 : 0);
		}

		public virtual int indexCompare(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
			 compFile)
		{
			if (!pTrackAlbumMBID.Equals(compFile.pTrackAlbumMBID))
			{
				return string.CompareOrdinal(pTrackAlbumMBID, compFile.pTrackAlbumMBID);
			}
			// Compare disc numbers first
			if (pDiscNumber > compFile.pDiscNumber)
			{
				return 1;
			}
			else
			{
				if (pDiscNumber == compFile.pDiscNumber)
				{
					// Compare track number field
					if (pTrackNumber > compFile.pTrackNumber)
					{
						return 1;
					}
					else
					{
						if (pTrackNumber == compFile.pTrackNumber)
						{
							return 0;
						}
						else
						{
							return -1;
						}
					}
				}
				else
				{
					return -1;
				}
			}
		}

		public virtual int compareTo(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDTrack
			 another)
		{
			if (another == null)
			{
				return -1;
			}
			string title = mPath;
			string[] pathSplit = title.split("/");
			if (pathSplit.Length > 0)
			{
				title = pathSplit[pathSplit.Length - 1];
			}
			string titleAnother = mPath;
			string[] pathSplitAnother = title.split("/");
			if (pathSplit.Length > 0)
			{
				titleAnother = pathSplit[pathSplit.Length - 1];
			}
			return string.CompareOrdinal(title.ToLower(), titleAnother.ToLower());
		}
	}
}
