# How to deploy a new nuget package
First update the `SVSModel.csproj` `<version>` field, then run these commands with the version number replacing the `X.X.X`

```bash
$ dotnet pack SVSModel/SVSModel.csproj
```

```bash
$ dotnet nuget push SVSModel/bin/Release/SVSModel.1.2.2.nupkg --source "https://pkgs.dev.azure.com/rezaresystems/48ae16c6-5f20-44a0-ad41-e047c311de0a/_packaging/svs-model-calculator/nuget/v3/index.json" --api-key az --interactive
```