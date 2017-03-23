REM unless msbuild is on the path this should be run from a Visual Studio 2017 command prompt

msbuild ../src/SqlModeller/SqlModeller.csproj /t:restore;build;pack /p:Configuration=Release /p:Platform=AnyCPU /p:IncludeSymbols=true

copy ..\src\SqlModeller\bin\Release\*.nupkg .\nuget\*.*
