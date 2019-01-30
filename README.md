# PXE Boot

Small service for Windows, to serivce PXE Boot clients.
This program supports multiple architectures, using serparate folders for different architectures.
Includes a read-only TFTP server too.

*This program is intended to combine with [MiniNT5](https://github.com/VulpesSARL/MiniNT5-Tools), but you can serve any files to boot, even Linux.*

### Prerequisites

* a generic DHCP Server (those from routers is sufficient)
* any C# compiler (I use Visual Studio 2017 Enterprise)

### Compiling

* Open the SLN file, and compile

### Using the program

* Compile the program as Relase, and copy the Executable where you want on your server.
* Open Registry, and go to (create if needed) to this folder: HKLM\Software\Fox\PXEBoot and create a REG_SZ RootPath pointing to the folder where the boot files are located.
* Execute (using administrative permissions)
	* PXEBoot -install
	* PXEBoot -registereventlog
	* PXEBoot -createdirstruct

* Put the boot files (like MiniNT5) to the folders
* Start the service.
* Boot from network and see what happens

### Note

All these tools are provided as-is from [Vulpes](https://vulpes.lu).
If you've some questions, contact me [here](https://go.vulpes.lu/contact).


