# Requires Microsoft Excel installed
$ErrorActionPreference = 'Stop'


# Set base path interactively or hardcoded
$basePath = Read-Host 'Enter the base path to your Excel files'

$workbooks = @(
    @{ Name = 'Strategy_PaaS_Preferred.xlsx'; Expected = @{
        "Server_to_AzureVM" = @(
            "SERVER_NAME", "MIGRATION_READINESS", "SECURITY_READINESS", "CONFIDENCE_RATING_PERCENT", "OS_SUPPORT_STATUS", "SUPPORT_ENDS_IN_MONTHS", "SUPPORT_END_DATE", "RECOMMENDED_COMPUTE_SKU", "RECOMMENDED_STORAGE_SKU", "STORAGE_UTILIZATION_PERCENT", "TOTAL_MONTHLY_COST_USD", "MONTHLY_COMPUTE_COST_USD", "MONTHLY_STORAGE_COST_USD", "MONTHLY_SECURITY_COST_USD", "OPERATING_SYSTEM_NAME", "OS_VERSION", "OS_ARCHITECTURE", "BOOT_TYPE", "TOTAL_DISKS_COUNT", "ONPREM_STORAGE_GB", "ONPREM_CPU_USAGE_PERCENT", "ONPREM_MEMORY_USAGE_PERCENT", "DISK_READ_IOPS", "DISK_WRITE_IOPS", "NETWORK_READ_MBPS", "NETWORK_WRITE_MBPS", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "ONPREM_CORES_COUNT", "ONPREM_MEMORY_MB", "NETWORK_ADAPTERS_COUNT", "SOURCE_SYSTEM", "IP_ADDRESS", "MAC_ADDRESS", "TOTAL_ISSUES_COUNT", "RESOURCE_TAGS"
        );
        "SQLinstance_to_AzureSQLVM" = @(
            "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_VM_READINESS", "AZURE_SQL_VM_READINESS_ISSUES", "AZURE_SQL_VM_READINESS_WARNINGS", "STRATEGY", "SIZING_CRITERIA", "AZURE_SQL_VM_SKU", "ONPREM_STORAGE_GB", "AZURE_SQL_VM_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_VM_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "STORAGE_TYPE", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_VM_FAMILY", "TARGET_VCORES", "TARGET_STORAGE_SIZE_GB", "TARGET_STORAGE_TYPE", "TARGET_STORAGE_REDUNDANCY", "TARGET_IOPS", "TARGET_THROUGHPUT_MBPS", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "PgSQL_to_AzureFlexServerPG" = @(
            "SERVER", "POSTGRESQL INSTANCE", "USER DATABASE", "AZURE DB FOR POSTGRESQL READINESS", "AZURE DB FOR POSTGRESQL READINESS - ISSUES", "AZURE DB FOR POSTGRESQL READINESS - WARNINGS", "POSTGRESQL SERVER FOR AZURE VM READINESS", "RECOMMENDED TARGET ON AZURE", "AZURE DB FOR POSTGRESQL CONFIGURATION", "COMPUTE MONTHLY COST ESTIMATE (USD)", "STORAGE MONTHLY COST ESTIMATE (USD)", "SECURITY MONTHLY COST ESTIMATE (USD)", "MIGRATION GUIDANCE", "RECOMMENDATION DETAILS - TARGET REASONINGS", "POSTGRESQL EDITION", "POSTGRESQL VERSION", "STORAGE", "TOTAL DB SIZE (MB)", "VCORES ALLOCATED", "SUPPORT STATUS", "OUT OF SUPPORT DATE", "AZURE DB FORPOSTGRESQL CONFIGURATION - SERVICE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - COMPUTE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - INSTANCE (IN VCORES)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE (IN GB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE TIER"

        );
        "SQLinstance_to_AzureSQLMI" = @(
            "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_MI_READINESS", "AZURE_SQL_MI_READINESS_ISSUES", "AZURE_SQL_MI_READINESS_WARNINGS", "SIZING_CRITERIA", "STRATEGY", "AZURE_SQL_MI_CONFIGURATION", "AZURE_SQL_MI_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_MI_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "ONPREM_STORAGE_GB", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_SERVICE_TIER", "TARGET_COMPUTE_TIER", "TARGET_HARDWARE_TYPE", "TARGET_INSTANCE_VCORES", "TARGET_STORAGE_GB", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "WebApp_to_AKS" = @(
            "WebAppName","WebAppReadiness","ReadinessIssues","ClusterName","NodePoolId","RecommendedSKU","WebAppType","GroupName","ServerName"
        );
        "WebApp_to_AKS_Costdetails" = @(
            "ClusterName","NodePoolName","NodeCount","PodCount","RecommendedSKU","MonthlyCostEstimate","OSType"
        );
        "Webapp_to_Appservice" = @(
            "WebAppName", "WebAppType", "WebAppReadiness", "ReadinessIssues", "AppServicePlan", "RecommendedSKU", "GroupName", "ServerName"
        );
        "Webapp_to_Appservice_Costdetail" = @(
            "App_service_plan","RecommendedSKU","MonthlyCostEstimate","WebAppCount"
        );
    }}
    @{ Name = 'Strategy_PaasOnly.xlsx'; Expected = @{
        "PgSQL_to_AzureFlexServerPG" = @(
            "SERVER", "POSTGRESQL INSTANCE", "USER DATABASES", "AZURE DB FOR POSTGRESQL READINESS", "AZURE DB FOR POSTGRESQL READINESS - ISSUES", "AZURE DB FOR POSTGRESQL READINESS - WARNINGS", "SIZING CRITERIA", "AZURE DB FOR POSTGRESQL CONFIGURATION", "COMPUTE MONTHLY COST ESTIMATE (USD)", "STORAGE MONTHLY COST ESTIMATE (USD)", "SECURITY MONTHLY COST ESTIMATE (USD)", "POSTGRESQL EDITION", "POSTGRESQL VERSION", "STORAGE (GB)", "VCORES ALLOCATED", "SUPPORT STATUS", "OUT OF SUPPORT DATE", "RECOMMENDED TARGET", "MIGRATION GUIDANCE", "TOTAL DB SIZE (MB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - SERVICE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - COMPUTE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - INSTANCE (in vCores)", "AZURE DB FOR POSTGRESQL CONFIGURATION -STORAGE (in GB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE TIER"
        );
        "Server_to_AzureVM" = @(
            "SERVER_NAME","MIGRATION_READINESS","SECURITY_READINESS","CONFIDENCE_RATING_PERCENT","OS_SUPPORT_STATUS","SUPPORT_ENDS_IN_MONTHS","SUPPORT_END_DATE","RECOMMENDED_COMPUTE_SKU","RECOMMENDED_STORAGE_SKU","STORAGE_UTILIZATION_PERCENT","TOTAL_MONTHLY_COST_USD","MONTHLY_COMPUTE_COST_USD","MONTHLY_STORAGE_COST_USD","MONTHLY_SECURITY_COST_USD","OPERATING_SYSTEM_NAME","OS_VERSION","OS_ARCHITECTURE","BOOT_TYPE","TOTAL_DISKS_COUNT","ONPREM_STORAGE_GB","ONPREM_CPU_USAGE_PERCENT","ONPREM_MEMORY_USAGE_PERCENT","DISK_READ_IOPS","DISK_WRITE_IOPS","NETWORK_READ_MBPS","NETWORK_WRITE_MBPS","DISK_READ_MBPS","DISK_WRITE_MBPS","ONPREM_CORES_COUNT","ONPREM_MEMORY_MB","NETWORK_ADAPTERS_COUNT","SOURCE_SYSTEM","IP_ADDRESS","MAC_ADDRESS","TOTAL_ISSUES_COUNT","RESOURCE_TAGS"
        );
        "SQLinstance_to_AzureSQLMI" = @(
            "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_MI_READINESS", "AZURE_SQL_MI_READINESS_ISSUES", "AZURE_SQL_MI_READINESS_WARNINGS", "SIZING_CRITERIA", "STRATEGY", "AZURE_SQL_MI_CONFIGURATION", "AZURE_SQL_MI_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_MI_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "ONPREM_STORAGE_GB", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_SERVICE_TIER", "TARGET_COMPUTE_TIER", "TARGET_HARDWARE_TYPE", "TARGET_INSTANCE_VCORES", "TARGET_STORAGE_GB", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "WebApp_to_AKS" = @(
            "WebAppName", "WebAppReadiness", "ReadinessIssues", "ClusterName", "NodePoolId", "RecommendedSKU", "WebAppType", "GroupName", "ServerName"
        );
        "WebApp_to_AKS_Costdetails" = @(
            "ClusterName","NodePoolName","NodeCount","PodCount","RecommendedSKU","MonthlyCostEstimate","OSType"
        );
        "Webapp_to_Appservice" = @(
            "WebAppName","WebAppReadiness","ReadinessIssues","AppServicePlan","RecommendedSKU","EstimatedWebAppCost","GroupName","ServerName"
        );
        "Webapp_to_Appservice_Costdetail" = @(
            "App_service_plan","RecommendedSKU","MonthlyCostEstimate","WebAppCount"
        );
    } }
    @{ Name = 'Issues&Warnings.xlsx'; Expected = @{
        "Issues&Warnings_PgSQL" = @(
            "SERVER","POSTGRESQL INSTANCE","DATABASE","CATEGORY","ISSUE/WARNING LEVEL (SOURCE)",
            "MIGRATION READINESS TARGET","TITLE","IMPACTED OBJECT TYPE","IMPACTED OBJECT NAME"
        );
        "Issues&Warnings_SQL" = @(
            "SERVER","SQL_INSTANCE","DATABASE","CATEGORY","ISSUE_WARNING_LEVEL_SOURCE",
            "MIGRATION_READINESS_TARGET","TITLE","IMPACTED_OBJECT_TYPE","IMPACTED_OBJECT_NAME",
            "PROBABLE_CAUSE","RECOMMENDATIONS"
        );
        "Issues&Warnings_VM" = @(
            "ServerName","MachineOperatingSystem","AzureVMReadiness","Category","AzureReadinessIssues",
            "DataCollectionIssues","ProbableCause","Recommendation"
        );
        "Issues&Warnings_WebApps" = @(
            "WebAppName","ApplicationType","GroupName","AzureTarget","AppServicePlan","RecommendedSKU",
            "MigrationPlatform","WebAppReadiness","SecurityReadiness","IssueCode","IssueCategory",
            "IssueDescription","ProbableCause","RecommendedActions","MoreInformation","TotalMonthlyCost",
            "ConfidenceScore","WebAppCount","SuggestedMigrationTool","ResourceType"
        );
    }}
    @{ Name = 'AzureMigrate_Discovery_Report.xlsx'; Expected = @{
        "ARGData" = @(
            "armId","parentId","resourceName","resourceType","edition","version","dependencyMapping","supportStatus","discoverySource","source","properties.createdTimestamp","properties.updatedTimestamp","properties.operatingSystemDetails.osArchitecture","properties.operatingSystemDetails.osVersion","properties.operatingSystemDetails.osType","properties.operatingSystemDetails.osName","properties.numberOfProcessorCore","properties.allocatedMemoryInMB","properties.totalInstanceCount","properties.biosSerialNumber","properties.isDeleted","properties.displayName","properties.ipAddresses","properties.arcDiscovery.status","properties.arcDiscovery.machineResourceId","properties.runAsAccountId","properties.errors[0].runAsAccountId","properties.errors[0].recommendedAction","properties.errors[0].updatedTimeStamp","properties.errors[0].applianceName","properties.errors[0].possibleCauses","properties.errors[0].summaryMessage","properties.errors[0].discoveryScope","properties.errors[0].severity","properties.errors[0].message","properties.errors[0].source","properties.errors[0].code","properties.errors[0].id","properties.biosGuid","properties.firmware","properties.totalApplicationCount","properties.isGuestDetailsDiscoveryInProgress","properties.vmFqdn","properties.guestDetailsDiscoveryTimestamp","properties.totalFreeSpaceOfAllDisksInGB","properties.autoEnableDependencyMapping","properties.disks[0].logicalSectorSizeInBytes","properties.disks[0].usedSpaceInBytes","properties.disks[0].name","properties.disks[0].maxSizeInBytes","properties.disks[0].diskType","properties.disks[0].path","properties.disks[0].diskProvisioningPolicy","properties.disks[0].diskScrubbingPolicy","properties.disks[0].lun","properties.disks[0].controllerType","properties.disks[0].label","properties.disks[0].isOSDisk","properties.disks[0].diskMode","properties.disks[0].uuid","properties.disks[0].usedSpaceInBytesV2","properties.dependencyMappingStartTime","properties.dependencyMappingEndTime","properties.eTag","properties.dependencyMapDiscovery.discoveryScopeStatus","properties.dependencyMapDiscovery.hydratedRunAsAccountId","properties.dependencyMapDiscovery.errors","properties.productSupportStatus","properties.numberOfSecurityRisks","properties.isFileServerSupported","properties.applicationDiscovery.discoveryScopeStatus","properties.applicationDiscovery.hydratedRunAsAccountId","properties.applicationDiscovery.errors[0].runAsAccountId","properties.applicationDiscovery.errors[0].recommendedAction","properties.applicationDiscovery.errors[0].updatedTimeStamp","properties.applicationDiscovery.errors[0].applianceName","properties.applicationDiscovery.errors[0].possibleCauses","properties.applicationDiscovery.errors[0].summaryMessage","properties.applicationDiscovery.errors[0].discoveryScope","properties.applicationDiscovery.errors[0].severity","properties.applicationDiscovery.errors[0].message","properties.applicationDiscovery.errors[0].source","properties.applicationDiscovery.errors[0].code","properties.applicationDiscovery.errors[0].id","properties.numberOfApplications","properties.vmConfigurationFileLocation","properties.springBootDiscovery.discoveryScopeStatus","properties.springBootDiscovery.totalInstanceCount","properties.springBootDiscovery.shallowDiscoveryStatus","properties.springBootDiscovery.totalApplicationCount","properties.discoveredWorkloads","properties.applianceNames","properties.distinctErrorCount","properties.dependencyMapping","properties.totalDiskSizeInGB","properties.changeTrackingSupported","properties.numberOfSoftware","properties.networkAdapters[0].macAddress","properties.networkAdapters[0].ipAddressList","properties.networkAdapters[0].ipAddressType","properties.networkAdapters[0].networkName","properties.networkAdapters[0].label","properties.networkAdapters[0].adapterType","properties.networkAdapters[0].nicId","properties.oracleDiscovery.discoveryScopeStatus","properties.oracleDiscovery.totalInstanceCount","properties.oracleDiscovery.shallowDiscoveryStatus","properties.oracleDiscovery.totalDatabaseCount","properties.tomcatDiscovery.discoveryScopeStatus","properties.tomcatDiscovery.totalWebApplicationCount","properties.tomcatDiscovery.totalWebServerCount","properties.staticDiscovery.discoveryScopeStatus","properties.staticDiscovery.hydratedRunAsAccountId","properties.staticDiscovery.errors","properties.webAppDiscovery.discoveryScopeStatus","properties.webAppDiscovery.totalWebApplicationCount","properties.webAppDiscovery.totalWebServerCount","properties.changeTrackingEnabled","properties.hostInMaintenanceMode","properties.appsAndRoles","properties.pgSQLDiscovery.discoveryScopeStatus","properties.pgSQLDiscovery.totalInstanceCount","properties.pgSQLDiscovery.shallowDiscoveryStatus","properties.pgSQLDiscovery.totalDatabaseCount","properties.guestOSDetails.osArchitecture","properties.guestOSDetails.osVersion","properties.guestOSDetails.osType","properties.guestOSDetails.osName","properties.mySQLDiscovery.discoveryScopeStatus","properties.mySQLDiscovery.totalInstanceCount","properties.mySQLDiscovery.shallowDiscoveryStatus","properties.mySQLDiscovery.totalDatabaseCount","properties.vMwareToolsVersion","properties.hostProcessorInfo.name","properties.hostProcessorInfo.numberOfCoresPerSocket","properties.hostProcessorInfo.numberOfSockets","properties.secureBootEnabled","properties.vMwareToolsStatus","properties.numberOfSnapshots","properties.iisDiscovery.discoveryScopeStatus","properties.iisDiscovery.totalWebApplicationCount","properties.iisDiscovery.totalWebServerCount","properties.sqlDiscovery.discoveryScopeStatus","properties.sqlDiscovery.successfullyDiscoveredServerCount","properties.sqlDiscovery.sqlMetadataHydratedRunAsAccountId","properties.sqlDiscovery.sqlMetadataDiscoveryPipe","properties.sqlDiscovery.totalServerCount","properties.dataCenterScope","properties.diskEnabledUuid","properties.hostPowerState","properties.hostName","properties.instanceUuid","properties.altGuestName","properties.maxSnapshots","properties.description","properties.powerStatus","properties.hostVersion","properties.vCenterFQDN","properties.vCenterId","dbProperties","cores","memoryInMB","diskCount","totalSizeInGB","osType","supportEndsIn","powerOnStatus","siteId","dbEngineStatus","userdatabases","dbhadrConfiguration","depmapErrorCount","depMapDiscoveryScopeStatus","autoEnableDependencyMapping","ipAddressList","totalDatabaseInstances","totalWebAppCount","webServerId","webServerVersion","webServerType","arcStatus"
        )
    }}
)



function Test-SheetExists {
    param($Workbook, [string]$SheetName)
    $existing = @($Workbook.Sheets | ForEach-Object { $_.Name })
    return $existing -contains $SheetName
}

function Set-CellValueWithRetry {
    param(
        $Cell,
        $Value,
        [int]$MaxRetries = 5,
        [int]$DelayMs = 300
    )
    for ($try = 1; $try -le $MaxRetries; $try++) {
        try {
            $Cell.Value2 = $Value
            return
        } catch {
            if ($try -eq $MaxRetries) { throw }
            Start-Sleep -Milliseconds $DelayMs
        }
    }
}


foreach ($wb in $workbooks) {
    $xlsxPath = Join-Path $basePath $wb.Name
    $expected = $wb.Expected

    Write-Host "Processing $xlsxPath..."

    $excel = $null
    $workbook = $null
    try {
        Get-Process excel -ErrorAction SilentlyContinue | Where-Object { !$_.MainWindowHandle } | Stop-Process -Force

        $excel = New-Object -ComObject Excel.Application
        $excel.Visible = $false
        $excel.DisplayAlerts = $false
        $excel.ScreenUpdating = $false

        $workbook = $excel.Workbooks.Open($xlsxPath)

        foreach ($kvp in $expected.GetEnumerator()) {
            $sheetName = $kvp.Key
            $columns   = $kvp.Value

            if (-not (Test-SheetExists -Workbook $workbook -SheetName $sheetName)) {
                $newSheet = $workbook.Worksheets.Add()
                $newSheet.Name = $sheetName

                for ($i = 0; $i -lt $columns.Count; $i++) {
                    Set-CellValueWithRetry -Cell $newSheet.Cells.Item(1, $i + 1) -Value $columns[$i]
                }

                $headerRange = $newSheet.Range("A1", $newSheet.Cells.Item(1, $columns.Count))
                $headerRange.Font.Bold = $true
                $newSheet.Columns.AutoFit() | Out-Null

                Write-Host "🆕 Added sheet '$sheetName' with $($columns.Count) columns."
            }
            else {
                $sheet = $workbook.Sheets.Item($sheetName)
                # Read existing header row (assume first row)
                $existingHeaders = @()
                $col = 1
                while ($true) {
                    $val = $sheet.Cells.Item(1, $col).Text
                    if ($null -eq $val -or $val -eq "") { break }
                    $existingHeaders += $val
                    $col++
                }
                $missing = $columns | Where-Object { $_ -notin $existingHeaders }
                if ($missing.Count -gt 0) {
                    $startCol = $existingHeaders.Count + 1
                    foreach ($colName in $missing) {
                        Set-CellValueWithRetry -Cell $sheet.Cells.Item(1, $startCol) -Value $colName
                        $startCol++
                    }
                    $headerRange = $sheet.Range("A1", $sheet.Cells.Item(1, $existingHeaders.Count + $missing.Count))
                    $headerRange.Font.Bold = $true
                    $sheet.Columns.AutoFit() | Out-Null
                    Write-Host "✏️ Sheet '$sheetName' exists - added missing columns: $($missing -join ', ')"
                } else {
                    Write-Host "✅ Sheet '$sheetName' already exists and has all columns."
                }
            }
        }

        $workbook.Save()
        Write-Host "💾 Workbook saved: $xlsxPath"
    }
    catch {
        $errMsg = $_.Exception.Message
        Write-Error "❌ ${xlsxPath}: $errMsg"
    }
    finally {
        if ($workbook) { $workbook.Close($true) }
        if ($excel)    { $excel.Quit() }
        [System.GC]::Collect()
        [System.GC]::WaitForPendingFinalizers()
    }
}
