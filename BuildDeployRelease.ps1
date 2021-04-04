dotnet build --configuration=RELEASE

& "C:\Program Files (x86)\WinSCP\WinSCP.com" `
  /log="C:\Users\franc\source\repos\StrategyTester\WinSCP.log" /ini=nul `
  /command `
    "open scp://pi@192.168.1.133/ -hostkey=`"`"ssh-ed25519 255 oNxrUOLl4sw/x2dGE0lSmnHTSjf5/EYCToJc0MHpi84=`"`" -privatekey=`"`"C:\Users\franc\.ssh\id_rsa.ppk`"`" -passphrase=`"`"outb10biz`"`" -timeout=5" `
    "rm TradingBot/*" `
    "put C:\Users\franc\source\repos\StrategyTester\StrategyTester\bin\Release\net5.0\* TradingBot/" `
    "exit"

$winscpResult = $LastExitCode
if ($winscpResult -eq 0)
{
  ssh pi@192.168.1.133
  Write-Host "Success"
}
else
{
  Write-Host "Error"
}

exit $winscpResult
