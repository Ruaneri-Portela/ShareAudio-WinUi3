$builds = @("Win32", "ARM64", "x64")
foreach ($build in $builds) {
    if ($build -eq "Win32") {
        $msbild = "x86"
    } else {
        $msbild = $build
    }
    & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ShareAudio-WinUi3.sln -p:Configuration=Release -p:Platform="$msbild"
    & "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ShareAudio-WinUi3.sln -p:Configuration=Debug -p:Platform="$msbild"
}