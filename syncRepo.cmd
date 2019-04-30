SET CURRENT_LOCAL=D:\Projects\JsonRpc.Standard
SET ORIGIN_REPO=https://github.com/CXuesong/JsonRpc.Standard
SET ORIGIN_REPO_NAME=CXuesong/JsonRpc.Standard
SET CURRENT_REPO=https://github.com/geleems/JsonRpc.Standard
SET CURRENT_REPO_NAME=geleems/JsonRpc.Standard

pushd .

cd %CURRENT_LOCAL%
call git checkout master
call git checkout ./

call git remote add %ORIGIN_REPO_NAME% %ORIGIN_REPO%
call git remote add %CURRENT_REPO_NAME% %CURRENT_REPO%
call git remote -v
call git fetch %ORIGIN_REPO_NAME%
call git fetch %CURRENT_REPO_NAME%
call git rebase %ORIGIN_REPO_NAME%/master
call git push -f

popd
