param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries"
$body = '{ "Date": "20230717", "Start": "9:", "End": "11:45", "Project": "learning", "Description": "Learned Azure Functions" }'

try {
	Invoke-WebRequest $appUrl -Method POST -Body $body
	Write-Host "AddLogEntry - Invalid Date: Failure" -ForegroundColor Red
} catch {
	Write-Host "AddLogEntry - Invalid Date: Success" -ForegroundColor Green
}