dotnet run --project tools\PatchPackages --no-build  -- src\Atma.%1\source\Atma.%1.csproj
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
dotnet pack src\Atma.%1 --no-build -c %configuration% /property:Version=%GitVersion_NuGetVersion% -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg -o publish\Atma.%1
@IF %ERRORLEVEL% NEQ 0 GOTO :ERROR
ren publish\Atma.%1\*.snupkg *.symbols.nupkg
nuget add publish\Atma.%1\Atma.%1.%GitVersion_NuGetVersion%.nupkg -Source %APPVEYOR_BUILD_FOLDER%\packages
:ERROR
