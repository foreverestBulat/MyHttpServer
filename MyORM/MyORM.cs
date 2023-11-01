using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;

namespace MyORM
{
    public class MyDataContext : IMyDataContext
    {
        public string ConnectionString;

        public MyDataContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        T IMyDataContext.SelectByID<T>(int id)
        {
            var type = typeof(T);
            var properties = type.GetProperties();
            var tableName = type.Name;

            ConstructorInfo info = type.GetConstructor(new Type[] { });
            object obj = info.Invoke(new object[] { });

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var sqlExpression = $"SELECT * FROM {tableName} WHERE {properties[0].Name} = {id}";

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            if (properties[i].Name == reader.GetName(j))
                            {
                                properties[i].SetValue(obj, reader.GetValue(j));
                                break;
                            }
                        }
                    }
                }

                reader.Close();
                command.ExecuteNonQuery();
            }

            return (T)obj;
        }

        List<T> IMyDataContext.Select<T>()
        {
            List<T> list = new List<T>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var type = typeof(T);
                var properties = type.GetProperties();
                var tableName = type.Name;
                var sqlExpression = $"SELECT * FROM {tableName}";

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                ConstructorInfo info = type.GetConstructor(new Type[] { });

                while (reader.Read())
                {
                    object obj = info.Invoke(new object[] { });
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        for (int j = 0; j < reader.FieldCount; j++)
                        {
                            if (properties[i].Name == reader.GetName(j))
                            {
                                properties[i].SetValue(obj, reader.GetValue(j));
                                break;
                            }
                        }


                    }
                    list.Add((T)obj);
                }
                reader.Close();

                command.ExecuteNonQuery();
            }

            return list;
        }

        void IMyDataContext.Delete<T>(int id)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var type = typeof(T);
                var tableName = type.Name;
                var sqlExpression = $"DELETE {tableName} WHERE {type.GetProperties()[0].Name} = {id}";

                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }

        void IMyDataContext.Update<T>(T row)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var type = row?.GetType();
                var properties = type?.GetProperties();
                var tableName = type?.Name;
                var sqlExpression = $"SELECT * FROM {tableName}";

                SqlCommand command = new SqlCommand(sqlExpression, connection);

                StringBuilder sbUpdate = new StringBuilder($"UPDATE {tableName} SET ");

                SqlDataReader reader = command.ExecuteReader();

                string field = null;
                int id = -1;

                while (reader.Read())
                {
                    if (reader.GetValue(0).Equals(properties?[0].GetValue(row)))
                    {
                        field = reader.GetName(0);
                        id = (int)reader.GetValue(0);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            sbUpdate.Append($"{reader.GetName(i)} = '{properties?[i].GetValue(row)}'");

                            if (i < reader.FieldCount - 1)
                                sbUpdate.Append(',');
                        }

                        break;
                    }
                }
                reader.Close();

                sbUpdate.Append($" WHERE {field} = {id}");

                var sqlExpressionUpdate = sbUpdate.ToString();
                command.CommandText = sqlExpressionUpdate;
                command.ExecuteNonQuery();
            }
        }

        void IMyDataContext.Add<T>(T row)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                var type = row?.GetType();
                var properties = type?.GetProperties();
                var tableName = type?.Name;
                var sqlExpression = $"SELECT * FROM {tableName}";

                SqlCommand command = new SqlCommand(sqlExpression, connection);

                StringBuilder sbInsert = new StringBuilder($"INSERT INTO {tableName} (");
                StringBuilder sbValues = new StringBuilder($"VALUES(");

                SqlDataReader reader = command.ExecuteReader();

                var nameFields = new List<string>();
                var values = new List<object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var name = reader.GetName(i);
                    Console.WriteLine(name);
                    sbInsert.Append($"{name}");

                    if (i < reader.FieldCount - 1)
                    {
                        sbInsert.Append(',');
                    }

                    sbValues.Append($"@{name}");

                    if (i < reader.FieldCount - 1)
                    {
                        sbValues.Append(',');
                    }

                    object value = null;
                    foreach (var property in properties)
                        if (property?.Name.ToLower() == name.ToLower())
                            value = property?.GetValue(row);

                    nameFields.Add(name);
                    values.Add(value);
                }
                reader.Close();

                sbValues.Append(')');

                var sqlExpressionInsert = sbInsert.Append($") {sbValues.ToString()}").ToString();

                command.CommandText = sqlExpressionInsert;

                foreach (var (name, value) in nameFields.Zip(values))
                {
                    SqlParameter param = new SqlParameter($"@{name}", value);
                    command.Parameters.Add(param);
                }

                command.ExecuteNonQuery();
            }
        }
    }
}