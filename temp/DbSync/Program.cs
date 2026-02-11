using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;

namespace DbSync
{
    class Program
    {
        static string sourceConnectionString = "Server=localhost;Database=ArikanCWIDB;User ID=sa;Password=GuchluBirSifre123!;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;";
        static string targetConnectionString = "Server=10.137.50.200;Database=ArikanCWIDB;User ID=sa;Password=Tre5w+@;Encrypt=False;TrustServerCertificate=True;Integrated Security=False;";

        static void Main(string[] args)
        {
            Console.WriteLine("Veritabanı senkronizasyonu başlıyor...");

            var sourceSchema = GetSchema(sourceConnectionString);
            var targetSchema = GetSchema(targetConnectionString);

            Console.WriteLine($"Kaynak veritabanında {sourceSchema.Count} tablo bulundu.");
            Console.WriteLine($"Hedef veritabanında {targetSchema.Count} tablo bulundu.");

            try
            {
                SyncDatabases(sourceSchema, targetSchema);
                Console.WriteLine("Senkronizasyon tamamlandı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }

        static List<TableSchema> GetSchema(string connectionString)
        {
            var tables = new List<TableSchema>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // 1. Tabloları al
                var tableQuery = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_SCHEMA = 'dbo'";
                using (var command = new SqlCommand(tableQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tables.Add(new TableSchema
                        {
                            Schema = reader["TABLE_SCHEMA"].ToString(),
                            Name = reader["TABLE_NAME"].ToString(),
                            Columns = new List<ColumnSchema>()
                        });
                    }
                }

                // 2. Kolonları al (Her tablo için tek tek yerine toplu alıp dağıtmak daha performanslı olabilir ama şimdilik döngü basitlik için)
                // Daha iyi performans için tüm kolonları çekip bellekta eşleştirelim
                var columnQuery = @"
                    SELECT 
                        TABLE_SCHEMA, 
                        TABLE_NAME, 
                        COLUMN_NAME, 
                        DATA_TYPE, 
                        CHARACTER_MAXIMUM_LENGTH, 
                        IS_NULLABLE, 
                        NUMERIC_PRECISION, 
                        NUMERIC_SCALE 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = 'dbo'";

                var allColumns = new List<dynamic>();
                using (var command = new SqlCommand(columnQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allColumns.Add(new
                        {
                            Schema = reader["TABLE_SCHEMA"].ToString(),
                            TableName = reader["TABLE_NAME"].ToString(),
                            ColumnName = reader["COLUMN_NAME"].ToString(),
                            DataType = reader["DATA_TYPE"].ToString(),
                            MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? (int?)reader["CHARACTER_MAXIMUM_LENGTH"] : null,
                            IsNullable = reader["IS_NULLABLE"].ToString(),
                            Precision = reader["NUMERIC_PRECISION"] != DBNull.Value ? (byte?)reader["NUMERIC_PRECISION"] : null,
                            Scale = reader["NUMERIC_SCALE"] != DBNull.Value ? (int?)reader["NUMERIC_SCALE"] : null
                        });
                    }
                }

                foreach (var table in tables)
                {
                    var tableColumns = allColumns
                        .Where(c => c.Schema == table.Schema && c.TableName == table.Name)
                        .Select(c => new ColumnSchema
                        {
                            Name = c.ColumnName,
                            DataType = c.DataType,
                            MaxLength = c.MaxLength,
                            IsNullable = c.IsNullable == "YES",
                            Precision = c.Precision,
                            Scale = c.Scale
                        }).ToList();

                    table.Columns = tableColumns;
                }
            }

            return tables;
        }

        static void SyncDatabases(List<TableSchema> source, List<TableSchema> target)
        {
            using (var connection = new SqlConnection(targetConnectionString))
            {
                connection.Open();

                foreach (var sourceTable in source)
                {
                    var targetTable = target.FirstOrDefault(t => t.Name == sourceTable.Name && t.Schema == sourceTable.Schema);

                    if (targetTable == null)
                    {
                        // Tablo yok, oluştur
                        Console.WriteLine($"Tablo oluşturuluyor: {sourceTable.Schema}.{sourceTable.Name}");
                        var createSql = GenerateCreateTableScript(sourceTable);
                        ExecuteSql(connection, createSql);
                    }
                    else
                    {
                        // Tablo var, kolon kontrolü
                        foreach (var sourceCol in sourceTable.Columns)
                        {
                            var targetCol = targetTable.Columns.FirstOrDefault(c => c.Name == sourceCol.Name);
                            if (targetCol == null)
                            {
                                // Kolon yok, ekle
                                Console.WriteLine($"Kolon ekleniyor: {sourceTable.Schema}.{sourceTable.Name}.{sourceCol.Name}");
                                var addColSql = GenerateAddColumnScript(sourceTable, sourceCol);
                                ExecuteSql(connection, addColSql);
                            }
                            else
                            {
                                // Kolon var, tip veya özellik farkı var mı? (Şimdilik sadece eksiklere odaklanalım, güncelleme riskli olabilir)
                                // İstenirse burada ALTER TABLE ALTER COLUMN eklenebilir.
                            }
                        }
                    }
                }
            }
        }

        static string GenerateCreateTableScript(TableSchema table)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE [{table.Schema}].[{table.Name}] (");

            var colDefinitions = new List<string>();
            foreach (var col in table.Columns)
            {
                colDefinitions.Add($"    [{col.Name}] {GetSqlType(col)} {(col.IsNullable ? "NULL" : "NOT NULL")}");
            }
            
            // Basitlik için Primary Key'i ilk kolon varsaymıyoruz, tam olarak PK bilgisi çekilmedi.
            // Bu script create ederken PK olmadan oluşturabilir, bu bir eksiklik (TODO: PK desteği ekle)

            sb.AppendLine(string.Join(",\n", colDefinitions));
            sb.AppendLine(");");

            return sb.ToString();
        }

        static string GenerateAddColumnScript(TableSchema table, ColumnSchema col)
        {
            var sb = new StringBuilder();
            sb.Append($"ALTER TABLE [{table.Schema}].[{table.Name}] ADD [{col.Name}] {GetSqlType(col)}");

            if (!col.IsNullable)
            {
                // NOT NULL kolon eklenirken varsayılan değer verilmeli
                string defaultValue = "";
                switch (col.DataType.ToLower())
                {
                    case "bit":
                        defaultValue = "DEFAULT 0";
                        break;
                    case "int":
                    case "bigint":
                    case "smallint":
                    case "tinyint":
                    case "decimal":
                    case "numeric":
                    case "float":
                    case "real":
                    case "money":
                        defaultValue = "DEFAULT 0";
                        break;
                    case "datetime":
                    case "date":
                    case "datetime2":
                    case "smalldatetime":
                        defaultValue = "DEFAULT '1900-01-01'";
                        break;
                    case "uniqueidentifier":
                        defaultValue = "DEFAULT '00000000-0000-0000-0000-000000000000'"; 
                        break;
                    default:
                        defaultValue = "DEFAULT ''"; // String tipleri için
                        break;
                }
                sb.Append($" NOT NULL {defaultValue}");
            }
            else
            {
                sb.Append(" NULL");
            }
            sb.Append(";");
            
            return sb.ToString();
        }

        static string GetSqlType(ColumnSchema col)
        {
            switch (col.DataType.ToLower())
            {
                case "nvarchar":
                case "varchar":
                case "char":
                case "nchar":
                case "binary":
                case "varbinary":
                    return $"{col.DataType}({(col.MaxLength == -1 ? "MAX" : col.MaxLength.ToString())})";
                case "decimal":
                case "numeric":
                    return $"{col.DataType}({col.Precision},{col.Scale})";
                default:
                    return col.DataType;
            }
        }

        static void ExecuteSql(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                try 
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Başarılı.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"SQL Hatası: {ex.Message}");
                    Console.WriteLine($"Query: {sql}");
                }
            }
        }
    }

    class TableSchema
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public List<ColumnSchema> Columns { get; set; }
    }

    class ColumnSchema
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public int? MaxLength { get; set; }
        public bool IsNullable { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
    }
}
