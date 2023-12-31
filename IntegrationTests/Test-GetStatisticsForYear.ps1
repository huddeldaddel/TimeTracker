param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/statistics/2023"
try {
	$result = Invoke-WebRequest $appUrl -Method GET
	Write-Host "GetStatisticsForYear: Success" -ForegroundColor Green
	Write-Host $result
} catch {
	Write-Host "GetStatisticsForYear: Failure" -ForegroundColor Red
}