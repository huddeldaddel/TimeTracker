param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries"
$body = '{ "Date": "2023-07-17", "Start": "9:", "End": "11:45", "Project": "learning", "Description": "Learned Azure Functions" }'

$response = Invoke-WebRequest $appUrl -Method POST -Body $body
if($response.StatusCode -eq 200) {
	Write-Host "AddLogEntry - Short Time: Success" -ForegroundColor Green
} else {
	Write-Host "AddLogEntry - Short Time: Failure" -ForegroundColor Red
}