# Updated readme:
Updated by [Avril112113](https://github.com/Avril112113)

Updated to work on **both Windows and Linux**, since the old [inidbi2-linux](https://github.com/cmd-johnson/inidbi2-linux) port broke in an Arma 3 update.  
It can load the old linux saves but might not work in certain cases. It will always save new data using the original windows format. (Both are using `.ini` but there was some minor difference which were incompatible)  
Requires .NET 9, which at this time is in preview (It might work with .NET 8?)  
Only x64 binaries are provided for both Windows and Linux, x86 builds aren't tested.  

Building requires to be run on that platform.  
Windows: `dotnet publish -r win-x64`  
Linux: `dotnet publish -r linux-x64`  
Be weary of Linux distro versions, if built on newer Debian version, it will not work on an older one.  
*Both binaries have changed, I haven't gone through the process of getting them approved by battle-eye, this is a non-issue for servers.*  

# Original readme:
	Description:
	INIDBI 2.06 - A simple server-side database extension using INI files

	Author:  code34 nicolas_boiteux@yahoo.fr

	Copyright (C) 2013-2019 Nicolas BOITEUX 

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.

	How to install:
	1- Unpack the archive and copy the entire "@inidbi2" folder into the ARMA3 root directory.

		The @inibdi2 folder should look like this:
		../Arma 3/@inidbi2/inidbi2.dll
		../Arma 3/@inidbi2/db/
		../Arma 3/@inidbi2/Addons/inidbi.pbo

	2- check inidbi2.dll execution permissions, right click on it, and authorize it
	3- load it through -serverMod=@inidbi2; or -mod=@inidbi2;

	Changelog
	- version 2.06
		- add getKeys method
		- fix setseparator method, manage exception in delete method, manage exception in encode64base/decode64base
	- version 2.05
		- re factory gettimestamp method return array instead string containing system UTC TIME
	- version 2.04
		- add getSections method
	- version 2.02 
		- add methods to tune separators
		- fix write returns
		- fix read types
		- fix buffer overflow of decode/encodebase64
		- fix getTimeStamp
	- version 2.0 - rebuild from scratch C# & SQF++
