rmdir /S /Q bin
rmdir /S /Q obj
dotnet restore
npm install -g bower
bower install
npm install
pause