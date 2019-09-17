Alpha Legends
=============

Attemp to create the first playable 0.5.3 emulator. This project started as a fork of [barncastle's Alpha-WoW](https://github.com/barncastle/Alpha-WoW).

Requirements:
  - [Visual Studio 2017/2019](https://visualstudio.microsoft.com/es/) or [MonoDevelop](https://www.monodevelop.com/).
  - It's recommended using [MariaDB](https://mariadb.org/) instead of [MySql](https://www.mysql.com/) for compatibility.
  
Installation:

[Server]
-	Move the dbc folder to the same folder as your compiled executable (Usually _bin/Debug_).
-	Run the sql file under the Database folder, and then apply everything inside the Database/Updates folder.
-	Rename the app.config.dist file inside the WorldServer folder to app.config and edit it to point at your database server.

[Client]
-	Execute the WoWClient.exe file with -uptodate argument. I also recommend executing it with -windowed argument and/or -console argument, for conveniences.
-	The account information is stored in a file called wow.ses, the account will be automatically created when logged in, but it will have a GM level = 0, set it to 1 manually if you want.

[Problems]
- Logitech G Hub and Origin updater could use the same port as server, so it's recommended to close them if you have problems during server initialization with busy sockets.
