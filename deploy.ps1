pushd .\src\Streamnesia.ConsoleApp
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true -o ../deployment --self-contained
popd

pushd .\src\Streamnesia.WebApp
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true -o ../deployment --self-contained
popd

pushd .\src\deployment
Remove-Item *.pdb
popd

Copy-Item -Path .\src\deployment\* -Destination "C:\Program Files (x86)\Steam\steamapps\common\Amnesia The Dark Descent\streamnesia" -Recurse -force
