param(
    [Parameter()]
    [string]$BaseUrl
)

Get-ChildItem '.\Test-*.ps1' | ForEach-Object {
  & $_.FullName $BaseUrl
}