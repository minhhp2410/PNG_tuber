using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Data;
using System.Reflection;

namespace System
{
    public class JsonDataSet<T> where T : class
    {
        private readonly string _filePath;
        private List<T> _data;
        string _uniquePropertyName;

        public JsonDataSet(string filePath, string uniquePropertyName = "")
        {
            _filePath = filePath;
            _data = LoadData();
            _uniquePropertyName = uniquePropertyName;
        }

        private PropertyInfo GetProperty(string propertyName)
        {
            return typeof(T).GetProperty(propertyName);
        }
        private PropertyInfo[] GetProperties()
        {
            return typeof(T).GetProperties();
        }

        private List<T> LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                }
                else
                {
                    File.WriteAllText(_filePath, JsonConvert.SerializeObject(new List<T>(), Formatting.None));
                }
                return new List<T>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
                return new List<T>();
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_data, Formatting.None);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}");
            }
        }

        private bool IsUnique(T item)
        {
            if (string.IsNullOrEmpty(_uniquePropertyName))
                return true;

            var property = GetProperty(_uniquePropertyName);
            if (property == null)
            {
                MessageBox.Show($"Property {_uniquePropertyName} does not exist on type {typeof(T).Name}");
                return false;
            }

            var newValue = property.GetValue(item);
            return !_data.Any(existing =>
                !ReferenceEquals(existing, item) &&
                Equals(property.GetValue(existing), newValue));
        }

        public void Add(T item)
        {
            var property = GetProperty(_uniquePropertyName);
            if (!IsUnique(item))
            {
                MessageBox.Show($"Item with {_uniquePropertyName} = '{property.GetValue(item)}' already exists.");
                return;
            }
            _data.Add(item);
            SaveData();
        }

        public List<T> GetAll()
        {
            return _data;
        }

        public T GetById(Predicate<T> predicate)
        {
            return _data.Find(predicate);
        }

        public void Update(T item, Predicate<T> predicate)
        {
            int index = _data.FindIndex(predicate);
            if (index != -1)
            {
                _data[index] = item;
                SaveData();
            }
        }

        public void Delete(Predicate<T> predicate)
        {
            try
            {
                List<T> itemsToRemove = _data.FindAll(predicate);
                foreach (var item in itemsToRemove)
                {
                    //get Id & Deleted properties
                    var idProperty = GetProperty("Id");
                    var deletedProperty = GetProperty("Deleted");
                    deletedProperty.SetValue(item, true);
                    //update item in _data using Update method
                    Update(item, x => Equals(idProperty.GetValue(x), idProperty.GetValue(item)));
                }
                //print notification & save data
                MessageBox.Show($"{itemsToRemove.Count} item(s) marked as deleted.");
                SaveData();
            }
            catch (Exception)
            {
                MessageBox.Show("Error deleting item(s).");
            }

        }

        public DataTable ToDataTable()
        {
            //get properties of T
            var properties = GetProperties();
            DataTable table = new DataTable(typeof(T).Name);
            //add columns to DataTable
            foreach (var prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            //add rows to DataTable
            foreach (var item in _data)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
