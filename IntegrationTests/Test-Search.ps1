param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/search"
$body = '{ "Year": 2022, "Project": "Sonstiges", "Query": "Arbeitstag*" }'

$response = Invoke-WebRequest $appUrl -Method POST -Body $body
if($response.StatusCode -eq 200) {
	Write-Host "AddLogEntry: Success" -ForegroundColor Green
    Write-Host $response
} else {
	Write-Host "AddLogEntry: Failure" -ForegroundColor Red
}