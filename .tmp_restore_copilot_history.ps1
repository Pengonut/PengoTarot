$ErrorActionPreference = 'Stop'

$oldStorage = 'C:\Users\Pengo\AppData\Roaming\Code\User\workspaceStorage\7292af5e95b9f11380a8f852b37e4de8'
$newStorage = 'C:\Users\Pengo\AppData\Roaming\Code\User\workspaceStorage\c41051645a711074158d8387a6056a39'
$statusFile = 'C:\Users\Pengo\AppData\Roaming\Code\User\workspaceStorage\pengo-copilot-restore-status.txt'
$backupRoot = 'C:\Users\Pengo\AppData\Roaming\Code\User\workspaceStorage\pengo-copilot-backup-20260825-restore'

try {
    Set-Content -LiteralPath $statusFile -Value 'WAITING_FOR_VSCODE_EXIT' -Encoding utf8

    $deadline = (Get-Date).AddMinutes(15)
    do {
        $codeProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
            $_.ProcessName -match '^(Code|code-insiders)$'
        })
        if ($codeProcesses.Count -eq 0) {
            Start-Sleep -Seconds 3
            $codeProcesses = @(Get-Process -ErrorAction SilentlyContinue | Where-Object {
                $_.ProcessName -match '^(Code|code-insiders)$'
            })
            if ($codeProcesses.Count -eq 0) { break }
        }
        Start-Sleep -Seconds 1
    } while ((Get-Date) -lt $deadline)

    if ($codeProcesses.Count -ne 0) {
        throw 'Timed out waiting for VS Code to exit; restore was not run.'
    }

    $resolvedOld = (Resolve-Path -LiteralPath $oldStorage).Path
    $resolvedNew = (Resolve-Path -LiteralPath $newStorage).Path
    if ($resolvedOld -eq $resolvedNew) { throw 'Source and target storage are identical.' }

    Set-Content -LiteralPath $statusFile -Value 'BACKING_UP' -Encoding utf8
    New-Item -ItemType Directory -Path $backupRoot -ErrorAction Stop | Out-Null
    Copy-Item -LiteralPath $resolvedOld -Destination (Join-Path $backupRoot 'old-storage') -Recurse -Force
    Copy-Item -LiteralPath $resolvedNew -Destination (Join-Path $backupRoot 'new-storage') -Recurse -Force

    Set-Content -LiteralPath $statusFile -Value 'RESTORING' -Encoding utf8
    Get-ChildItem -LiteralPath (Join-Path $resolvedOld 'chatSessions') -Force | Copy-Item -Destination (Join-Path $resolvedNew 'chatSessions') -Recurse -Force
    Get-ChildItem -LiteralPath (Join-Path $resolvedOld 'chatEditingSessions') -Force | Copy-Item -Destination (Join-Path $resolvedNew 'chatEditingSessions') -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $resolvedOld 'state.vscdb') -Destination (Join-Path $resolvedNew 'state.vscdb') -Force
    if (Test-Path -LiteralPath (Join-Path $resolvedOld 'state.vscdb.backup')) {
        Copy-Item -LiteralPath (Join-Path $resolvedOld 'state.vscdb.backup') -Destination (Join-Path $resolvedNew 'state.vscdb.backup') -Force
    }

    $chatCount = @(Get-ChildItem -LiteralPath (Join-Path $resolvedNew 'chatSessions') -File).Count
    $editCount = @(Get-ChildItem -LiteralPath (Join-Path $resolvedNew 'chatEditingSessions') -Recurse -File).Count
    Set-Content -LiteralPath $statusFile -Value "COMPLETE`nChatSessions=$chatCount`nEditingFiles=$editCount`nBackup=$backupRoot" -Encoding utf8
}
catch {
    Set-Content -LiteralPath $statusFile -Value "FAILED`n$($_.Exception.Message)" -Encoding utf8
    exit 1
}
