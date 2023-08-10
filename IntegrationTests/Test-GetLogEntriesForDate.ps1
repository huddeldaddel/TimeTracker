param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries/2023-07-17"
try {
	$response = Invoke-RestMethod $appUrl -Method GET
	Write-Host "GetLogEntriesByDate: Success" -ForegroundColor Green
} catch {
	Write-Host "GetLogEntriesByDate: Failure" -ForegroundColor Red
}