dotnet restore ../src/SqlModeller/SqlModeller.csproj

dotnet build ../src/SqlModeller/SqlModeller.csproj -c Release /p:Platform=AnyCPU /p:OutDir=nuget

dotnet pack ../src/SqlModeller/SqlModeller.csproj --no-build --include-symbols -c Release --output ../../lib/nuget /p:OutDir=nuget
