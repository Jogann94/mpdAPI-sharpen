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
	public class MPDOutput : org.gateshipone.malp.mpdservice.mpdprotocol.mpdobjects.MPDGenericItem
	{
		private string mOutputName;

		private bool mActive;

		private int mOutputId;

		public MPDOutput(string name, bool enabled, int id)
		{
			mOutputName = name;
			mActive = enabled;
			mOutputId = id;
		}

		public virtual string getOutputName()
		{
			return mOutputName;
		}

		public virtual bool getOutputState()
		{
			return mActive;
		}

		public virtual int getID()
		{
			return mOutputId;
		}

		public virtual void setOutputState(bool active)
		{
			mActive = active;
		}

		public virtual string getSectionTitle()
		{
			return mOutputName;
		}
	}
}
