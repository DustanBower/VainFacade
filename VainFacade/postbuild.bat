copy /y "C:\Users\%USERNAME%\source\repos\VainFacade\VainFacade\bin\Release\VainFacadePlaytest.dll" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest"
copy /y "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\manifest.json" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest"
copy /y "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\preview.jpg" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest"
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Atlas" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Atlas" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Cutouts" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Cutouts" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\DeckBrowser" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\DeckBrowser" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Endings" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Endings" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Environments" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Environments" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Fonts" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Fonts" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\LargeCardTextures" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\LargeCardTextures" /e
robocopy "C:\Users\%USERNAME%\source\repos\VainFacade\Resources\Music" "C:\Program Files (x86)\Steam\steamapps\common\Sentinels of the Multiverse\mods\VainFacadePlaytest\Music" /e
exit 0
