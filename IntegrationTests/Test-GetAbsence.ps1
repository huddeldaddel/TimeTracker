param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/absences/2023-10-16/2023-10-22"
try {
	$result = Invoke-WebRequest $appUrl -Method GET
	Write-Host "GetStatisticsForYear: Success" -ForegroundColor Green
	Write-Host $result
} catch {
	Write-Host "GetStatisticsForYear: Failure" -ForegroundColor Red
}