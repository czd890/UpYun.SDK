
param($projectDir,$projectName,$apiKey)
$currDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$nugetdownloadUrl="https://nuget.org/nuget.exe"
$publishdir=$currDir+"\.publish"
$nugetDir=$publishdir+"\.nuget"
$nuget=$nugetDir+"\nuget.exe"
$outputdir=$publishdir+"\"+$projectDir
$projectFull=$currDir+"\"+$projectDir+"\"+$projectName
write-Host "project dir: $projectDir"
write-Host "current dir: $currDir"
write-Host "nuget dir: $nugetDir"
write-Host "nuget file path: $nuget"
write-Host "nuget output dir: $outputdir"
write-Host "nuget apiKey: $apiKey"
function DownloadWithRetry([string] $url, [string] $downloadLocation, [int] $retries) 
{
    while($true)
    {
        try
        {
            Invoke-WebRequest $url -OutFile $downloadLocation
            break
        }
        catch
        {
            $exceptionMessage = $_.Exception.Message
            Write-Host "Failed to download '$url': $exceptionMessage"
            if ($retries -gt 0) {
                $retries--
                Write-Host "Waiting 10 seconds before retrying. Retries left: $retries"
                Start-Sleep -Seconds 10

            }
            else 
            {
                $exception = $_.Exception
                throw $exception
            }
        }
    }
}
if(!(Test-Path $publishdir)){
	New-Item -Path $publishdir -ItemType Directory
}
if (!(Test-Path $nugetDir)) {
    New-Item -Path $nugetDir -ItemType Directory
}

if(!(Test-Path $nuget)){
	write-Host "download nuget.exe"
	DownloadWithRetry -url $nugetdownloadUrl -downloadLocation $nuget -retries 3
}
cd $projectDir

if(!(Test-Path $outputdir)){
	New-Item -Path $outputdir -Type directory | Out-Null
}
$rem=$outputdir+"\*"
Remove-Item $rem -recurse
cd $nugetDir

.\nuget.exe pack $projectFull -OutputDirectory $outputdir

$fileList=Get-ChildItem  -Path $outputdir -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -match "^((?!symbols).)*$" } | %{$_.FullName}
foreach($f in $fileList){
write-Host "publish:::$f" 
.\nuget.exe  push $f -ApiKey $apiKey
}
cd $currDir
write-Host "finsh"

