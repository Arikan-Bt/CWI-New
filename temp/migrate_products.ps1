
$srcConnStr = "Server=localhost;Database=ArikanCWIDB;User ID=sa;Password=GuchluBirSifre123!;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;"
$dstConnStr = "Server=10.137.50.200;Database=ArikanCWIDB;User ID=sa;Password=Tre5w+@;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;"

function Migrate-Table($tableName) {
    Write-Host "Migrating table: $tableName..."
    
    $srcConn = New-Object System.Data.SqlClient.SqlConnection($srcConnStr)
    try {
        $srcConn.Open()
        $cmd = $srcConn.CreateCommand()
        $cmd.CommandText = "SELECT * FROM [$tableName]"
        $reader = $cmd.ExecuteReader()
        
        $dt = New-Object System.Data.DataTable
        $dt.Load($reader)
        Write-Host "Read $($dt.Rows.Count) rows from source."
    } finally {
        $srcConn.Close()
    }

    if ($dt.Rows.Count -eq 0) {
        Write-Host "No rows to migrate for $tableName."
        return
    }

    $dstConn = New-Object System.Data.SqlClient.SqlConnection($dstConnStr)
    try {
        $dstConn.Open()
        
        # Check if table exists and has identity
        $checkCmd = $dstConn.CreateCommand()
        $checkCmd.CommandText = "SELECT OBJECTPROPERTY(OBJECT_ID('$tableName'), 'TableHasIdentity')"
        $hasIdentity = $checkCmd.ExecuteScalar() -eq 1
        
        $bulkCopy = New-Object System.Data.SqlClient.SqlBulkCopy($dstConn, [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity, $null)
        $bulkCopy.DestinationTableName = $tableName
        $bulkCopy.BatchSize = 1000
        $bulkCopy.BulkCopyTimeout = 600
        
        try {
            $bulkCopy.WriteToServer($dt)
            Write-Host "Successfully migrated $tableName."
        } catch {
            Write-Error "Failed to migrate ${tableName}. Error: $($_.Exception.Message)"
            if ($_.Exception.Message -match "foreign key constraint") {
                Write-Host "Hint: You may need to migrate lookup tables (Brands, Colors, etc.) first."
            }
        }
    } finally {
        $dstConn.Close()
    }
}

# Migrate Products
Migrate-Table "Products"
