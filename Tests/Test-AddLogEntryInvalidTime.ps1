param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries"
$body = '{ "Date": "2023-07-17", "Start": "9", "End": "11:45", "Project": "learning", "Description": "Learned Azure Functions" }'

try {
	Invoke-WebRequest $appUrl -Method POST -Body $body
	Write-Host "AddLogEntry - Invalid Time: Failure" -ForegroundColor Red
} catch {
	Write-Host "AddLogEntry - Invalid Time: Success" -ForegroundColor Green
}
