:run.bat

echo "run Client1"
cd "./Client1/bin/Debug"
start "" "./Client1.exe"

echo "run Client2"
cd "../../../Client2/bin/Debug"
start "" "./Client2.exe"

echo "run Server1"
cd "../../../Server1/bin/Debug"
start "" "./Server1.exe"

echo "run Server2"
cd "../../../Server2/bin/Debug"
start "" "./Server2.exe"