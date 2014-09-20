rem use call to execute other bat files
echo npm install 
call "%ProgramFiles%\nodejs\npm" install

rem because we have listed grunt-cli as a dev dependency,
rem the executable will be located in the node_modules folder
echo grunt
call "./node_modules/.bin/grunt"