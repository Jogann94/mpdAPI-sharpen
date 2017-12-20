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
	public class MPDAlbum : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem
		, java.lang.Comparable<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
		>, android.os.Parcelable
	{
		private string mName;

		private string mMBID;

		private string mArtistName;

		private System.DateTime mDate;

		private bool mImageFetching;

		public MPDAlbum(string name)
		{
			/* Album properties */
			/* Musicbrainz ID */
			/* Artists name (if any) */
			mName = name;
			mMBID = string.Empty;
			mArtistName = string.Empty;
			mDate = new System.DateTime();
		}

		protected internal MPDAlbum(android.os.Parcel @in)
		{
			/* Getters */
			mName = @in.readString();
			mMBID = @in.readString();
			mArtistName = @in.readString();
			mImageFetching = @in.readByte() != 0;
			mDate = (System.DateTime)@in.readSerializable();
		}

		private sealed class _Creator_63 : android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			>
		{
			public _Creator_63()
			{
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum createFromParcel
				(android.os.Parcel @in)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum(@in);
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum[] newArray
				(int size)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum[size];
			}
		}

		public static readonly android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			> CREATOR = new _Creator_63();

		public virtual string getName()
		{
			return mName;
		}

		public virtual string getMBID()
		{
			return mMBID;
		}

		public virtual string getArtistName()
		{
			return mArtistName;
		}

		public virtual void setArtistName(string artistName)
		{
			if (artistName != null)
			{
				mArtistName = artistName;
			}
		}

		public virtual void setMBID(string mbid)
		{
			if (null != mbid)
			{
				mMBID = mbid;
			}
		}

		public virtual void setDate(System.DateTime date)
		{
			if (null != date)
			{
				mDate = date;
			}
		}

		public virtual System.DateTime getDate()
		{
			return mDate;
		}

		public virtual string getSectionTitle()
		{
			return mName;
		}

		public override bool Equals(object @object)
		{
			if (!(@object is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum))
			{
				return false;
			}
			org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum album = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
				)@object;
			if ((mName.Equals(album.mName)) && (mArtistName.Equals(album.mArtistName)) && (mMBID
				.Equals(album.mMBID)) && (mDate.Equals(album.mDate)))
			{
				return true;
			}
			return false;
		}

		public virtual int compareTo(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			 another)
		{
			if (another.Equals(this))
			{
				return 0;
			}
			return string.CompareOrdinal(mName.ToLower(), another.mName.ToLower());
		}

		public override int GetHashCode()
		{
			return (mName + mArtistName + mMBID).GetHashCode();
		}

		public virtual void setFetching(bool fetching)
		{
			lock (this)
			{
				mImageFetching = fetching;
			}
		}

		public virtual bool getFetching()
		{
			lock (this)
			{
				return mImageFetching;
			}
		}

		public virtual int describeContents()
		{
			return 0;
		}

		public virtual void writeToParcel(android.os.Parcel dest, int flags)
		{
			dest.writeString(mName);
			dest.writeString(mMBID);
			dest.writeString(mArtistName);
			dest.writeByte(unchecked((byte)(mImageFetching ? 1 : 0)));
			dest.writeSerializable(mDate);
		}

		public class MPDAlbumDateComparator : java.util.Comparator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
			>
		{
			public virtual int compare(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum
				 o1, org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDAlbum o2)
			{
				if (o2.Equals(o1))
				{
					return 0;
				}
				return o1.mDate.compareTo(o2.mDate);
			}

			public override bool Equals(object obj)
			{
				return obj.Equals(this);
			}
		}
	}
}
