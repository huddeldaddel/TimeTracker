$appUrl = "http://localhost:30575/api/health"
$response = Invoke-WebRequest $appUrl -Method GET
if($response.StatusCode -eq 200) {
	Write-Host "Health: Success" -ForegroundColor Green
} else {
	Write-Host "Health: Failure" - ForegroundColor Red
}