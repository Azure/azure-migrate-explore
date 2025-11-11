# Requires Microsoft Excel installed
$ErrorActionPreference = 'Stop'


# Set base path interactively or hardcoded
$basePath = Read-Host 'Enter the base path to your Excel files'

$workbooks = @(
    @{ Name = 'Strategy_PaaS_Preferred.xlsx'; Expected = @{
        "Server_to_AzureVM" = @(
            "APPLICATION", "SERVER_NAME", "MIGRATION_READINESS", "SECURITY_READINESS", "CONFIDENCE_RATING_PERCENT", "OS_SUPPORT_STATUS", "SUPPORT_ENDS_IN_MONTHS", "SUPPORT_END_DATE", "RECOMMENDED_COMPUTE_SKU", "RECOMMENDED_STORAGE_SKU", "STORAGE_UTILIZATION_PERCENT", "TOTAL_MONTHLY_COST_USD", "MONTHLY_COMPUTE_COST_USD", "MONTHLY_STORAGE_COST_USD", "MONTHLY_SECURITY_COST_USD", "OPERATING_SYSTEM_NAME", "OS_VERSION", "OS_ARCHITECTURE", "BOOT_TYPE", "TOTAL_DISKS_COUNT", "ONPREM_STORAGE_GB", "ONPREM_CPU_USAGE_PERCENT", "ONPREM_MEMORY_USAGE_PERCENT", "DISK_READ_IOPS", "DISK_WRITE_IOPS", "NETWORK_READ_MBPS", "NETWORK_WRITE_MBPS", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "ONPREM_CORES_COUNT", "ONPREM_MEMORY_MB", "NETWORK_ADAPTERS_COUNT", "SOURCE_SYSTEM", "IP_ADDRESS", "MAC_ADDRESS", "TOTAL_ISSUES_COUNT", "RESOURCE_TAGS", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e"
        );
        "SQLinstance_to_AzureSQLVM" = @(
            "APPLICATION", "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_VM_READINESS", "AZURE_SQL_VM_READINESS_ISSUES", "AZURE_SQL_VM_READINESS_WARNINGS", "STRATEGY", "SIZING_CRITERIA", "AZURE_SQL_VM_SKU", "ONPREM_STORAGE_GB", "AZURE_SQL_VM_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_VM_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "STORAGE_TYPE", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_VM_FAMILY", "TARGET_VCORES", "TARGET_STORAGE_SIZE_GB", "TARGET_STORAGE_TYPE", "TARGET_STORAGE_REDUNDANCY", "TARGET_IOPS", "TARGET_THROUGHPUT_MBPS", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "PgSQL_to_AzureFlexServerPG" = @(
            "APPLICATION", "SERVER", "POSTGRESQL INSTANCE", "USER DATABASE", "AZURE DB FOR POSTGRESQL READINESS", "AZURE DB FOR POSTGRESQL READINESS - ISSUES", "AZURE DB FOR POSTGRESQL READINESS - WARNINGS", "POSTGRESQL SERVER FOR AZURE VM READINESS", "RECOMMENDED TARGET ON AZURE", "AZURE DB FOR POSTGRESQL CONFIGURATION", "COMPUTE MONTHLY COST ESTIMATE (USD)", "STORAGE MONTHLY COST ESTIMATE (USD)", "SECURITY MONTHLY COST ESTIMATE (USD)", "MIGRATION GUIDANCE", "RECOMMENDATION DETAILS - TARGET REASONINGS", "POSTGRESQL EDITION", "POSTGRESQL VERSION", "STORAGE", "TOTAL DB SIZE (MB)", "VCORES ALLOCATED", "SUPPORT STATUS", "OUT OF SUPPORT DATE", "AZURE DB FORPOSTGRESQL CONFIGURATION - SERVICE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - COMPUTE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - INSTANCE (IN VCORES)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE (IN GB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE TIER", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e"
        );
        "SQLinstance_to_AzureSQLMI" = @(
            "APPLICATION", "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_MI_READINESS", "AZURE_SQL_MI_READINESS_ISSUES", "AZURE_SQL_MI_READINESS_WARNINGS", "SIZING_CRITERIA", "STRATEGY", "AZURE_SQL_MI_CONFIGURATION", "AZURE_SQL_MI_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_MI_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "ONPREM_STORAGE_GB", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_SERVICE_TIER", "TARGET_COMPUTE_TIER", "TARGET_HARDWARE_TYPE", "TARGET_INSTANCE_VCORES", "TARGET_STORAGE_GB", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "WebApp_to_AKS" = @(
            "APPLICATION", "SERVERNAME", "WEBAPPNAME", "WEBAPPTYPE", "READINESS", "READINESSISSUES", "NODEPOOLID", "RECOMMENDED_SKU", "CARBON_EMISSIONS_SCOPE1_MTCO2E", "CARBON_EMISSIONS_SCOPE2_MTCO2E", "CARBON_EMISSIONS_SCOPE3_MTCO2E", "TOTAL_CARBON_EMISSIONS_MTCO2E"
        );
        "WebApp_to_AKS_Costdetails" = @(
            "ClusterName","NodePoolName","NodeCount","PodCount","RecommendedSKU","MonthlyCostEstimate","OSType"
        );
        "Webapp_to_Appservice" = @(
            "APPLICATIONNAME", "SERVERNAME", "WEBAPPNAME", "WEBAPPTYPE", "READINESS", "READINESSISSUES", "APPSERVICEPLAN", "RECOMMENDED_SKU", "CARBON_EMISSIONS_SCOPE1_MTCO2E", "CARBON_EMISSIONS_SCOPE2_MTCO2E", "CARBON_EMISSIONS_SCOPE3_MTCO2E", "TOTAL_CARBON_EMISSIONS_MTCO2E"
        );
        "Webapp_to_Appservice_Costdetail" = @(
            "App_service_plan", "RecommendedSKU", "MonthlyCostEstimate", "WebAppCount", "Storage", "Cores", "Ram"
        );
        "Application_Overview" = @(
            "APPLICATION/WORKLOADS",
            "APPLICATION_TYPE",
            "BUSINESS_CRITICALITY",
            "WORKLOADS_CONSIDERED(#)",
            "AZURE_TARGETS(#)",
            "READY",
            "READY_WITH_CONDITIONS",
            "NOT_READY",
            "READINESS_UNKNOWN",
            "MIGRATION_STRATEGY",
            "ESTIMATED_COST",
            "CODE_CHANGES",
            "EFFORT_Hr_CODE_SCAN",
            "SECURITY_SCORE_CODE_SCAN",
            "CLOUD_MATURITY_SCORE_CODE_SCAN",
            "GREEN_IMPACT_CODE_SCAN",
	        "MIGRATION_READINESS"
        );
        "Code_Changes_Workloads" = @(
            "SERVER_NAME",
            "WORKLOAD_NAME",
            "WORKLOAD_TYPE",
            "ISSUE_NAME",
            "TARGET",
            "MIGRATION_STRATEGY",
            "CODE_SCAN_TOOL",
            "SEVERITY",
            "IMPACT",
            "IMPACTED_OBJECTS",
            "OCCURRENCES",
            "ESTIMATED_EFFORT",
            "RECOMMENDED_ACTION"
        );
        "Code_Changes_Applications" = @(
            "APPLICATION",
            "MIGRATION_TYPE",
            "ISSUE_NAME",
            "CODE_SCAN_TOOL",
            "SEVERITY",
            "IMPACT",
            "IMPACTED_OBJECTS",
            "OCCURRENCES",
            "ESTIMATED_EFFORT_HR",
            "RECOMMENDED_ACTION"
        );
    }}
    @{ Name = 'Strategy_PaasOnly.xlsx'; Expected = @{
        "PgSQL_to_AzureFlexServerPG" = @(
            "APPLICATION", "SERVER", "POSTGRESQL INSTANCE", "USER DATABASES", "AZURE DB FOR POSTGRESQL READINESS", "AZURE DB FOR POSTGRESQL READINESS - ISSUES", "AZURE DB FOR POSTGRESQL READINESS - WARNINGS", "SIZING CRITERIA", "AZURE DB FOR POSTGRESQL CONFIGURATION", "COMPUTE MONTHLY COST ESTIMATE (USD)", "STORAGE MONTHLY COST ESTIMATE (USD)", "SECURITY MONTHLY COST ESTIMATE (USD)", "POSTGRESQL EDITION", "POSTGRESQL VERSION", "STORAGE (GB)", "VCORES ALLOCATED", "SUPPORT STATUS", "OUT OF SUPPORT DATE", "RECOMMENDED TARGET", "MIGRATION GUIDANCE", "TOTAL DB SIZE (MB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - SERVICE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - COMPUTE TIER", "AZURE DB FOR POSTGRESQL CONFIGURATION - INSTANCE (in vCores)", "AZURE DB FOR POSTGRESQL CONFIGURATION -STORAGE (in GB)", "AZURE DB FOR POSTGRESQL CONFIGURATION - STORAGE TIER", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e"
        );
        "Server_to_AzureVM" = @(
            "APPLICATION", "SERVER_NAME", "MIGRATION_READINESS", "SECURITY_READINESS", "CONFIDENCE_RATING_PERCENT", "OS_SUPPORT_STATUS", "SUPPORT_ENDS_IN_MONTHS", "SUPPORT_END_DATE", "RECOMMENDED_COMPUTE_SKU", "RECOMMENDED_STORAGE_SKU", "STORAGE_UTILIZATION_PERCENT", "TOTAL_MONTHLY_COST_USD", "MONTHLY_COMPUTE_COST_USD", "MONTHLY_STORAGE_COST_USD", "MONTHLY_SECURITY_COST_USD", "OPERATING_SYSTEM_NAME", "OS_VERSION", "OS_ARCHITECTURE", "BOOT_TYPE", "TOTAL_DISKS_COUNT", "ONPREM_STORAGE_GB", "ONPREM_CPU_USAGE_PERCENT", "ONPREM_MEMORY_USAGE_PERCENT", "DISK_READ_IOPS", "DISK_WRITE_IOPS", "NETWORK_READ_MBPS", "NETWORK_WRITE_MBPS", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "ONPREM_CORES_COUNT", "ONPREM_MEMORY_MB", "NETWORK_ADAPTERS_COUNT", "SOURCE_SYSTEM", "IP_ADDRESS", "MAC_ADDRESS", "TOTAL_ISSUES_COUNT", "RESOURCE_TAGS", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e"
        );
        "SQLinstance_to_AzureSQLMI" = @(
            "APPLICATION", "SERVER", "SQL_INSTANCE", "FCI_PARTICIPANT", "USER_DATABASES", "AG_PARTICIPANT", "AZURE_SQL_MI_READINESS", "AZURE_SQL_MI_READINESS_ISSUES", "AZURE_SQL_MI_READINESS_WARNINGS", "SIZING_CRITERIA", "STRATEGY", "AZURE_SQL_MI_CONFIGURATION", "AZURE_SQL_MI_COMPUTE_MONTHLY_COST_USD", "AZURE_SQL_MI_STORAGE_MONTHLY_COST_USD", "SQL_EDITION", "SQL_VERSION", "ONPREM_STORAGE_GB", "SYNC_DATABASES", "ASYNC_DATABASES", "TOTAL_DB_SIZE_MB", "LARGEST_DB_SIZE_MB", "VCORES_ALLOCATED", "CPU_UTILIZATION_PERCENT", "MEMORY_IN_USE_MB", "NUMBER_OF_DISKS", "DISK_READ_OPS_SEC", "DISK_WRITE_OPS_SEC", "DISK_READ_MBPS", "DISK_WRITE_MBPS", "CONFIDENCE_RATING_PERCENT", "TARGET_SERVICE_TIER", "TARGET_COMPUTE_TIER", "TARGET_HARDWARE_TYPE", "TARGET_INSTANCE_VCORES", "TARGET_STORAGE_GB", "MIGRATION_GUIDANCE", "SKU_REASONINGS", "READINESS_REASONINGS", "SECURITY_READINESS", "SECURITY_MONTHLY_COST_USD", "CARBON_EMISSIONS_SCOPE1_MtCO2e", "CARBON_EMISSIONS_SCOPE2_MtCO2e", "CARBON_EMISSIONS_SCOPE3_MtCO2e", "TOTAL_CARBON_EMISSIONS_MtCO2e", "SUPPORT_STATUS", "SUPPORT_END_DATE", "SUPPORT_ENDS_IN_MONTHS", "VERSION_NUMBER", "FCI", "AVAILABILITY_GROUP"
        );
        "WebApp_to_AKS" = @(
            "APPLICATION", "SERVERNAME", "WEBAPPNAME", "WEBAPPTYPE", "READINESS", "READINESSISSUES", "NODEPOOLID", "RECOMMENDED_SKU", "CARBON_EMISSIONS_SCOPE1_MTCO2E", "CARBON_EMISSIONS_SCOPE2_MTCO2E", "CARBON_EMISSIONS_SCOPE3_MTCO2E", "TOTAL_CARBON_EMISSIONS_MTCO2E"
        );
        "WebApp_to_AKS_Costdetails" = @(
            "ClusterName","NodePoolName","NodeCount","PodCount","RecommendedSKU","MonthlyCostEstimate","OSType"
        );
        "Webapp_to_Appservice" = @(
            "APPLICATIONNAME", "SERVERNAME", "WEBAPPNAME", "WEBAPPTYPE", "READINESS", "READINESSISSUES", "APPSERVICEPLAN", "RECOMMENDED_SKU", "CARBON_EMISSIONS_SCOPE1_MTCO2E", "CARBON_EMISSIONS_SCOPE2_MTCO2E", "CARBON_EMISSIONS_SCOPE3_MTCO2E", "TOTAL_CARBON_EMISSIONS_MTCO2E"
        );
        "Webapp_to_Appservice_Costdetail" = @(
            "App_service_plan","RecommendedSKU","MonthlyCostEstimate","WebAppCount", "Storage", "Cores", "Ram"
        );
        "Application_Overview" = @(
            "APPLICATION/WORKLOADS",
            "APPLICATION_TYPE",
            "BUSINESS_CRITICALITY",
            "WORKLOADS_CONSIDERED(#)",
            "AZURE_TARGETS(#)",
            "READY",
            "READY_WITH_CONDITIONS",
            "NOT_READY",
            "READINESS_UNKNOWN",
            "MIGRATION_STRATEGY",
            "ESTIMATED_COST",
            "CODE_CHANGES",
            "EFFORT_Hr_CODE_SCAN",
            "SECURITY_SCORE_CODE_SCAN",
            "CLOUD_MATURITY_SCORE_CODE_SCAN",
            "GREEN_IMPACT_CODE_SCAN",
	        "MIGRATION_READINESS"
        );
        "Code_Changes_Workloads" = @(
            "SERVER_NAME",
            "WORKLOAD_NAME",
            "WORKLOAD_TYPE",
            "ISSUE_NAME",
            "TARGET",
            "MIGRATION_STRATEGY",
            "CODE_SCAN_TOOL",
            "SEVERITY",
            "IMPACT",
            "IMPACTED_OBJECTS",
            "OCCURRENCES",
            "ESTIMATED_EFFORT",
            "RECOMMENDED_ACTION"
        );
        "Code_Changes_Applications" = @(
            "APPLICATION",
            "MIGRATION_TYPE",
            "ISSUE_NAME",
            "CODE_SCAN_TOOL",
            "SEVERITY",
            "IMPACT",
            "IMPACTED_OBJECTS",
            "OCCURRENCES",
            "ESTIMATED_EFFORT_HR",
            "RECOMMENDED_ACTION"
        );
    } }
    @{ Name = 'Issues&Warnings.xlsx'; Expected = @{
        "Issues&Warnings_PgSQL" = @(
            "APPLICATION",
            "SERVER",
            "POSTGRESQL INSTANCE",
            "DATABASE",
            "CATEGORY",
            "ISSUE/WARNING LEVEL (SOURCE)",
            "MIGRATION READINESS TARGET",
            "TITLE",
            "IMPACTED OBJECT TYPE",
            "IMPACTED OBJECT NAME"
        );
        "Issues&Warnings_SQL" = @(
            "APPLICATION",
            "SERVER",
            "SQL_INSTANCE",
            "DATABASE_COUNT",
            "SQL_INSTANCE_READINESS",
            "CATEGORY",
            "ISSUE_WARNING_LEVEL_SOURCE",
            "MIGRATION_READINESS_TARGET",
            "TITLE",
            "IMPACTED_OBJECT_TYPE",
            "IMPACTED_OBJECT_NAME",
            "PROBABLE_CAUSE",
            "RECOMMENDATIONS"
        );
        "Issues&Warnings_VM" = @(
            "APPLICATION",
            "SERVER_NAME",
            "MACHINE_OPERATING_SYSTEM",
            "AZURE_VM_READINESS",
            "CATEGORY",
            "AZURE_READINESS_ISSUES",
            "DATA_COLLECTION_ISSUES",
            "PROBABLE_CAUSE",
            "RECOMMENDATION"
        );
        "Issues&Warnings_WebApps" = @(
            "APPLICATION",
            "SERVER_NAME",
            "WEB_APP_NAME",
            "WEB_APP_READINESS",
            "CATEGORY",
            "ISSUE_WARNING_LEVEL_SOURCE",
            "MIGRATION_READINESS_TARGET",
            "TITLE",
            "PROBABLE_CAUSE",
            "RECOMMENDATION"
        );
    }}
    @{ Name = 'AzureMigrate_Discovery_Report.xlsx'; Expected = @{
        "ARGData" = @(
            "armId", "resourceType", "supportStatus", "supportEndsIn", "osType", "properties.guestOSDetails.osType", "arcStatus", "hasApplications", "applicationIdSet", "countOfApplications", "applicationNameSet"
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
