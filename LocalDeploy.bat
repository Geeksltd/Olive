@echo off
rem ============================================================================
rem  LocalDeploy.bat
rem
rem  Builds (Release) and packs the Olive NuGet packages published by the GitHub
rem  Action (.github/workflows/publish_Olive.yml) into a LOCAL nuget feed.
rem
rem  For every project, BEFORE building it we query both nuget.org AND the local
rem  feed to make sure the version in the .csproj is genuinely new. If that exact
rem  version already exists in either place, the project is skipped. Only new
rem  versions are built and packed.
rem
rem  A full RELEASE BUILD is performed first (dotnet build -c Release), then the
rem  package is produced with 'dotnet pack --no-build' so the compiled assembly
rem  always exists (avoids NU5026 "dll not found on disk").
rem
rem  On failure, the complete dotnet output for that project is printed.
rem
rem  Output feed:  D:\nuget
rem
rem  The real logic lives in the embedded PowerShell below the marker line
rem  (batch looping over a self-modified .bat is unreliable; PowerShell is not).
rem ============================================================================
setlocal
set "DEPLOY_ROOT=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$s=[IO.File]::ReadAllText('%~f0'); $m=[char]35+'::PSCODE::'+[char]35; $i=$s.IndexOf($m); Invoke-Expression $s.Substring($i + $m.Length)"
exit /b %ERRORLEVEL%
#::PSCODE::#

$ErrorActionPreference = 'Continue'
$LocalDir = 'D:\nuget'
$Config   = 'Release'
$RepoRoot = $env:DEPLOY_ROOT

if (-not (Test-Path $LocalDir)) { New-Item -ItemType Directory -Path $LocalDir -Force | Out-Null }

# --- Projects published by publish_Olive.yml (commented-out ones excluded) ---
$projects = @(
    'Olive\Olive.csproj',
    'Olive.Entities.Data.IRepository\Olive.Entities.Data.IRepository.csproj',
    'Olive.ApiClient\Olive.ApiClient.csproj',
    'Olive.Audit\Olive.Audit.csproj',
    'Olive.Azure.DocumentAnalyzer\Olive.Azure.DocumentAnalyzer.csproj',
    'Olive.Azure.DocumentClassification\Olive.Azure.DocumentClassification.csproj',
    'Olive.Audit.DatabaseLogger\Olive.Audit.DatabaseLogger.csproj',
    'Olive.Aws\Olive.Aws.csproj',
    'Olive.Aws.EventBus\Olive.Aws.EventBus.csproj',
    'Olive.Aws.LambdaFunction\Olive.Aws.LambdaFunction.csproj',
    'Olive.Aws.S3Backup.SqlServer\Olive.Aws.S3Backup.SqlServer.csproj',
    'Olive.Aws.Mvc\Olive.Aws.Mvc.csproj',
    'Olive.Aws.Ses\Olive.Aws.Ses.csproj',
    'Olive.Aws.Ses.AutoFetch\Olive.Aws.Ses.AutoFetch.csproj',
    'Olive.Azure\Olive.Azure.csproj',
    'Olive.Azure.EventBus\Olive.Azure.EventBus.csproj',
    'Olive.Blob.Aws\Olive.Blob.Aws.csproj',
    'Olive.Blob.Azure\Olive.Blob.Azure.csproj',
    'Olive.Cloud\Olive.Cloud.csproj',
    'Olive.Compression\Olive.Compression.csproj',
    'Olive.Console\Olive.Console.csproj',
    'Olive.CSV\Olive.CSV.csproj',
    'Olive.Drawing\Olive.Drawing.csproj',
    'Olive.Email\Olive.Email.csproj',
    'Olive.Email.Imap\Olive.Email.Imap.csproj',
    'Olive.Encryption\Olive.Encryption.csproj',
    'Olive.Encryption.NETCore\Olive.Encryption.NETCore.csproj',
    'Olive.Entities\Olive.Entities.csproj',
    'Olive.Entities.Cache.Redis\Olive.Entities.Cache.Redis.csproj',
    'Olive.Entities.Data\Olive.Entities.Data.csproj',
    'Olive.Entities.Data.MySql\Olive.Entities.Data.MySql.csproj',
    'Olive.Entities.Data.PostgreSQL\Olive.Entities.Data.PostgreSQL.csproj',
    'Olive.Entities.Data.DynamoDB\Olive.Entities.Data.DynamoDB.csproj',
    'Olive.Entities.Data.Replication\Olive.Entities.Data.Replication.csproj',
    'Olive.Entities.Data.Replication.Mvc.Extensions\Olive.Entities.Data.Replication.Mvc.Extensions.csproj',
    'Olive.Entities.Data.Replication.QueueUrlProvider\Olive.Entities.Data.Replication.QueueUrlProvider.csproj',
    'Olive.Entities.Data.SQLite\Olive.Entities.Data.SQLite.csproj',
    'Olive.Entities.Data.SqlServer\Olive.Entities.Data.SqlServer.csproj',
    'Olive.EventBus\Olive.EventBus.csproj',
    'Olive.EventBus.Mvc.Extensions\Olive.EventBus.Mvc.Extensions.csproj',
    'Olive.Export\Olive.Export.csproj',
    'Olive.GeoLocation\Olive.GeoLocation.csproj',
    'Olive.Globalization\Olive.Globalization.csproj',
    'Olive.Hangfire\Olive.Hangfire.csproj',
    'Olive.Hangfire.Cron\Olive.Hangfire.Cron.csproj',
    'Olive.Hangfire.MySql\Olive.Hangfire.MySql.csproj',
    'Olive.Log.EventBus\Olive.Log.EventBus.csproj',
    'Olive.Microservices\Olive.Microservices.csproj',
    'Olive.Mvc\Olive.Mvc.csproj',
    'Olive.Mvc.CKEditorFileManager\Olive.Mvc.CKEditorFileManager.csproj',
    'Olive.Mvc.IpFilter\Olive.Mvc.IpFilter.csproj',
    'Olive.Mvc.Microservices\Olive.Mvc.Microservices.csproj',
    'Olive.Mvc.Paging\Olive.Mvc.Paging.csproj',
    'Olive.Mvc.Recaptcha\Olive.Mvc.Recaptcha.csproj',
    'Olive.Mvc.Security\Olive.Mvc.Security.csproj',
    'Olive.Mvc.Testing\Olive.Mvc.Testing.csproj',
    'Olive.PassiveBackgroundTasks\Olive.PassiveBackgroundTasks.csproj',
    'Olive.PDF\Olive.PDF.csproj',
    'Olive.PushNotification\Olive.PushNotification.csproj',
    'Olive.Security.Auth0\Olive.Security.Auth0.csproj',
    'Olive.Security.Aws\Olive.Security.Aws.csproj',
    'Olive.Security.Azure\Olive.Security.Azure.csproj',
    'Olive.Security.Cloud\Olive.Security.Cloud.csproj',
    'Olive.Security.Impersonation\Olive.Security.Impersonation.csproj',
    'Olive.SMS\Olive.SMS.csproj',
    'Olive.Sms.Aws\Olive.Sms.Aws.csproj',
    'Olive.Web\Olive.Web.csproj',
    'Olive.Aws.Rekognition\Olive.Aws.Rekognition.csproj',
    'Olive.Aws.Textract\Olive.Aws.Textract.csproj',
    'Olive.Aws.Comprehend\Olive.Aws.Comprehend.csproj',
    'Olive.Gpt\Olive.Gpt.csproj',
    'Olive.Entities.Data.Replication.DataGenerator.UI\Olive.Entities.Data.Replication.DataGenerator.UI.csproj',
    'Olive.OpenAI\Olive.OpenAI\Olive.OpenAI.csproj',
    'Olive.OpenAI.Voice\Olive.OpenAI.Voice\Olive.OpenAI.Voice.csproj',
    'Olive.MFA\Olive.MFA.csproj'
)

$built = 0; $skipped = 0; $failed = 0
$failedProjects = @()

Write-Host "============================================================"
Write-Host " Local NuGet deploy  ->  $LocalDir"
Write-Host " Configuration: $Config"
Write-Host "============================================================"
Write-Host ""

function Dump-Log($title, $lines) {
    Write-Host "   >> $title"
    Write-Host "------------------------------------------------------------"
    $lines | ForEach-Object { Write-Host $_ }
    Write-Host "------------------------------------------------------------"
}

foreach ($rel in $projects) {
    $full = Join-Path $RepoRoot $rel

    if (-not (Test-Path $full)) {
        Write-Host "[MISSING] $rel"
        $failed++; $failedProjects += $rel; continue
    }

    try { [xml]$xml = Get-Content -Raw $full } catch {
        Write-Host "[ERROR ] $rel  (cannot read csproj)"
        $failed++; $failedProjects += $rel; continue
    }

    $ver = $xml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
    $id  = [IO.Path]::GetFileNameWithoutExtension($full)

    if (-not $ver) {
        Write-Host "[WARN  ] $rel  (no <Version> found - skipped)"
        $skipped++; continue
    }

    # --- local feed check ---
    if (Test-Path (Join-Path $LocalDir "$id.$ver.nupkg")) {
        Write-Host "[SKIP  ] $rel  (v$ver already in local feed)"
        $skipped++; continue
    }

    # --- nuget.org check ---
    $onNuget = $false
    try {
        $resp = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/$($id.ToLowerInvariant())/index.json" -TimeoutSec 30
        if ($resp.versions -contains $ver) { $onNuget = $true }
    } catch { }

    if ($onNuget) {
        Write-Host "[SKIP  ] $rel  (v$ver already on nuget.org)"
        $skipped++; continue
    }

    # --- new version: RELEASE BUILD, then PACK (no-build) ---
    Write-Host "[BUILD ] $rel  (v$ver)"

    $buildLog = & dotnet build $full -c $Config --nologo -v normal 2>&1
    if ($LASTEXITCODE -ne 0) {
        Dump-Log "build FAILED - full dotnet output below:" $buildLog
        $failed++; $failedProjects += $rel; continue
    }

    $packLog = & dotnet pack $full -c $Config --no-build --nologo -v normal -o $LocalDir 2>&1
    if ($LASTEXITCODE -ne 0) {
        Dump-Log "pack FAILED - full dotnet output below:" $packLog
        $failed++; $failedProjects += $rel; continue
    }

    Write-Host "   packed v$ver -> $LocalDir"
    $built++
}

Write-Host ""
Write-Host "============================================================"
Write-Host " Done.   Built: $built   Skipped: $skipped   Failed: $failed"
Write-Host " Feed:   $LocalDir"
if ($failedProjects.Count -gt 0) {
    Write-Host " Failed projects:"
    $failedProjects | ForEach-Object { Write-Host "   - $_" }
}
Write-Host "============================================================"

exit $failed
