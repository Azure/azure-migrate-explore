[CmdletBinding()]

$buildNumber = $Env:BUILD_BUILDNUMBER


$buildVersion ='v' + $buildNumber
$assemblyVersionPattern = '\[assembly: AssemblyVersion\("(.*)"\)\]'
$assemblyFileVersionPattern = '\[assembly: AssemblyFileVersion\("(.*)"\)\]'
$assemblyInformationalVersionPattern = '\[assembly: AssemblyInformationalVersion\("(.*)"\)\]'

$AssemblyFiles = Get-ChildItem . PipelineAssemblyInfo.cs -rec

foreach ($file in $AssemblyFiles)
{
    (Get-Content $file.PSPath) | ForEach-Object {
        if($_ -match $assemblyVersionPattern){
            '[assembly: AssemblyVersion("{0}")]' -f $buildNumber
        } elseif($_ -match $assemblyFileVersionPattern) {
            '[assembly: AssemblyFileVersion("{0}")]' -f $buildNumber
        } elseif($_ -match $assemblyInformationalVersionPattern) {
            '[assembly: AssemblyInformationalVersion("{0}")]' -f $buildVersion
        } else {
            # output line as-is
            $_
        }
    } | Set-Content $file.PSPath
}
