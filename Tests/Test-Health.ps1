param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/health"
$response = Invoke-WebRequest $appUrl -Method GET
if($response.StatusCode -eq 200) {
	Write-Host "Health: Success" -ForegroundColor Green
} else {
	Write-Host "Health: Failure" -ForegroundColor Red
}