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
	public class MPDDirectory : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDFileEntry
		, org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem
	{
		public MPDDirectory(string path)
			: base(path)
		{
		}

		public override string getSectionTitle()
		{
			string title = mPath;
			string[] pathSplit = title.split("/");
			if (pathSplit.Length > 0)
			{
				title = pathSplit[pathSplit.Length - 1];
			}
			return title;
		}

		public virtual int compareTo(org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDDirectory
			 another)
		{
			if (another == null)
			{
				return -1;
			}
			return string.CompareOrdinal(getSectionTitle().ToLower(), another.getSectionTitle
				().ToLower());
		}
	}
}
