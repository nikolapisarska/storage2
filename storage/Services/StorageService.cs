using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using storage.Model;

namespace storage.Services
{
    public class StorageService
    {
        // Ścieżka do pliku storage.json w folderze aplikacji
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage.json");
        
        private readonly List<Product> _graffitiProductsMock;
        private readonly List<Box> _allBoxes;

        public StorageService()
        {

            // lista produktów testowych
            _graffitiProductsMock = new List<Product>
            {
                new Product { Name = "Bompka ładna", CodeOrIdGraffiti = "12345" },
                new Product { Name = "Bompka ładniejsza", CodeOrIdGraffiti = "54321" },
                new Product { Name = "Bompka zwykła taka", CodeOrIdGraffiti = "99999" },
                new Product { Name = "Bompka taka inna bo Szklana Czerwona 10cm", CodeOrIdGraffiti = "2137" },
            new Product { Name = "Trzymak bompek ", CodeOrIdGraffiti = "6767" },
            new Product { Name = "Karton wysyłkowy ", CodeOrIdGraffiti = "KART-B" }
            };
           

            // Wczytanie zapisanych kartonów z pliku przy starcie aplikacji
            _allBoxes = LoadBoxesFromFile();
        }

        // 1. Wyszukiwanie produktu po kodzie z Graffiti
        public Product? FindProduct(string code)
        {
            return _graffitiProductsMock.FirstOrDefault(p => p.CodeOrIdGraffiti == code);
        }

        // 2. Zapisywanie nowego, skompletowanego kartonu do pliku
        public void SaveBox(Box box)
        {
            // Jeśli kod kartonu jest pusty, generujemy go automatycznie
            if (string.IsNullOrWhiteSpace(box.BoxCode))
            {
                box.BoxCode = $"BOX-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            }
            else
            {
                // Opcjonalnie: upewniamy się, że kod nie ma zbędnych spacji
                box.BoxCode = box.BoxCode.Trim();
            }
    
            _allBoxes.Add(box);
    
            // Zapis zaktualizowanej listy do pliku JSON
            SaveBoxesToFile();
        }

        // 3. Szukanie istniejącego kartonu 
        public Box? FindBox(string boxCode)
        {
            return _allBoxes.FirstOrDefault(b => b.BoxCode.Equals(boxCode, StringComparison.OrdinalIgnoreCase));
        }
        public void UpdateBox(Box updatedBox)
        {
            // Szukamy istniejącego kartonu w naszej liście po jego unikalnym BoxCode
            var existingBox = _allBoxes.FirstOrDefault(b => b.BoxCode.Equals(updatedBox.BoxCode, StringComparison.OrdinalIgnoreCase));
    
            if (existingBox != null)
            {
              
                existingBox.Width = updatedBox.Width;
                existingBox.Height = updatedBox.Height;
                existingBox.Length = updatedBox.Length;
                existingBox.Weight = updatedBox.Weight;
        
                // Nadpisujemy listę przedmiotów wewnątrz kartonu
                existingBox.Items = updatedBox.Items;
        
                // Zapisujemy zmiany do pliku storage.json
                SaveBoxesToFile();
            }
        }

        // METODY ZAPISU I ODCZYTU Z PLIKU JSON
   
        private void SaveBoxesToFile()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(_allBoxes, options);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas zapisu do pliku: {ex.Message}");
            }
        }

        private List<Box> LoadBoxesFromFile()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<Box>();
                }

                string jsonString = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return new List<Box>();
                }

                var deserializedBoxes = JsonSerializer.Deserialize<List<Box>>(jsonString);
                return deserializedBoxes ?? new List<Box>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas wczytywania pliku: {ex.Message}");
                return new List<Box>();
            }
        }
    }
}