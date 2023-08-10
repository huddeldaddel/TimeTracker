param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries"
$body = '{ "Date": "2023-07-18", "Start": "9:", "End": "11:45", "Project": "Lernen", "Description": "Python" }'

$response = Invoke-WebRequest $appUrl -Method POST -Body $body
if($response.StatusCode -eq 200) {
	Write-Host "AddLogEntry: Success" -ForegroundColor Green
} else {
	Write-Host "AddLogEntry: Failure" -ForegroundColor Red
}