SET BasePath=D:\Projects\JsonRpc.Standard

pushd .

cd %BasePath%
call git add -A
call git clean -fxd
call dotnet clean
call dotnet restore
call dotnet build

popd
