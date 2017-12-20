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
	public class MPDArtist : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem
		, java.lang.Comparable<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
		>, android.os.Parcelable
	{
		private string pArtistName;

		private System.Collections.Generic.List<string> pMBIDs;

		private bool mImageFetching;

		public MPDArtist(string name)
		{
			/* Artist properties */
			/* Musicbrainz ID */
			pArtistName = name;
			pMBIDs = new System.Collections.Generic.List<string>();
		}

		protected internal MPDArtist(android.os.Parcel @in)
		{
			pArtistName = @in.readString();
			pMBIDs = @in.createStringArrayList();
			mImageFetching = @in.readByte() != 0;
		}

		private sealed class _Creator_52 : android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			>
		{
			public _Creator_52()
			{
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist createFromParcel
				(android.os.Parcel @in)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist(@in);
			}

			public org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist[] newArray
				(int size)
			{
				return new org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist[size]
					;
			}
		}

		public static readonly android.os.Parcelable.Creator<org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			> CREATOR = new _Creator_52();

		public virtual string getArtistName()
		{
			return pArtistName;
		}

		public virtual int getMBIDCount()
		{
			return pMBIDs.Count;
		}

		public virtual string getMBID(int position)
		{
			return pMBIDs[position];
		}

		public virtual void addMBID(string mbid)
		{
			pMBIDs.add(mbid);
		}

		public virtual string getSectionTitle()
		{
			return pArtistName;
		}

		public override bool Equals(object @object)
		{
			if (!(@object is org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
				))
			{
				return false;
			}
			org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist artist = (org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
				)@object;
			if (!artist.pArtistName.Equals(pArtistName) || artist.pMBIDs.Count != pMBIDs.Count)
			{
				return false;
			}
			for (int i = 0; i < pMBIDs.Count; i++)
			{
				if (!pMBIDs[i].Equals(artist.pMBIDs[i]))
				{
					return false;
				}
			}
			return true;
		}

		public virtual int compareTo(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDArtist
			 another)
		{
			if (another.Equals(this))
			{
				return 0;
			}
			if (another.pArtistName.ToLower().Equals(pArtistName.ToLower()))
			{
				//Log.v(MPDArtist.class.getSimpleName(),"another mbids: " + another.pMBIDs.size() + "self mbids:" + pMBIDs.size());
				// Try to position artists with one mbid at the end
				// Use MBID as sort criteria, without MBID before the ones with
				if ((another.pMBIDs.Count > pMBIDs.Count) || another.pMBIDs.Count == 1)
				{
					return -1;
				}
				else
				{
					if ((another.pMBIDs.Count < pMBIDs.Count) || pMBIDs.Count == 1)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
			}
			return string.CompareOrdinal(pArtistName.ToLower(), another.pArtistName.ToLower()
				);
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
			dest.writeString(pArtistName);
			string[] mbids = Sharpen.Collections.ToArray(pMBIDs, new string[pMBIDs.Count]);
			dest.writeStringArray(mbids);
			dest.writeByte(mImageFetching ? unchecked((byte)1) : unchecked((byte)0));
		}
	}
}
