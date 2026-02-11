#!/bin/bash

# Configuration
DB_USER="sa"
DB_PASSWORD="GuchluBirSifre123!"
DB_NAME="ArikanCWIDB"
CONTAINER_NAME="mssql_server"

echo "Checking MSSQL connectivity..."

# Try with new path (mssql-tools18) used in MSSQL 2022 images
SQLCMD_PATH="/opt/mssql-tools18/bin/sqlcmd"

# Fallback check - if we can't find it there, maybe it's the old path (unlikely for :2022-latest but good to be safe if image differs)
# Since we execute inside container, we can't easily check with [ -f ], so let's try invoking it.
if docker exec $CONTAINER_NAME ls $SQLCMD_PATH > /dev/null 2>&1; then
   echo "Using sqlcmd at $SQLCMD_PATH"
else
   echo "New path not found, trying old path..."
   SQLCMD_PATH="/opt/mssql-tools/bin/sqlcmd"
fi

# Try initial connection. If it fails, print the error to help debugging
if ! docker exec $CONTAINER_NAME $SQLCMD_PATH -C -S localhost -U $DB_USER -P $DB_PASSWORD -Q "SELECT 1" > /dev/null 2>&1; then
    echo "Initial connection attempt failed. Here is the error from MSSQL:"
    docker exec $CONTAINER_NAME $SQLCMD_PATH -C -S localhost -U $DB_USER -P $DB_PASSWORD -Q "SELECT 1"
    echo "------------------------------------------------------------"
    echo "Attempting to wait for readiness..."
fi

# Wait for MSSQL
until docker exec $CONTAINER_NAME $SQLCMD_PATH -C -S localhost -U $DB_USER -P $DB_PASSWORD -Q "SELECT 1" > /dev/null 2>&1
do
  echo -n "."
  sleep 2
done
echo
echo "MSSQL is ready!"

# Create Database
echo "Creating database $DB_NAME if it doesn't exist..."
docker exec $CONTAINER_NAME $SQLCMD_PATH -C -S localhost -U $DB_USER -P $DB_PASSWORD -d master -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '$DB_NAME') BEGIN CREATE DATABASE [$DB_NAME]; END"

# Helper function to execute SQL files
run_sql_pipe() {
    local file=$1
    if [ ! -f "$file" ]; then
        echo "Warning: File $file not found!"
        return
    fi
    echo "Executing $file..."
    # Using -C for TrustServerCertificate
    cat "$file" | docker exec -i $CONTAINER_NAME $SQLCMD_PATH -C -S localhost -U $DB_USER -P $DB_PASSWORD -d $DB_NAME
}

# 1. Schema
run_sql_pipe "backend/database/CreateSchema.sql"

# 2. Stored Procedures
echo "Executing Stored Procedures..."
for file in backend/database/StoredProcedures/*.sql; do
    [ -e "$file" ] || continue
    run_sql_pipe "$file"
done

# 3. Sample Data
run_sql_pipe "backend/database/SampleData.sql"

echo "Migration completed successfully!"
