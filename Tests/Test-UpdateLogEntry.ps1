param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/logEntries"
$body = '{ "Id": "dc52df71-bbac-4b4d-a35f-2781b92c4b73", "Date": "2023-07-17", "Start": "11:45", "End": "12:00", "Project": "Bathroom", "Description": "Washed hands" }'

try {
	$response = Invoke-RestMethod $appUrl -Method PUT -Body $body
	Write-Host "UpdateLogEntry: Success" -ForegroundColor Green
} catch {
	Write-Host "UpdateLogEntry: Failure" -ForegroundColor Red
}