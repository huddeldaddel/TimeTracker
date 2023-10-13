param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/absences"
$body = '{ "Date": "2023-10-16", "HomeOffice": false, "PublicHoliday": false, "SickLeave": false, "Vacation": 2 }'

try {
	$response = Invoke-WebRequest $appUrl -Method PUT -Body $body	
	Write-Host "UpdateAbsence: Success" -ForegroundColor Green
	Write-Host $response
} catch {
	Write-Host "UpdateAbsence: Failure" -ForegroundColor Red
}