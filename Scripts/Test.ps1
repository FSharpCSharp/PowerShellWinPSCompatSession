Import-Module BcContainerHelper -ArgumentList $true -DisableNameChecking -UseWindowsPowerShell

$artifactUrl = Get-BCArtifactUrl -Type OnPrem -Select Latest -country gb
if (-not $artifactUrl) {
    Write-Error "Error getting artifact Url"
}
$artifactPaths = Download-Artifacts -artifactUrl $artifactUrl -includePlatform
$artifactPaths