param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries/3a82f2aa-e060-4b18-be04-38f960032f8a"
try {
	$response = Invoke-RestMethod $appUrl -Method DELETE
	Write-Host "DeleteLogEntry: Success" -ForegroundColor Green
} catch {
	Write-Host "DeleteLogEntry: Failure" -ForegroundColor Red
}