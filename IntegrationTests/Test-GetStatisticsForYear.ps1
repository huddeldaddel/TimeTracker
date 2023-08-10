param(
     [Parameter()]
     [string]$BaseUrl
 )

$appUrl = "${BaseUrl}/api/statistics/2023"
try {
	$result = Invoke-RestMethod $appUrl -Method GET
	Write-Host "GetStatisticsForYear: Success" -ForegroundColor Green
} catch {
	Write-Host "GetStatisticsForYear: Failure" -ForegroundColor Red
}