param(
    [string]$Path = ".\config\live-state.json"
)

$state = @{
    health   = 86
    shield   = 50
    ammo     = "18 / 144"
    loadout  = "AR | Shotgun | SMG | Minis | Medkit"
    teammates= "Ezra, Ally, Bot42"
    match    = "Round 4 | 5 Elims | #8"
    killFeed = "Bot42 eliminated Player77"
}

$state | ConvertTo-Json -Depth 4 | Set-Content -Path $Path -Encoding UTF8
Write-Host "Updated simulated live state at $Path"
