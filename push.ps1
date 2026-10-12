# Заливка WebGL-билда на itch.io.
# Запуск из корня проекта: .\push.ps1
# Предполагает, что билд уже собран в Builds\Web (Build Profile "Web" в Unity).

$ErrorActionPreference = 'Stop'

# Папка билда и цель на itch: <пользователь>/<игра>:<канал>.
$buildDir = Join-Path $PSScriptRoot 'Builds\Web'
$target   = 'laandscape/ld60:html5'

# Без index.html itch не запустит игру — проверяем до заливки.
if (-not (Test-Path (Join-Path $buildDir 'index.html'))) {
    Write-Error "В $buildDir нет index.html. Сначала собери билд в Unity."
}

# butler шлёт только изменившиеся блоки, поэтому повторная заливка быстрая.
butler push $buildDir $target

# Показать, что на itch появилась новая версия канала.
butler status 'laandscape/ld60'
